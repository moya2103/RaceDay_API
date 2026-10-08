namespace RaceDay.API.DTOs;

public record CreateResultDto(
    string? FinishTime,
    int? Position,
    bool IsCompleted,
    string? Notes
);