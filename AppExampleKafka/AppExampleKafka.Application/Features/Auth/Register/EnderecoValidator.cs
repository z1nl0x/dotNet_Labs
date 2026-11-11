using AppExampleKafka.Application.DTOs.Usuarios;
using FluentValidation;
using AppExampleKafka.Domain.Entities.Usuarios;

namespace AppExampleKafka.Application.Features.Auth.Register;

public class EnderecoValidator : AbstractValidator<EnderecoDto>
{
    public EnderecoValidator()
    {
        RuleFor(e => e.Logradouro).NotEmpty().MaximumLength(180);
        RuleFor(e => e.Numero).NotEmpty().MaximumLength(20);
        RuleFor(e => e.Bairro).NotEmpty().MaximumLength(100);
        RuleFor(e => e.Cidade).NotEmpty().MaximumLength(100);
        RuleFor(e => e.Cep).NotEmpty().Matches("^[0-9]{8}$");
        RuleFor(e => e.Uf).NotEmpty().Must(uf => "AC AL AP AM BA CE DF ES GO MA MT MS MG PA PB PR PE PI RJ RN RS RO RR SC SP SE TO".Split(' ').Contains(uf));
        RuleFor(e => e.Complemento).MaximumLength(100);
    }
}
