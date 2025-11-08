using BuildingBlocks.Application.Models;
using MediatR;

namespace BuildingBlocks.Application.Queries.EnumValues;

/// <summary>
/// Query to retrieve all values of a specific enum type.
/// </summary>
/// <param name="EnumType">The type of the enum to retrieve values for.</param>
public record GetListEnumValuesQuery(Type EnumType) : IRequest<Result<IEnumerable<EnumValueDto>>>;

/// <summary>
/// Data transfer object containing enum value information.
/// </summary>
/// <param name="Value">Numeric value of the enum.</param>
/// <param name="Name">Text name of the enum value.</param>
/// <param name="Description">Optional description of the enum value.</param>
public record EnumValueDto(
    int Value,
    string Name,
    string? Description
);
