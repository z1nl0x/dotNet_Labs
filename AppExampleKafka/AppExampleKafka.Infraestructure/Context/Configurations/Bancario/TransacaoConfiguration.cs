using AppExampleKafka.Domain.Entities.Bancario;
using AppExampleKafka.Domain.Entities.Usuarios;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AppExampleKafka.Infraestructure.Context.Configurations.Bancario;

public class TransacaoConfiguration : IEntityTypeConfiguration<Transacao>
{
    public void Configure(EntityTypeBuilder<Transacao> b)
    {
        b.ToTable("transacoes", t => t.HasCheckConstraint("ck_transacao_valor", "\"Valor\" > 0 AND \"ContaOrigemId\" <> \"ContaDestinoId\""));
        b.HasKey(t => t.Id);
        b.Property(t => t.Valor).HasPrecision(18, 2);
        b.Property(t => t.Moeda).HasMaxLength(3);
        b.Property(t => t.ChaveIdempotencia).HasMaxLength(100).IsRequired();
        b.HasIndex(t => new { t.UsuarioId, t.ChaveIdempotencia }).IsUnique();
        b.HasOne<Usuario>().WithMany().HasForeignKey(t => t.UsuarioId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<ContaBancaria>().WithMany().HasForeignKey(t => t.ContaOrigemId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<ContaBancaria>().WithMany().HasForeignKey(t => t.ContaDestinoId).OnDelete(DeleteBehavior.Restrict);
    }
}
