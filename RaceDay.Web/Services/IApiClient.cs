namespace RaceDay.Web.Services;

public interface IApiClient
{
    Task<HttpResponseMessage> GetAsync(string endpoint);
    Task<HttpResponseMessage> PostAsync(string endpoint, object data);
    Task<HttpResponseMessage> PutAsync(string endpoint, object data);
    Task<HttpResponseMessage> DeleteAsync(string endpoint);
    Task<HttpResponseMessage> PostFileAsync(string endpoint, IFormFile file);
    Task<HttpResponseMessage> PostMultipartAsync(string endpoint, MultipartFormDataContent content);
}