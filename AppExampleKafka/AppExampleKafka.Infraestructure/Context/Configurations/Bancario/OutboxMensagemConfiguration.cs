using AppExampleKafka.Domain.Entities.Bancario;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AppExampleKafka.Infraestructure.Context.Configurations.Bancario;

public class OutboxMensagemConfiguration : IEntityTypeConfiguration<OutboxMensagem>
{
    public void Configure(EntityTypeBuilder<OutboxMensagem> b)
    {
        b.ToTable("outbox_mensagens");
        b.HasKey(e => e.Id);
        b.Property(e => e.Payload).IsRequired();
        b.HasIndex(e => new { e.PublicadoEm, e.CriadoEm });
        b.HasOne<Transacao>().WithMany().HasForeignKey(e => e.TransacaoId).OnDelete(DeleteBehavior.Restrict);
    }
}
