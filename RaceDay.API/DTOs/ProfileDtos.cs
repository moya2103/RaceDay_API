namespace RaceDay.API.DTOs;

public record UpdateProfileDto(
    string FullName,
    string? PhoneNumber,
    DateTime? DateOfBirth,
    string? Gender,
    string? EmergencyContact,
    string? CompanyName,
    string? OrganisationPhoneNumber
);

public record ChangePasswordDto(string CurrentPassword, string NewPassword);