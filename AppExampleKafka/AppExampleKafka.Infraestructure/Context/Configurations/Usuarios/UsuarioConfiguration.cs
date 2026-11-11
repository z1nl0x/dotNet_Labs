using AppExampleKafka.Domain.Enums;
using AppExampleKafka.Domain.Entities.Usuarios;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AppExampleKafka.Infraestructure.Context.Configurations.Users;

/// <summary>Mapeamento Fluent API da entidade <see cref="Usuarios"/>.</summary>
public class UsuarioConfiguration : IEntityTypeConfiguration<Usuario>
{
    public void Configure(EntityTypeBuilder<Usuario> builder)
    {
        builder.ToTable("usuarios");

        builder.HasKey(u => u.Id);
        builder.OwnsOne(u => u.Cadastro, c =>
        {
            c.Property(x => x.TipoPessoa).HasConversion<string>().HasMaxLength(2);
            c.Property(x => x.Documento).HasMaxLength(14).IsRequired();
            c.HasIndex(x => x.Documento).IsUnique();
            c.Property(x => x.Telefone).HasMaxLength(11).IsRequired();
            c.OwnsOne(x => x.Endereco, e =>
            {
                e.Property(x => x.Logradouro).HasMaxLength(180);
                e.Property(x => x.Numero).HasMaxLength(20);
                e.Property(x => x.Bairro).HasMaxLength(100);
                e.Property(x => x.Cidade).HasMaxLength(100);
                e.Property(x => x.Uf).HasMaxLength(2);
                e.Property(x => x.Cep).HasMaxLength(8);
                e.Property(x => x.Complemento).HasMaxLength(100);
            });
            c.Navigation(x => x.Endereco).IsRequired();
            c.OwnsOne(x => x.PessoaFisica, pf =>
            {
                pf.Property(x => x.Nacionalidade).HasMaxLength(80);
                pf.Property(x => x.EstadoCivil).HasMaxLength(40);
                pf.Property(x => x.Profissao).HasMaxLength(100);
                pf.Property(x => x.RendaMensal).HasPrecision(18, 2);
            });
            c.OwnsOne(x => x.PessoaJuridica, pj =>
            {
                pj.Property(x => x.RazaoSocial).HasMaxLength(180);
                pj.Property(x => x.NomeFantasia).HasMaxLength(180);
                pj.Property(x => x.NaturezaJuridica).HasMaxLength(100);
                pj.Property(x => x.Cnae).HasMaxLength(7);
                pj.Property(x => x.FaturamentoMensal).HasPrecision(18, 2);
                pj.OwnsOne(x => x.RepresentanteLegal, r =>
                {
                    r.Property(x => x.Nome).HasMaxLength(120);
                    r.Property(x => x.Cpf).HasMaxLength(11);
                });
                pj.Navigation(x => x.RepresentanteLegal).IsRequired();
            });
        });

        builder.Property(u => u.Nome)
            .IsRequired()
            .HasMaxLength(120);

        builder.Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(180);

        builder.HasIndex(u => u.Email).IsUnique();

        builder.Property(u => u.PasswordHash)
            .IsRequired();

        // Persiste o enum como string ("User"/"Admin") para legibilidade no banco.
        builder.Property(u => u.Role)
            .IsRequired()
            .HasMaxLength(20)
            .HasConversion<string>();

        builder.Property(u => u.CriadoEm)
            .IsRequired();
        
        builder.Property(u => u.AtualizadoEm)
            .IsRequired();
    }
}
