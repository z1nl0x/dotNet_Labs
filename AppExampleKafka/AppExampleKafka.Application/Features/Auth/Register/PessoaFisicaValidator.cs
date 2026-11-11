using AppExampleKafka.Application.DTOs.Usuarios;
using FluentValidation;
using AppExampleKafka.Domain.Entities.Usuarios;

namespace AppExampleKafka.Application.Features.Auth.Register;

public class PessoaFisicaValidator : AbstractValidator<DadosPessoaFisicaDto>
{
    public PessoaFisicaValidator()
    {
        RuleFor(p => p.DataNascimento).Must(CadastroBancarioValidator.Adulto).WithMessage("Titular deve ter entre 18 e 120 anos.");
        RuleFor(p => p.Nacionalidade).NotEmpty().MaximumLength(80);
        RuleFor(p => p.EstadoCivil).NotEmpty().MaximumLength(40);
        RuleFor(p => p.Profissao).NotEmpty().MaximumLength(100);
        RuleFor(p => p.RendaMensal).InclusiveBetween(0, 9999999999999999.99m).Must(v => v == decimal.Round(v, 2));
    }
}
