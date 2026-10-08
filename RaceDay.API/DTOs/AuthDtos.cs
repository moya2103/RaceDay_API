namespace RaceDay.API.DTOs;

public record RegisterDto(string Email, string Password, string FullName, string Role, string? PhoneNumber);
public record LoginDto(string Email, string Password);
