using AppTest.Application.DTOs.Categories;
using AppTest.Domain.Entities;
using AppTest.Domain.Entities.Categorias;
using AutoMapper;

namespace AppTest.Application.Mappings.Categorias;

/// <summary>Perfil do AutoMapper para a entidade <see cref="Category"/>.</summary>
public class CategoriaProfile : Profile
{
    public CategoriaProfile()
    {
        CreateMap<Categoria, CategoriaDto>();
    }
}
