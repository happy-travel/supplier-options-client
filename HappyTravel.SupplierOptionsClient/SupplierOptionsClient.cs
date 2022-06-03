using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
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
        => SendWithResult<List<SlimSupplier>>(new HttpRequestMessage(HttpMethod.Get, $"{_clientSettings.BaseEndpoint}/api/1.0/suppliers"));


    public Task<Result<RichSupplier>> Get(string code) 
        => SendWithResult<RichSupplier>(new HttpRequestMessage(HttpMethod.Get, $"{_clientSettings.BaseEndpoint}/api/1.0/suppliers/{code}"));


    public Task<Result> Add(RichSupplier supplier)
        => SendWithoutResult(new HttpRequestMessage(HttpMethod.Post, $"{_clientSettings.BaseEndpoint}/api/1.0/suppliers")
        {
            Content = JsonContent.Create(inputValue: supplier, options: SerializerOptions)
        });


    public Task<Result> Modify(string code, RichSupplier supplier) 
        => SendWithoutResult(new HttpRequestMessage(HttpMethod.Put, $"{_clientSettings.BaseEndpoint}/api/1.0/suppliers/{code}")
        {
            Content = JsonContent.Create(inputValue: supplier, options: SerializerOptions)
        });


    public Task<Result> Delete(string code) 
        => SendWithoutResult(new HttpRequestMessage(HttpMethod.Delete, $"{_clientSettings.BaseEndpoint}/api/1.0/suppliers/{code}"));
    
    
    public Task<Result> Activate(string code, string reason) 
        => SendWithoutResult(new HttpRequestMessage(HttpMethod.Post, $"{_clientSettings.BaseEndpoint}/api/1.0/suppliers/{code}/activate?reason={reason}"));
    
    
    public Task<Result> Deactivate(string code, string reason) 
        => SendWithoutResult(new HttpRequestMessage(HttpMethod.Post, $"{_clientSettings.BaseEndpoint}/api/1.0/suppliers/{code}/deactivate?reason={reason}"));


    public Task<Result<SupplierPriorityByTypes>> GetPriorities()
        => SendWithResult<SupplierPriorityByTypes>(new HttpRequestMessage(HttpMethod.Get, $"{_clientSettings.BaseEndpoint}/api/1.0/supplier-priorities"));
    
    
    public Task<Result> ModifyPriorities(SupplierPriorityByTypes priorities)
        => SendWithoutResult(new HttpRequestMessage(HttpMethod.Put, $"{_clientSettings.BaseEndpoint}/api/1.0/supplier-priorities")
        {
            Content = JsonContent.Create(inputValue: priorities, options: SerializerOptions)
        });
        
    
    public Task<Result> SetEnablementState(string code, EnablementState state, string reason) 
        => SendWithoutResult(new HttpRequestMessage(HttpMethod.Post,
                $"{_clientSettings.BaseEndpoint}/api/1.0/suppliers/{code}/set-enablement-state")
            {
                Content = JsonContent.Create(inputValue: new
                {
                    State = state,
                    Reason = reason
                }, options: SerializerOptions)
            }
        );


    private async Task<Result> SendWithoutResult(HttpRequestMessage message)
    {
        using var response = await Send(message);
        if (response.IsSuccessStatusCode)
            return Result.Success();

        var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>(SerializerOptions) 
                             ?? throw new JsonException("Could not deserialize server error response");

        return Result.Failure(problemDetails?.Detail ?? $"Request to sunpu returned: {(int)response.StatusCode} {response.ReasonPhrase} {await response.Content.ReadAsStringAsync()}");
    }
    

    private async Task<Result<TResponse>> SendWithResult<TResponse>(HttpRequestMessage message)
    {
        using var response = await Send(message);
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<TResponse>(SerializerOptions)
                   ?? throw new JsonException("Could not deserialize server response");
        }

        var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>(SerializerOptions) 
                           ?? throw new JsonException("Could not deserialize server error response");
        
        return Result.Failure<TResponse>(problemDetails?.Detail ?? $"Request to sunpu returned: {(int)response.StatusCode} {response.ReasonPhrase} {await response.Content.ReadAsStringAsync()}");
    }


    private Task<HttpResponseMessage> Send(HttpRequestMessage message) 
        => _httpClientFactory.CreateClient(HttpClientNames.SupplierOptionsClient).SendAsync(message);


    private static readonly JsonSerializerOptions SerializerOptions = new ()
    {
        Converters = { new JsonStringEnumConverter() },
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };
    
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly SupplierOptionsClientSettings _clientSettings;
}