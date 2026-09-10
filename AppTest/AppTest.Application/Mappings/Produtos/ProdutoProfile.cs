using AppTest.Application.DTOs.Produtos;
using AppTest.Domain.Entities.Produtos;
using AutoMapper;

namespace AppTest.Application.Mappings;

/// <summary>Perfil do AutoMapper para a entidade <see cref="Produto"/>.</summary>
public class ProdutoProfile : Profile
{
    public ProdutoProfile()
    {
        CreateMap<Produto, ProdutoDto>()
            .ForCtorParam(
                nameof(ProdutoDto.NomeCategoria),
                opt => opt.MapFrom(src => src.Categoria != null ? src.Categoria.Nome : null));
    }
}
