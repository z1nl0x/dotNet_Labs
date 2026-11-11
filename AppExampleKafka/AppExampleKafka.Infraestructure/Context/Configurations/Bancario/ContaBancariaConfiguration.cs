using AppExampleKafka.Domain.Entities.Bancario;
using AppExampleKafka.Domain.Entities.Usuarios;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AppExampleKafka.Infraestructure.Context.Configurations.Bancario;

public class ContaBancariaConfiguration : IEntityTypeConfiguration<ContaBancaria>
{
    public void Configure(EntityTypeBuilder<ContaBancaria> b)
    {
        b.ToTable("contas_bancarias", t => t.HasCheckConstraint("ck_conta_saldo", "\"Saldo\" >= 0"));
        b.HasKey(c => c.Id);
        b.Property(c => c.Saldo).HasPrecision(18, 2).IsConcurrencyToken();
        b.Property(c => c.Moeda).HasMaxLength(3);
        b.Property(c => c.Agencia).HasMaxLength(4).IsRequired();
        b.Property(c => c.Numero).HasMaxLength(12).IsRequired();
        b.Property(c => c.DigitoVerificador).HasMaxLength(1).IsRequired();
        b.Property(c => c.TipoConta).HasMaxLength(20).IsRequired();
        b.HasIndex(c => c.Numero).IsUnique();
        b.HasOne<Usuario>().WithMany().HasForeignKey(c => c.UsuarioId).OnDelete(DeleteBehavior.Restrict);
    }
}
