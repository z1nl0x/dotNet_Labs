namespace AppTest.Application.DTOs.Common;

/// <summary>
/// Envelope genérico para respostas paginadas, com os metadados de navegação.
/// </summary>
public record PagedResult<T>(
    IReadOnlyList<T> Items,
    int PageNumber,
    int PageSize,
    int TotalCount)
{
    /// <summary>Total de páginas considerando o tamanho de página informado.</summary>
    public int TotalPages => PageSize > 0 ? (int)Math.Ceiling(TotalCount / (double)PageSize) : 0;

    public bool HasPreviousPage => PageNumber > 1;

    public bool HasNextPage => PageNumber < TotalPages;
}
