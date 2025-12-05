using Microsoft.EntityFrameworkCore;
using SistemaGestionDocumentos.API.Data;
using SistemaGestionDocumentos.API.Utils;

var constructorAplicacion = WebApplication.CreateBuilder(args);

// ================== CONFIGURAR SERVICIOS ==================

// Agregar DbContext con SQL Server
constructorAplicacion.Services.AddDbContext<SistemaGestionDocumentosDbContext>(opcionesConfiguración =>
    opcionesConfiguración.UseSqlServer(
        constructorAplicacion.Configuration.GetConnectionString("ConexionBaseDatosPrincipal")
    )
);

// Agregar controladores
constructorAplicacion.Services.AddControllers();

// Agregar Swagger para documentación de API
constructorAplicacion.Services.AddEndpointsApiExplorer();
constructorAplicacion.Services.AddSwaggerGen(c =>
{
    // El filter ya no es necesario con el modelo DTO SubirDocumentoRequest
});

// Agregar CORS para permitir peticiones desde React
constructorAplicacion.Services.AddCors(opcionesConfiguracionCors =>
{
    opcionesConfiguracionCors.AddPolicy("PoliticaPermisivaCors", constructor =>
    {
        constructor
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

// ================== CONSTRUIR LA APLICACIÓN ==================

var aplicacion = constructorAplicacion.Build();

// ================== CONFIGURAR EL PIPELINE HTTP ==================

if (aplicacion.Environment.IsDevelopment())
{
    aplicacion.UseSwagger();
    aplicacion.UseSwaggerUI();
    aplicacion.UseDeveloperExceptionPage();
}

// Redirigir HTTP a HTTPS
aplicacion.UseHttpsRedirection();

// Usar CORS
aplicacion.UseCors("PoliticaPermisivaCors");

// Usar autenticación y autorización
aplicacion.UseAuthentication();
aplicacion.UseAuthorization();

// Mapear controladores
aplicacion.MapControllers();

// Ejecutar la aplicación
aplicacion.Run();
