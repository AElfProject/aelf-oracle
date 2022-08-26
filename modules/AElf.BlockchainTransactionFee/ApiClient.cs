using System.Net;
using System.Text.Json;
using Newtonsoft.Json;

namespace AElf.BlockchainTransactionFee;

public class ApiClient
{
    private readonly HttpClient _httpClient;

    public ApiClient()
    {
        _httpClient = new HttpClient();
    }

    public async Task<T> GetAsync<T>(string uri)
    {
        var proxy = new WebProxy
        {
            Address = new Uri("http://127.0.0.1:1087"),
        };
        var clientHandler = new HttpClientHandler()
        {
            Proxy = proxy,
        };
        HttpClient.DefaultProxy = proxy;
        
        var response = await _httpClient.GetAsync(uri)
            .ConfigureAwait(false);
        response.EnsureSuccessStatusCode();

        var responseContent = await response.Content.ReadAsStringAsync();
        try
        {
            return JsonConvert.DeserializeObject<T>(responseContent);
        }
        catch (Exception e)
        {
            throw new HttpRequestException(e.Message);
        }
    }
}