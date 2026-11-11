namespace AppExampleKafka.Domain.Entities.Usuarios;

/// <summary>Validação local dos dígitos; não comprova identidade nem situação cadastral.</summary>
public static class DocumentoBrasileiro
{
    public static bool CpfValido(string? documento)
    {
        if (documento is null || documento.Length != 11 || documento.Any(c => c is < '0' or > '9') || documento.Distinct().Count() == 1) return false;
        for (var tamanho = 9; tamanho <= 10; tamanho++)
        {
            var soma = 0;
            for (var i = 0; i < tamanho; i++) soma += (documento[i] - '0') * (tamanho + 1 - i);
            var resto = soma % 11;
            if (documento[tamanho] - '0' != (resto < 2 ? 0 : 11 - resto)) return false;
        }
        return true;
    }

    public static bool CnpjValido(string? documento)
    {
        if (documento is null || documento.Length != 14 || documento.Distinct().Count() == 1) return false;
        if (documento.Take(12).Any(c => !(c is >= '0' and <= '9' or >= 'A' and <= 'Z')) || documento.Skip(12).Any(c => c is < '0' or > '9')) return false;
        // Receita Federal: caracteres em ASCII menos 48; compatível com CNPJ numérico e alfanumérico.
        for (var tamanho = 12; tamanho <= 13; tamanho++)
        {
            var soma = 0;
            var peso = 2;
            for (var i = tamanho - 1; i >= 0; i--)
            {
                soma += (documento[i] - 48) * peso;
                peso = peso == 9 ? 2 : peso + 1;
            }
            var resto = soma % 11;
            if (documento[tamanho] - '0' != (resto < 2 ? 0 : 11 - resto)) return false;
        }
        return true;
    }
}
