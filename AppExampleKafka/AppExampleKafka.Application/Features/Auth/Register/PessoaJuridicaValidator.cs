using AppExampleKafka.Application.DTOs.Usuarios;
using FluentValidation;
using AppExampleKafka.Domain.Entities.Usuarios;

namespace AppExampleKafka.Application.Features.Auth.Register;

public class PessoaJuridicaValidator : AbstractValidator<DadosPessoaJuridicaDto>
{
    public PessoaJuridicaValidator()
    {
        RuleFor(p => p.RazaoSocial).NotEmpty().MaximumLength(180);
        RuleFor(p => p.NomeFantasia).NotEmpty().MaximumLength(180);
        RuleFor(p => p.DataConstituicao).Must(d => d >= new DateOnly(1800, 1, 1) && d <= DateOnly.FromDateTime(DateTime.UtcNow));
        RuleFor(p => p.NaturezaJuridica).NotEmpty().MaximumLength(100);
        RuleFor(p => p.Cnae).NotEmpty().Matches("^[0-9]{7}$");
        RuleFor(p => p.FaturamentoMensal).InclusiveBetween(0, 9999999999999999.99m).Must(v => v == decimal.Round(v, 2));
        RuleFor(p => p.RepresentanteLegal).NotNull().SetValidator(new RepresentanteLegalValidator());
    }
}
