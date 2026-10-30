namespace RaceDay.API.Services;

public interface IBlobService
{
    Task<string> UploadFileAsync(Stream fileStream, string fileName);
}