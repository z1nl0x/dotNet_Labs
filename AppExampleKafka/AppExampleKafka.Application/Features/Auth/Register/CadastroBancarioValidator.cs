using AppExampleKafka.Domain.Enums;
using AppExampleKafka.Application.DTOs.Usuarios;
using FluentValidation;
using AppExampleKafka.Domain.Entities.Usuarios;

namespace AppExampleKafka.Application.Features.Auth.Register;

public class CadastroBancarioValidator : AbstractValidator<CadastroBancarioDto>
{
    public CadastroBancarioValidator()
    {
        RuleFor(c => c.TipoPessoa).IsInEnum();
        RuleFor(c => c.Telefone).NotEmpty().Matches("^[0-9]{10,11}$");
        RuleFor(c => c.Endereco).NotNull().SetValidator(new EnderecoValidator());
        When(c => c.TipoPessoa == TipoPessoa.PF, () =>
        {
            RuleFor(c => c.Documento).Must(DocumentoBrasileiro.CpfValido).WithMessage("CPF inválido; informe 11 dígitos sem máscara.");
            RuleFor(c => c.PessoaFisica).NotNull().SetValidator(new PessoaFisicaValidator()!);
            RuleFor(c => c.PessoaJuridica).Null();
        });
        When(c => c.TipoPessoa == TipoPessoa.PJ, () =>
        {
            RuleFor(c => c.Documento).Must(DocumentoBrasileiro.CnpjValido).WithMessage("CNPJ inválido; informe 14 caracteres sem máscara, letras maiúsculas.");
            RuleFor(c => c.PessoaJuridica).NotNull().SetValidator(new PessoaJuridicaValidator()!);
            RuleFor(c => c.PessoaFisica).Null();
        });
    }
    internal static bool Adulto(DateOnly data) => data <= DateOnly.FromDateTime(DateTime.UtcNow).AddYears(-18) && data >= DateOnly.FromDateTime(DateTime.UtcNow).AddYears(-120);
}
