using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace AcuarioWebs.Models;

public partial class AcuarioContext : DbContext
{
    public AcuarioContext()
    {
    }

    public AcuarioContext(DbContextOptions<AcuarioContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Alimentoo> Alimentoos { get; set; }

    public virtual DbSet<Atencione> Atenciones { get; set; }

    public virtual DbSet<Pece> Peces { get; set; }

    public virtual DbSet<Peceraa> Peceraas { get; set; }

    public virtual DbSet<Rol> Rols { get; set; }

    public virtual DbSet<Usuario> Usuarios { get; set; }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Alimentoo>(entity =>
        {
            entity.HasKey(e => e.IdAlimento).HasName("PK__ALIMENTO__DA2BB2F69192EA78");

            entity.ToTable("ALIMENTOO");

            entity.Property(e => e.IdAlimento).HasColumnName("ID_ALIMENTO");
            entity.Property(e => e.NombreAlimento)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("NOMBRE_ALIMENTO");
            entity.Property(e => e.Tipo)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("TIPO");
        });

        modelBuilder.Entity<Atencione>(entity =>
        {
            entity.HasKey(e => e.IdAtencion).HasName("PK__ATENCION__033C0E560932541B");

            entity.ToTable("ATENCIONES");

            entity.Property(e => e.IdAtencion).HasColumnName("ID_ATENCION");
            entity.Property(e => e.Fecha)
                .HasColumnType("datetime")
                .HasColumnName("FECHA");
            entity.Property(e => e.IdAlimento).HasColumnName("ID_ALIMENTO");
            entity.Property(e => e.IdPeces).HasColumnName("ID_PECES");
            entity.Property(e => e.IdUser).HasColumnName("ID_USER");
            entity.Property(e => e.TipoActividad)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("TIPO_ACTIVIDAD");

            entity.HasOne(d => d.IdAlimentoNavigation).WithMany(p => p.Atenciones)
                .HasForeignKey(d => d.IdAlimento)
                .HasConstraintName("FK__ATENCIONE__ID_AL__571DF1D5");

            entity.HasOne(d => d.IdPecesNavigation).WithMany(p => p.Atenciones)
                .HasForeignKey(d => d.IdPeces)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ATENCIONE__ID_PE__5629CD9C");

            entity.HasOne(d => d.IdUserNavigation).WithMany(p => p.Atenciones)
                .HasForeignKey(d => d.IdUser)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ATENCIONE__ID_US__5535A963");
        });

        modelBuilder.Entity<Pece>(entity =>
        {
            entity.HasKey(e => e.IdPeces).HasName("PK__PECES__3522C880E30A2E0F");

            entity.ToTable("PECES");

            entity.Property(e => e.IdPeces).HasColumnName("ID_PECES");
            entity.Property(e => e.Edad).HasColumnName("EDAD");
            entity.Property(e => e.Especie)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("ESPECIE");
            entity.Property(e => e.IdPecera).HasColumnName("ID_PECERA");
            entity.Property(e => e.NombrePez)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("NOMBRE_PEZ");

            entity.HasOne(d => d.IdPeceraNavigation).WithMany(p => p.Peces)
                .HasForeignKey(d => d.IdPecera)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__PECES__ID_PECERA__5070F446");
        });

        modelBuilder.Entity<Peceraa>(entity =>
        {
            entity.HasKey(e => e.IdPecera).HasName("PK__PECERAAS__F2C70AE0653B6F8E");

            entity.ToTable("PECERAAS");

            entity.Property(e => e.IdPecera).HasColumnName("ID_PECERA");
            entity.Property(e => e.Litros)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("LITROS");
            entity.Property(e => e.NombrePecera)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("NOMBRE_PECERA");
            entity.Property(e => e.Ph)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("PH");
            entity.Property(e => e.Temperatura)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("TEMPERATURA");
        });

        modelBuilder.Entity<Rol>(entity =>
        {
            entity.HasKey(e => e.IdRol).HasName("PK__ROL__203B0F687B05A3DD");

            entity.ToTable("ROL");

            entity.Property(e => e.IdRol).HasColumnName("ID_ROL");
            entity.Property(e => e.Rol1)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("ROL");
        });

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasKey(e => e.IdUser).HasName("PK__USUARIO__95F48440D1D0F41B");

            entity.ToTable("USUARIO");

            entity.Property(e => e.IdUser).HasColumnName("ID_USER");
            entity.Property(e => e.Email)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("EMAIL");
            entity.Property(e => e.IdRol).HasColumnName("ID_ROL");
            entity.Property(e => e.Nombre)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("NOMBRE");
            entity.Property(e => e.Pass)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("PASS");

            entity.HasOne(d => d.IdRolNavigation).WithMany(p => p.Usuarios)
                .HasForeignKey(d => d.IdRol)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__USUARIO__ID_ROL__4BAC3F29");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
