using System.Text.RegularExpressions;

namespace AppExampleKafka.Tests;

public class FeatureStructureTests
{
    private static string Root
    {
        get
        {
            var directory = new DirectoryInfo(AppContext.BaseDirectory);
            while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "AppExampleKafka.slnx")))
                directory = directory.Parent;
            return directory?.FullName ?? throw new DirectoryNotFoundException("AppExampleKafka.slnx não encontrado.");
        }
    }

    [Fact]
    public void Features_contem_um_tipo_publico_por_arquivo_sem_DTOs_ou_eventos()
    {
        var violations = new List<string>();
        foreach (var file in Directory.EnumerateFiles(Path.Combine(Root, "AppExampleKafka.Application", "Features"), "*.cs", SearchOption.AllDirectories))
        {
            var types = Regex.Matches(File.ReadAllText(file), @"^public\s+(?:(?:sealed|abstract|static|partial)\s+)*(?:class|record|enum|interface)\s+(\w+)", RegexOptions.Multiline);
            if (types.Count != 1 || types[0].Groups[1].Value != Path.GetFileNameWithoutExtension(file)
                || types.Any(t => t.Groups[1].Value.EndsWith("Dto") || t.Groups[1].Value == "TransacaoRealizadaV1"))
                violations.Add(Path.GetRelativePath(Root, file));
        }
        Assert.True(violations.Count == 0, string.Join(Environment.NewLine, violations));
    }

    [Fact]
    public void Tipos_bancarios_e_configuracao_Kafka_tem_arquivos_proprios()
    {
        string[] expected = [
            "AppExampleKafka.Domain/Entities/Bancario/Transacao.cs",
            "AppExampleKafka.Domain/Entities/Bancario/ContaBancaria.cs",
            "AppExampleKafka.Domain/Entities/Usuarios/CadastroBancario.cs",
            "AppExampleKafka.Domain/Enums/TipoPessoa.cs",
            "AppExampleKafka.Infraestructure/Messaging/KafkaEventoPublisher.cs",
            "AppExampleKafka.Domain/Entities/Bancario/OutboxMensagem.cs",
            "AppExampleKafka.Domain/Entities/Usuarios/Endereco.cs",
            "AppExampleKafka.Domain/Entities/Usuarios/DadosPessoaFisica.cs",
            "AppExampleKafka.Domain/Entities/Usuarios/DadosPessoaJuridica.cs",
            "AppExampleKafka.Domain/Entities/Usuarios/RepresentanteLegal.cs",
            "AppExampleKafka.Infraestructure/Messaging/KafkaOptions.cs",
            "AppExampleKafka.Application/Contracts/Events/TransacaoRealizadaV1.cs"
        ];
        Assert.All(expected, relative =>
        {
            var file = Path.Combine(Root, relative);
            Assert.True(File.Exists(file), relative);
            var declarations = Regex.Matches(File.ReadAllText(file), @"^public\s+(?:(?:sealed|abstract|static|partial)\s+)*(?:class|record|enum|interface)\s+(\w+)", RegexOptions.Multiline);
            Assert.Single(declarations);
            Assert.Equal(Path.GetFileNameWithoutExtension(file), declarations[0].Groups[1].Value);
        });
    }

    [Theory]
    [InlineData("Contas", "CriarContaCommand", "ObterContaQuery", "ListarContasQuery")]
    [InlineData("Transacoes", "CriarTransacaoCommand", "ObterTransacaoQuery", "ListarTransacoesQuery")]
    public void Casos_de_uso_seguem_pastas_do_projeto(string entity, string create, string get, string list)
    {
        foreach (var (folder, name) in new[] { ("Create", create), ("GetById", get), ("GetList", list) })
        {
            var path = Path.Combine(Root, "AppExampleKafka.Application", "Features", entity, folder);
            Assert.True(File.Exists(Path.Combine(path, name + ".cs")), path);
            Assert.True(File.Exists(Path.Combine(path, name + "Handler.cs")), path);
        }
    }
}
