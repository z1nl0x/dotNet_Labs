using FluentValidation;

namespace AppExampleKafka.Application.Features.Transacoes.Create;

public class CriarTransacaoCommandValidator : AbstractValidator<CriarTransacaoCommand>
{
    public CriarTransacaoCommandValidator()
    {
        RuleFor(x => x.ContaOrigemId).NotEmpty();
        RuleFor(x => x.ContaDestinoId).NotEmpty().NotEqual(x => x.ContaOrigemId);
        RuleFor(x => x.Valor).GreaterThan(0).LessThanOrEqualTo(9999999999999999.99m)
            .Must(v => v == decimal.Round(v, 2)).WithMessage("Valor deve ter até duas casas decimais.");
        RuleFor(x => x.ChaveIdempotencia).NotEmpty().MaximumLength(100).Matches("^[A-Za-z0-9._:-]+$");
    }
}
