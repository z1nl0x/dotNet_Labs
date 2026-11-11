using AppExampleKafka.Domain.Enums;
using AppExampleKafka.Application.DTOs.Usuarios;
using System.Text.Json;
using AppExampleKafka.Application.Features.Auth.Register;

using AppExampleKafka.Domain.Entities.Usuarios;

namespace AppExampleKafka.Tests;

public class OnboardingTests
{
    public static CadastroBancarioDto PF => new(TipoPessoa.PF, "52998224725", "11987654321",
        new EnderecoDto("Rua Um", "10", "Centro", "São Paulo", "SP", "01001000"),
        new DadosPessoaFisicaDto(new DateOnly(1990, 1, 1), "Brasileira", "Solteira", "Engenheira", 5000));
    public static CadastroBancarioDto PJ => new(TipoPessoa.PJ, "11222333000181", "1133334444", PF.Endereco,
        PessoaJuridica: new DadosPessoaJuridicaDto("Empresa Ltda", "Empresa", new DateOnly(2020, 1, 1), "Limitada", "6201501", 10000,
            new RepresentanteLegalDto("Ana", "52998224725", new DateOnly(1990, 1, 1))));

    [Theory]
    [InlineData("cpf")]
    [InlineData("cnpj")]
    [InlineData("tipo")]
    [InlineData("endereco")]
    [InlineData("telefone")]
    [InlineData("renda")]
    [InlineData("nascimento")]
    [InlineData("constituicao")]
    [InlineData("representante")]
    [InlineData("mistura")]
    public void Cadastro_invalido_e_rejeitado(string cenario)
    {
        var cadastro = cenario switch
        {
            "cpf" => PF with { Documento = "11111111111" },
            "cnpj" => PJ with { Documento = "11222333000182" },
            "tipo" => PF with { TipoPessoa = (TipoPessoa)99 },
            "endereco" => PF with { Endereco = PF.Endereco with { Cep = "abc", Uf = "XX", Cidade = "" } },
            "telefone" => PF with { Telefone = "123" },
            "renda" => PF with { PessoaFisica = PF.PessoaFisica! with { RendaMensal = -1 } },
            "nascimento" => PF with { PessoaFisica = PF.PessoaFisica! with { DataNascimento = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)) } },
            "constituicao" => PJ with { PessoaJuridica = PJ.PessoaJuridica! with { DataConstituicao = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)) } },
            "representante" => PJ with { PessoaJuridica = PJ.PessoaJuridica! with { RepresentanteLegal = new("Ana", "11111111111", new(1990, 1, 1)) } },
            _ => PF with { PessoaJuridica = PJ.PessoaJuridica }
        };
        var resultado = new RegisterCommandValidator().Validate(new RegisterCommand("Ana", "ana@example.com", "SenhaForte123!", cadastro));
        Assert.False(resultado.IsValid);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Registro_persiste_PF_ou_PJ_mantem_auth_e_role_User(bool pj)
    {
        await using var f = new BankingFixture();
        var cadastro = pj ? PJ : PF;
        var result = await f.Sender.Send(new RegisterCommand("Carla", " CARLA@example.com ", "SenhaForte123!", cadastro));
        Assert.Equal("User", result.Response.User.Role);
        Assert.NotEmpty(result.Response.AccessToken);
        f.Db.ChangeTracker.Clear();
        var stored = await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.SingleAsync(f.Db.Users, u => u.Id == result.Response.User.Id);
        Assert.Equal("carla@example.com", stored.Email);
        Assert.Equal(cadastro, JsonSerializer.Deserialize<CadastroBancarioDto>(JsonSerializer.Serialize(stored.Cadastro)));
        Assert.True(BCrypt.Net.BCrypt.Verify("SenhaForte123!", stored.PasswordHash));
        Assert.Single(f.Db.RefreshTokens);
    }

    [Fact]
    public async Task Documento_ja_cadastrado_retorna_conflito()
    {
        await using var f = new BankingFixture();
        await f.Sender.Send(new RegisterCommand("Carla", "carla@example.com", "SenhaForte123!", PF));
        await Assert.ThrowsAsync<AppExampleKafka.Application.Common.Exceptions.ConflictException>(() => f.Sender.Send(new RegisterCommand("Outra", "outra@example.com", "SenhaForte123!", PF)));
    }

    [Theory]
    [InlineData("11222333000181", true)]
    [InlineData("12ABC34501DE35", true)]
    [InlineData("12ABC34501DE36", false)]
    [InlineData("12abc34501de35", false)]
    [InlineData("00000000000000", false)]
    [InlineData(null, false)]
    public void Cnpj_aceita_formatos_numerico_e_alfanumerico_sem_normalizar(string? documento, bool valido) =>
        Assert.Equal(valido, DocumentoBrasileiro.CnpjValido(documento));

    [Fact]
    public void Registro_exige_cadastro_bancario()
    {
        var resultado = new RegisterCommandValidator().Validate(new RegisterCommand("Ana", "ana@example.com", "SenhaForte123!"));
        Assert.False(resultado.IsValid);
        Assert.Contains(resultado.Errors, e => e.PropertyName == "Cadastro");
    }
}
