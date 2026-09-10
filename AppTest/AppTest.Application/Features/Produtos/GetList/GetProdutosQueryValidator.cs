using AppTest.Application.Common.Models;
using FluentValidation;

namespace AppTest.Application.Features.Produtos.GetList;

public class GetProdutosQueryValidator : AbstractValidator<GetProdutosQuery>
{
    public GetProdutosQueryValidator()
    {
        RuleFor(x => x.Nome)
            .MaximumLength(150);

        RuleFor(x => x.PageNumber)
            .GreaterThanOrEqualTo(1).WithMessage("O número da página deve ser maior ou igual a 1.");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, PaginationDefaults.MaxPageSize)
            .WithMessage($"O tamanho da página deve estar entre 1 e {PaginationDefaults.MaxPageSize}.");
    }
}
