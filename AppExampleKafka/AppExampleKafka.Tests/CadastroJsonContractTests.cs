using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using AppExampleKafka.Application.DTOs.Auth;

namespace AppExampleKafka.Tests;

public class CadastroJsonContractTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Registro_e_perfil_preservam_contrato_JSON_externo_PF_e_PJ(bool pj)
    {
        // Literais independentes dos tipos C#: capturam renomes acidentais de propriedades e enums.
        var endereco = """
            {"logradouro":"Rua Um","numero":"10","bairro":"Centro","cidade":"São Paulo","uf":"SP","cep":"01001000","complemento":null}
            """;
        var pessoa = pj
            ? """
              "pessoaFisica":null,"pessoaJuridica":{"razaoSocial":"Empresa Ltda","nomeFantasia":"Empresa","dataConstituicao":"2020-01-01","naturezaJuridica":"Limitada","cnae":"6201501","faturamentoMensal":10000,"representanteLegal":{"nome":"Ana","cpf":"52998224725","dataNascimento":"1990-01-01"}}
              """
            : """
              "pessoaFisica":{"dataNascimento":"1990-01-01","nacionalidade":"Brasileira","estadoCivil":"Solteira","profissao":"Engenheira","rendaMensal":5000},"pessoaJuridica":null
              """;
        var cadastro = $$"""
            {"tipoPessoa":"{{(pj ? "PJ" : "PF")}}","documento":"{{(pj ? "11222333000181" : "52998224725")}}","telefone":"11987654321","endereco":{{endereco}},{{pessoa}}}
            """;
        var payload = $$"""
            {"nome":"Ana","email":"ana@example.com","password":"SenhaForte123!","cadastro":{{cadastro}}}
            """;
        using var factory = new ApiFactory();
        using var client = factory.CreateClient(new() { BaseAddress = new Uri("https://localhost"), AllowAutoRedirect = false });
        using var request = new StringContent(payload, Encoding.UTF8, "application/json");
        using var response = await client.PostAsync("/api/auth/register", request);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var auth = (await response.Content.ReadFromJsonAsync<AuthResponse>())!;
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);
        var profile = await client.GetFromJsonAsync<JsonElement>("/api/auth/me");
        using var expected = JsonDocument.Parse(cadastro);
        Assert.True(JsonElement.DeepEquals(expected.RootElement, profile.GetProperty("cadastro")), profile.ToString());
        Assert.Equal("User", profile.GetProperty("role").GetString());
        Assert.Equal(["cadastro", "email", "id", "nome", "role"], profile.EnumerateObject().Select(p => p.Name).Order().ToArray());
    }
}
