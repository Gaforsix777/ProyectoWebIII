using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection;

namespace SistemaGestionDocumentos.API.Utils
{
    /// <summary>
    /// Configuración para que Swagger maneje correctamente IFormFile y parámetros FromForm
    /// </summary>
    public class FileUploadOperation : IOperationFilter
    {
        public void Apply(OpenApiOperation operacion, OperationFilterContext contexto)
        {
            var tieneFormFile = contexto.ApiDescription.ActionDescriptor
                .Parameters.Any(p => p.ParameterType == typeof(IFormFile));

            if (tieneFormFile)
            {
                operacion.RequestBody = new OpenApiRequestBody()
                {
                    Content = new Dictionary<string, OpenApiMediaType>
                    {
                        {
                            "multipart/form-data",
                            new OpenApiMediaType()
                            {
                                Schema = new OpenApiSchema()
                                {
                                    Type = "object",
                                    Properties = new Dictionary<string, OpenApiSchema>()
                                    {
                                        { "archivo", new OpenApiSchema() { Type = "string", Format = "binary" } },
                                        { "nombreDescriptivo", new OpenApiSchema() { Type = "string" } },
                                        { "descripcion", new OpenApiSchema() { Type = "string" } },
                                        { "usuarioId", new OpenApiSchema() { Type = "integer" } }
                                    },
                                    Required = new HashSet<string> { "archivo", "nombreDescriptivo", "usuarioId" }
                                }
                            }
                        }
                    }
                };
            }
        }
    }
}
