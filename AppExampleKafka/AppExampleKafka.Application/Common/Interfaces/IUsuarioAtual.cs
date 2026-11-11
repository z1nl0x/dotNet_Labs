namespace AppExampleKafka.Application.Common.Interfaces;

/// <summary>Identidade autenticada; nunca recebida no corpo da requisição.</summary>
public interface IUsuarioAtual
{
    Guid Id { get; }
}
