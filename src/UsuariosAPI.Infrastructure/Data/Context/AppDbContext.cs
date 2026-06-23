using Microsoft.EntityFrameworkCore;
using UsuariosAPI.Domain.Entities;
using UsuariosAPI.Domain.Enums;

namespace UsuariosAPI.Infrastructure.Data.Context;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Usuario> Usuarios { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.ToTable("usuarios");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                .HasColumnName("id")
                .ValueGeneratedOnAdd();

            entity.Property(e => e.Nome)
                .HasColumnName("nome")
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(e => e.Sobrenome)
                .HasColumnName("sobrenome")
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(e => e.Email)
                .HasColumnName("email")
                .HasMaxLength(150)
                .IsRequired();

            entity.HasIndex(e => e.Email)
                .IsUnique()
                .HasDatabaseName("idx_usuarios_email");

            entity.Property(e => e.Genero)
                .HasColumnName("genero")
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsRequired();

            entity.Property(e => e.DataNascimento)
                .HasColumnName("data_nascimento")
                .HasColumnType("date")
                .IsRequired(false);

            entity.Property(e => e.CriadoEm)
                .HasColumnName("criado_em")
                .HasColumnType("datetime2")
                .IsRequired();

            entity.Property(e => e.AtualizadoEm)
                .HasColumnName("atualizado_em")
                .HasColumnType("datetime2")
                .IsRequired();
        });
    }
}
