using AppTest.Domain.Entities.Categorias;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AppTest.Infraestructure.Context.Configurations.Categorias;

/// <summary>Mapeamento Fluent API da entidade <see cref="Category"/>.</summary>
public class CategoriaConfiguration : IEntityTypeConfiguration<Categoria>
{
    public void Configure(EntityTypeBuilder<Categoria> builder)
    {
        builder.ToTable("categorias");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Nome)
            .IsRequired()
            .HasMaxLength(120);

        builder.HasIndex(c => c.Nome).IsUnique();

        builder.Property(c => c.Descricao)
            .HasMaxLength(500);

        builder.Property(c => c.CriadoEm)
            .IsRequired();
    }
}
