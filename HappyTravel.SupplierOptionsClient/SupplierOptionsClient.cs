using System.Net.Http.Json;
using System.Text.Json;
using CSharpFunctionalExtensions;
using HappyTravel.SupplierOptionsClient.Infrastructure;
using HappyTravel.SupplierOptionsClient.Models;
using HappyTravel.SupplierOptionsClient.Settings;
using Microsoft.Extensions.Options;

namespace HappyTravel.SupplierOptionsClient;

public class SupplierOptionsClient : ISupplierOptionsClient
{
    public SupplierOptionsClient(IHttpClientFactory httpClientFactory, IOptions<SupplierOptionsClientSettings> clientSettings)
    {
        _httpClientFactory = httpClientFactory;
        _clientSettings = clientSettings.Value;
    }

    
    public Task<Result<List<SlimSupplier>>> GetAll() 
        => SendWithResult<List<SlimSupplier>>(new HttpRequestMessage(HttpMethod.Get, _clientSettings.Endpoint));


    public Task<Result<RichSupplier>> Get(string code) 
        => SendWithResult<RichSupplier>(new HttpRequestMessage(HttpMethod.Get, $"{_clientSettings.Endpoint}/{code}"));


    public Task<Result> Add(RichSupplier supplier)
        => SendWithoutResult(new HttpRequestMessage(HttpMethod.Post, _clientSettings.Endpoint)
        {
            Content = new StringContent(JsonSerializer.Serialize(supplier))
        });


    public Task<Result> Modify(string code, RichSupplier supplier) 
        => SendWithoutResult(new HttpRequestMessage(HttpMethod.Put, $"{_clientSettings.Endpoint}/{code}")
        {
            Content = new StringContent(JsonSerializer.Serialize(supplier))
        });


    public Task<Result> Delete(string code) 
        => SendWithoutResult(new HttpRequestMessage(HttpMethod.Delete, $"{_clientSettings.Endpoint}/{code}"));

    
    private async Task<Result> SendWithoutResult(HttpRequestMessage message)
    {
        using var response = await Send(message);
        if (response.IsSuccessStatusCode)
            return Result.Success();

        var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>(SerializerOptions) 
                             ?? throw new JsonException("Could not deserialize server error response") ;;

        return Result.Failure(problemDetails.Detail);
    }
    

    private async Task<Result<TResponse>> SendWithResult<TResponse>(HttpRequestMessage message)
    {
        using var response = await Send(message);
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<TResponse>(SerializerOptions)
                   ?? throw new JsonException("Could not deserialize server response");
        }

        var errorDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>() 
                           ?? throw new JsonException("Could not deserialize server error response");
        
        return Result.Failure<TResponse>(errorDetails.Detail);
    }


    private Task<HttpResponseMessage> Send(HttpRequestMessage message) 
        => _httpClientFactory.CreateClient(HttpClientNames.SupplierOptionsClient).SendAsync(message);


    private static readonly JsonSerializerOptions SerializerOptions = new () { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
    
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly SupplierOptionsClientSettings _clientSettings;
}