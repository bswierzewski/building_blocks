using BuildingBlocks.Application.Models;
using BuildingBlocks.Extensions;
using MediatR;

namespace BuildingBlocks.Application.Queries.EnumValues;

/// <summary>
/// Handles retrieval of enum values by processing GetListEnumValuesQuery requests.
/// </summary>
public class GetListEnumValuesHandler : IRequestHandler<GetListEnumValuesQuery, Result<IEnumerable<EnumValueDto>>>
{
    /// <summary>
    /// Retrieves all values for the specified enum type.
    /// </summary>
    /// <param name="request">Query request containing enum type.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Collection of enum values with their metadata.</returns>
    public Task<Result<IEnumerable<EnumValueDto>>> Handle(GetListEnumValuesQuery request, CancellationToken cancellationToken)
    {
        if (request.EnumType == null)
            return Task.FromResult(Result<IEnumerable<EnumValueDto>>.Failure("Enum type cannot be null."));

        if (!request.EnumType.IsEnum)
            return Task.FromResult(Result<IEnumerable<EnumValueDto>>.Failure(
                $"Type '{request.EnumType.Name}' is not an enum type."));

        var enumValues = Enum.GetValues(request.EnumType)
            .Cast<Enum>()
            .Select(e => new EnumValueDto(
                Convert.ToInt32(e),
                e.ToString(),
                e.GetEnumDescription()
            ))
            .OrderBy(e => e.Name)
            .ToList();

        return Task.FromResult(Result<IEnumerable<EnumValueDto>>.Success(enumValues));
    }
}
