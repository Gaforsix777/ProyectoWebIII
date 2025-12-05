using Microsoft.EntityFrameworkCore;
using SistemaGestionDocumentos.API.Models;

namespace SistemaGestionDocumentos.API.Data
{
    public class SistemaGestionDocumentosDbContext : DbContext
    {
        public SistemaGestionDocumentosDbContext(DbContextOptions<SistemaGestionDocumentosDbContext> opciones)
            : base(opciones)
        {
        }

        public DbSet<Usuario> UsuariosDelSistema { get; set; }
        public DbSet<Documento> DocumentosDelSistema { get; set; }
        public DbSet<VersionDocumento> VersionesDelSistema { get; set; }
        public DbSet<WorkflowAprobacion> WorkflowAprobacionDelSistema { get; set; }
        public DbSet<AuditoriaAccion> AuditoriaAccionDelSistema { get; set; }
        public DbSet<NotificacionPendiente> NotificacionesPendientesDelSistema { get; set; }

       protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    base.OnModelCreating(modelBuilder);

    // Configurar clave primaria explícitamente
    modelBuilder.Entity<Usuario>()
        .HasKey(u => u.IdentificadorUsuario);

    modelBuilder.Entity<Documento>()
        .HasKey(d => d.IdentificadorDocumento);

    modelBuilder.Entity<VersionDocumento>()
        .HasKey(v => v.IdentificadorVersionDocumento);

    modelBuilder.Entity<WorkflowAprobacion>()
        .HasKey(w => w.IdentificadorRegistroAprobacion);

    modelBuilder.Entity<AuditoriaAccion>()
        .HasKey(a => a.IdentificadorRegistroAuditoria);

    modelBuilder.Entity<NotificacionPendiente>()
        .HasKey(n => n.IdentificadorNotificacionPendiente);

    // ===== CONFIGURACIÓN DE RELACIONES =====

    modelBuilder.Entity<Documento>()
        .HasOne(d => d.UsuarioPropietarioDelDocumento)
        .WithMany(u => u.DocumentosDelUsuario)
        .HasForeignKey(d => d.IdentificadorUsuarioPropietarioDelDocumento)
        .OnDelete(DeleteBehavior.Cascade);

    modelBuilder.Entity<VersionDocumento>()
        .HasOne(v => v.DocumentoAlQuePerteneceLaVersion)
        .WithMany(d => d.VersionesDelDocumento)
        .HasForeignKey(v => v.IdentificadorDocumentoPerteneceVersion)
        .OnDelete(DeleteBehavior.Cascade);

    modelBuilder.Entity<WorkflowAprobacion>()
        .HasOne(w => w.DocumentoEnAprobacion)
        .WithMany(d => d.FlujosAprobacionDelDocumento)
        .HasForeignKey(w => w.IdentificadorDocumentoAprobar)
        .OnDelete(DeleteBehavior.Cascade);

    modelBuilder.Entity<WorkflowAprobacion>()
        .HasOne(w => w.UsuarioQueAprueba)
        .WithMany(u => u.AprobacionesDelUsuario)
        .HasForeignKey(w => w.IdentificadorUsuarioAprobadorDelDocumento)
        .OnDelete(DeleteBehavior.NoAction);

    modelBuilder.Entity<AuditoriaAccion>()
        .HasOne(a => a.UsuarioQueRealiza)
        .WithMany(u => u.AccionesAuditoriasDelUsuario)
        .HasForeignKey(a => a.IdentificadorUsuarioQueRealizaAccion)
        .OnDelete(DeleteBehavior.NoAction);

    modelBuilder.Entity<NotificacionPendiente>()
        .HasOne(n => n.UsuarioReceptor)
        .WithMany(u => u.NotificacionesDelUsuario)
        .HasForeignKey(n => n.IdentificadorUsuarioReceptorNotificacion)
        .OnDelete(DeleteBehavior.NoAction);

    modelBuilder.Entity<NotificacionPendiente>()
        .HasOne(n => n.DocumentoRelacionado)
        .WithMany(d => d.NotificacionesDelDocumento)
        .HasForeignKey(n => n.IdentificadorDocumentoRelacionadoNotificacion)
        .OnDelete(DeleteBehavior.SetNull);
}
    }
}