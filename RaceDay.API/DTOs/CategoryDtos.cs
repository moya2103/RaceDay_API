namespace RaceDay.API.DTOs;

public record CreateCategoryDto(
    string Name,
    string? Description,
    int? MinAge,
    int? MaxAge,
    decimal? Distance,
    decimal? EntryFee
);

public record UpdateCategoryDto(
    string Name,
    string? Description,
    int? MinAge,
    int? MaxAge,
    decimal? Distance,
    decimal? EntryFee
);