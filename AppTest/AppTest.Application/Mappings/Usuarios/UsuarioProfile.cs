using AppTest.Application.DTOs.Auth;
using AutoMapper;


namespace AppTest.Application.Mappings.Usuarios;

/// <summary>Perfil do AutoMapper para a entidade <see cref="Usuarios"/>.</summary>
public class UsuarioProfile : Profile
{
    public UsuarioProfile()
    {
        CreateMap<Domain.Entities.Usuarios.Usuario, UserDto>()
            .ForCtorParam(nameof(UserDto.Role), opt => opt.MapFrom(src => src.Role.ToString()));
    }
}
