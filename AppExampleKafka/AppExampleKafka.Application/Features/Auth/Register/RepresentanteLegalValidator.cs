using AppExampleKafka.Application.DTOs.Usuarios;
using FluentValidation;
using AppExampleKafka.Domain.Entities.Usuarios;

namespace AppExampleKafka.Application.Features.Auth.Register;

public class RepresentanteLegalValidator : AbstractValidator<RepresentanteLegalDto>
{
    public RepresentanteLegalValidator()
    {
        RuleFor(p => p.Nome).NotEmpty().MaximumLength(120);
        RuleFor(p => p.Cpf).Must(DocumentoBrasileiro.CpfValido);
        RuleFor(p => p.DataNascimento).Must(CadastroBancarioValidator.Adulto);
    }
}
