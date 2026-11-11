using FluentValidation;

namespace AppExampleKafka.Application.Features.Contas.GetList;

public class ListarContasQueryValidator : AbstractValidator<ListarContasQuery>
{
    public ListarContasQueryValidator()
    {
        RuleFor(x => x.PageNumber).InclusiveBetween(1, 1000000);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
    }
}
