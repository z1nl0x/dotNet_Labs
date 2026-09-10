using AppTest.Domain.Entities.Produtos;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AppTest.Infraestructure.Context.Configurations.Produtos;

/// <summary>Mapeamento Fluent API da entidade <see cref="Produto"/>.</summary>
public class ProdutoConfiguration : IEntityTypeConfiguration<Produto>
{
    public void Configure(EntityTypeBuilder<Produto> builder)
    {
        builder.ToTable("produtos");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Nome)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(p => p.Descricao)
            .HasMaxLength(1000);

        builder.Property(p => p.Preco)
            .IsRequired()
            .HasColumnType("numeric(18,2)");

        builder.Property(p => p.CriadoEm)
            .IsRequired();

        builder.HasIndex(p => p.CategoriaId);

        // Relacionamento com Category: bloqueia a exclusão de categoria com produtos.
        builder.HasOne(p => p.Categoria)
            .WithMany()
            .HasForeignKey(p => p.CategoriaId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
