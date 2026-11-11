namespace AppExampleKafka.API.Documentation;

/// <summary>Payloads sintéticos para experimentar o contrato no Swagger.</summary>
internal static class BankingExamples
{
    public static Dictionary<string, string> Registration => new()
    {
        ["PF"] = """
        {
          "nome": "Ana Teste", "email": "ana@example.com", "password": "SenhaForte123!",
          "cadastro": {
            "tipoPessoa": "PF", "documento": "52998224725", "telefone": "11987654321",
            "endereco": { "logradouro": "Rua de Teste", "numero": "10", "bairro": "Centro", "cidade": "São Paulo", "uf": "SP", "cep": "01001000", "complemento": "Apto 1" },
            "pessoaFisica": { "dataNascimento": "1990-01-01", "nacionalidade": "Brasileira", "estadoCivil": "Solteira", "profissao": "Engenheira", "rendaMensal": 5000.00 }
          }
        }
        """,
        ["PJ"] = """
        {
          "nome": "Empresa Teste", "email": "empresa@example.com", "password": "SenhaForte123!",
          "cadastro": {
            "tipoPessoa": "PJ", "documento": "11222333000181", "telefone": "1133334444",
            "endereco": { "logradouro": "Rua da Empresa", "numero": "20", "bairro": "Centro", "cidade": "São Paulo", "uf": "SP", "cep": "01001000" },
            "pessoaJuridica": {
              "razaoSocial": "Empresa de Teste Ltda", "nomeFantasia": "Empresa Teste", "dataConstituicao": "2020-01-01",
              "naturezaJuridica": "Sociedade limitada", "cnae": "6201501", "faturamentoMensal": 10000.00,
              "representanteLegal": { "nome": "Ana Teste", "cpf": "52998224725", "dataNascimento": "1990-01-01" }
            }
          }
        }
        """
    };

    public const string Login = """
        { "email": "ana@example.com", "password": "SenhaForte123!" }
        """;

    public const string Transfer = """
        {
          "contaOrigemId": "00000000-0000-0000-0000-000000000001",
          "contaDestinoId": "00000000-0000-0000-0000-000000000002",
          "valor": 25.50,
          "chaveIdempotencia": "pedido-2026-001"
        }
        """;
}
