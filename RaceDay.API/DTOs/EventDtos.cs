namespace RaceDay.API.DTOs;

public record CreateEventDto(
    string Name,
    string Description,
    DateTime EventDate,
    string Location,
    decimal Distance,
    string EventType
);

public record UpdateEventDto(
    string Name,
    string Description,
    DateTime EventDate,
    string Location,
    decimal Distance,
    string EventType
);