using System.Reflection;
using System.Text.Json.Nodes;
using AppExampleKafka.Application.DTOs.Auth;
using AppExampleKafka.Application.DTOs.Common;
using AppExampleKafka.Application.DTOs.Contas;
using AppExampleKafka.Application.DTOs.Transacoes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace AppExampleKafka.API.Documentation;

/// <summary>Enriquece o contrato público sem alterar o comportamento dos endpoints.</summary>
public sealed class BankingOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        operation.Responses ??= new OpenApiResponses();
        var path = context.ApiDescription.RelativePath?.Split('?')[0].ToLowerInvariant();
        var method = context.ApiDescription.HttpMethod;
        var anonymous = context.MethodInfo.IsDefined(typeof(AllowAnonymousAttribute));
        operation.Security = anonymous ? [] :
        [new OpenApiSecurityRequirement { [new OpenApiSecuritySchemeReference("Bearer", context.Document)] = [] }];
        operation.OperationId = $"{context.MethodInfo.DeclaringType!.Name.Replace("Controller", "")}_{context.MethodInfo.Name}";
        if (!anonymous) Error("401", "Token JWT ausente, inválido ou expirado. O desafio Bearer pode retornar corpo vazio.", false);

        switch (path, method)
        {
            case ("api/auth/register", "POST"):
                Describe("Registrar titular PF ou PJ", "Cria um usuário com role User e retorna JWT. Escolha o exemplo PF ou PJ: envie somente o bloco correspondente ao tipoPessoa. CPF/CNPJ, telefone e CEP sem máscara; UF e letras do CNPJ em maiúsculas. Datas no formato AAAA-MM-DD; titular/representante entre 18 e 120 anos. Senha de 6 a 100 caracteres. Dados dos exemplos são apenas para teste. O refresh token é enviado no cookie HTTP-only, nunca no JSON. Repetir o cadastro gera conflito.");
                Success("200", typeof(AuthResponse), "Titular registrado; Set-Cookie define refreshToken.");
                Error("400", "Cadastro inválido ou incompleto.");
                Error("409", "E-mail ou documento já cadastrado.");
                Examples(BankingExamples.Registration);
                break;
            case ("api/auth/login", "POST"):
                Describe("Entrar com e-mail e senha", "Retorna accessToken e grava refreshToken em cookie HTTP-only. No botão Authorize, cole somente o JWT, sem o prefixo Bearer. Não compartilhe tokens nem credenciais reais.");
                Success("200", typeof(AuthResponse), "Autenticado; Set-Cookie define refreshToken.");
                Error("400", "E-mail ou senha não informado ou inválido.");
                Error("401", "E-mail ou senha incorretos.");
                Examples(new() { ["Login"] = BankingExamples.Login });
                break;
            case ("api/auth/refresh", "POST"):
                Describe("Renovar sessão pelo cookie", "Sem corpo e sem JWT obrigatório. Requer o cookie refreshToken recebido no login/registro. O navegador envia o cookie na mesma origem; não é possível defini-lo manualmente pelo Swagger. Rotaciona o refresh token e retorna outro accessToken: atualize também o Authorize. Cliente HTTP precisa preservar cookies.");
                Success("200", typeof(AuthResponse), "Tokens renovados; Set-Cookie substitui refreshToken.");
                Error("401", "Cookie ausente, inválido, revogado ou expirado.");
                break;
            case ("api/auth/logout", "POST"):
                Describe("Encerrar sessão", "Requer JWT e usa o cookie refreshToken para revogar a sessão. Remove o cookie. Não invalida imediatamente o access token já emitido, que permanece válido até expirar.");
                break;
            case ("api/auth/me", "GET"):
                Describe("Consultar meu perfil", "Retorna dados pessoais e cadastro completo exclusivamente do usuário do JWT. Nunca retorna senha, hash ou refresh token.");
                Success("200", typeof(MeuPerfilDto), "Perfil do titular autenticado.");
                Error("401", "Token inválido ou usuário autenticado não encontrado. O desafio Bearer pode retornar corpo vazio.", false);
                break;
            case ("api/auth/admin-only", "GET"):
                Describe("Verificar acesso administrativo", "Exige role Admin. O registro público sempre cria User; não aceita promoção por payload.");
                Success("200", typeof(AdminAccessResponse), "Acesso administrativo permitido.");
                Error("403", "JWT válido, mas sem role Admin. A resposta pode ter corpo vazio.", false);
                break;
            case ("api/contas", "POST"):
                Describe("Abrir conta corrente", "Não envie corpo: titular vem do JWT. Cria conta ativa em BRL, saldo zero, agência 0001, número de 12 dígitos e DV gerados pelo servidor. Não aceita saldo inicial ou identificação escolhida pelo cliente. Não existe endpoint de aporte. Identificadores locais ao laboratório.");
                operation.Responses.Remove("200");
                Success("201", typeof(ContaBancariaDto), "Conta criada; Location aponta para a consulta individual.");
                Error("409", "Conflito na geração do número; tente uma nova criação.");
                break;
            case ("api/transacoes", "POST"):
                Describe("Transferir entre contas", "Somente o titular da origem pode transferir. Contas devem ser distintas, ativas e da mesma moeda; valor positivo com até duas casas decimais e saldo suficiente. Contas novas têm saldo zero: o exemplo não realiza aporte. chaveIdempotencia é obrigatória (1–100 caracteres ASCII: letras, números, ponto, sublinhado, dois-pontos ou hífen). Reenvie a mesma chave e payload após timeout/conflito concorrente. Repetição idêntica retorna a transação original com 201; reutilização com conteúdo diferente retorna 409.");
                operation.Responses.Remove("200");
                Success("201", typeof(TransacaoDto), "Transferência criada ou replay idempotente; Location aponta para a transação.");
                Error("400", "GUID, valor ou chave de idempotência inválido.");
                Error("404", "Conta inexistente ou origem não pertence ao titular.");
                Error("409", "Saldo insuficiente, estado incompatível, chave reutilizada com outro conteúdo ou concorrência.");
                Examples(new() { ["Transferencia"] = BankingExamples.Transfer });
                break;
            case ("api/contas/{id}", "GET"):
                Describe("Consultar minha conta por ID", "Apenas o titular pode consultar a conta; recursos de terceiros retornam 404, inclusive para Admin. Agência, número e DV são strings para preservar zeros à esquerda.");
                Success("200", typeof(ContaBancariaDto), "Conta do titular.");
                Error("404", "Conta inexistente ou de outro titular.");
                break;
            case ("api/transacoes/{id}", "GET"):
                Describe("Consultar transferência por ID", "Visível apenas ao titular da origem ou do destino. Recursos não visíveis retornam 404, inclusive para Admin.");
                Success("200", typeof(TransacaoDto), "Transferência acessível ao titular.");
                Error("404", "Transferência inexistente ou não acessível.");
                break;
            case ("api/contas", "GET"):
                Describe("Listar minhas contas", "Lista somente contas do titular do JWT. Paginação começa em 1; tamanho padrão 20, máximo 100. items pode ser vazio.");
                Success("200", typeof(PagedResult<ContaBancariaDto>), "Contas e metadados de paginação.");
                Pagination();
                break;
            case ("api/transacoes", "GET"):
                Describe("Listar meu histórico", "Lista transferências em que o titular participa como origem ou destino. Paginação começa em 1; tamanho padrão 20, máximo 100. items pode ser vazio.");
                Success("200", typeof(PagedResult<TransacaoDto>), "Transferências e metadados de paginação.");
                Pagination();
                break;
        }

        void Describe(string summary, string description)
        {
            operation.Summary = summary;
            operation.Description = description;
        }
        void Success(string status, Type type, string description)
        {
            var response = new OpenApiResponse
            {
                Description = description,
                Content = new Dictionary<string, OpenApiMediaType>
                {
                    ["application/json"] = new() { Schema = context.SchemaGenerator.GenerateSchema(type, context.SchemaRepository) }
                }
            };
            operation.Responses[status] = response;
            if (status == "201") response.Headers = new Dictionary<string, IOpenApiHeader>
            {
                ["Location"] = new OpenApiHeader { Description = "URL da consulta do recurso.", Schema = new OpenApiSchema { Type = JsonSchemaType.String } }
            };
        }
        void Error(string status, string description, bool body = true)
        {
            var response = new OpenApiResponse { Description = description };
            operation.Responses[status] = response;
            if (body) response.Content = new Dictionary<string, OpenApiMediaType>
            {
                ["application/json"] = new()
                {
                    Schema = context.SchemaGenerator.GenerateSchema(typeof(ApiErrorResponse), context.SchemaRepository),
                    Example = new JsonObject { ["status"] = int.Parse(status), ["title"] = ((System.Net.HttpStatusCode)int.Parse(status)).ToString(), ["detail"] = description }
                }
            };
        }
        void Examples(Dictionary<string, string> examples)
        {
            if (operation.RequestBody?.Content is not { } content) return;
            foreach (var media in content.Values)
                media.Examples = examples.ToDictionary(x => x.Key, x => (IOpenApiExample)new OpenApiExample
                {
                    Summary = x.Key, Value = JsonNode.Parse(x.Value)
                });
        }
        void Pagination()
        {
            Error("400", "pageNumber deve estar entre 1 e 1000000; pageSize entre 1 e 100.");
            foreach (var parameter in operation.Parameters ?? [])
                if (parameter.Name is "pageNumber" or "pageSize" && parameter.Schema is OpenApiSchema schema)
                {
                    schema.Minimum = "1";
                    schema.Maximum = parameter.Name == "pageSize" ? "100" : "1000000";
                    parameter.Description = parameter.Name == "pageSize" ? "Itens por página (1–100). Padrão: 20." : "Número da página (1–1000000). Padrão: 1.";
                }
        }
    }
}

/// <summary>Formato retornado pelo middleware para erros da aplicação; falhas de binding podem usar ValidationProblemDetails.</summary>
public record ApiErrorResponse(int Status, string Title, string Detail);
public record AdminAccessResponse(string Message);
