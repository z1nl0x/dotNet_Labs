using FluentValidation;

namespace AppExampleKafka.Application.Features.Transacoes.GetList;

public class ListarTransacoesQueryValidator : AbstractValidator<ListarTransacoesQuery>
{
    public ListarTransacoesQueryValidator()
    {
        RuleFor(x => x.PageNumber).InclusiveBetween(1, 1000000);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
    }
}
