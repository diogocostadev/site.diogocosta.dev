using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace site.diogocosta.dev.Servicos;

public class MauticContact
{
    public string Email { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
}

// Services/IMauticService.cs
public interface IMauticService
{
    //Task<bool> 
    Task<(bool success, string message)> AdicionarContatoAsync(MauticContact contact);
    Task<(bool success, string message)> AdicionarContatoAsync(MauticContact contact, int segmentId);
}

public class MauticService : IMauticService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly string _mauticBaseUrl;
    private readonly string _basicAuthToken;

    private string _username = "diogo";
    private string _password = "Did@15csbr";
    
    public MauticService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _mauticBaseUrl = "https://m.didaticos.com";//_configuration["Mautic:BaseUrl"];
        
        // Criando o token de autenticação básica
        var username = "diogo" ;//_configuration["Mautic:Username"];
        var password = "Did@15csbr" ;//_configuration["Mautic:Password"];
        var authString = $"{username}:{password}";
        _basicAuthToken = Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes(authString));
    }

    public async Task<(bool success, string message)> AdicionarContatoAsync(MauticContact contact)
    {
        try
        {
            var authString = $"{_username}:{_password}";
            var base64Auth = Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes(authString));

            // Primeiro, verifica se o contato já existe
            var searchEndpoint = $"{_mauticBaseUrl}/api/contacts?search=email:{contact.Email}";
            var searchRequest = new HttpRequestMessage(HttpMethod.Get, searchEndpoint);
            searchRequest.Headers.Authorization = new AuthenticationHeaderValue("Basic", base64Auth);

            var searchResponse = await _httpClient.SendAsync(searchRequest);
            var searchContent = await searchResponse.Content.ReadAsStringAsync();

            string contactId;

            if (searchResponse.IsSuccessStatusCode)
            {
                using var doc = JsonDocument.Parse(searchContent);
                var root = doc.RootElement;

                if (root.TryGetProperty("contacts", out var contacts) && contacts.EnumerateObject().Any())
                {
                    // Usa contato existente
                    contactId = contacts.EnumerateObject().First().Name;
                }
                else
                {
                    // Cria novo contato
                    var createEndpoint = $"{_mauticBaseUrl}/api/contacts/new";
                    var contactData = new Dictionary<string, object>
                    {
                        { "email", contact.Email },
                        { "firstname", contact.FirstName },
                        { "lastname", contact.LastName },
                        { "tags", new[] { "newsletter", "site" } }
                    };

                    var createRequest = new HttpRequestMessage(HttpMethod.Post, createEndpoint)
                    {
                        Content = new StringContent(
                            JsonSerializer.Serialize(contactData),
                            Encoding.UTF8,
                            "application/json"
                        )
                    };
                    createRequest.Headers.Authorization = new AuthenticationHeaderValue("Basic", base64Auth);

                    var createResponse = await _httpClient.SendAsync(createRequest);
                    var createContent = await createResponse.Content.ReadAsStringAsync();
                    Console.WriteLine($"Resposta da criação: {createContent}");

                    if (!createResponse.IsSuccessStatusCode)
                    {
                        Console.WriteLine($"Erro ao criar contato no Mautic: {createContent}");
                        return (false,
                            "Não foi possível processar sua inscrição no momento. Por favor, tente novamente.");
                    }

                    using var createDoc = JsonDocument.Parse(createContent);
                    contactId = createDoc.RootElement.GetProperty("contact").GetProperty("id").GetInt32().ToString();
                }

                // Adiciona ao segmento usando o endpoint específico
                var addToSegmentEndpoint = $"{_mauticBaseUrl}/api/segments/1/contact/{contactId}/add";
                var addToSegmentRequest = new HttpRequestMessage(HttpMethod.Post, addToSegmentEndpoint);
                addToSegmentRequest.Headers.Authorization = new AuthenticationHeaderValue("Basic", base64Auth);

                var addToSegmentResponse = await _httpClient.SendAsync(addToSegmentRequest);
                var addToSegmentContent = await addToSegmentResponse.Content.ReadAsStringAsync();
                Console.WriteLine($"Resposta da adição ao segmento: {addToSegmentContent}");

                if (!addToSegmentResponse.IsSuccessStatusCode)
                {
                    Console.WriteLine($"Erro ao adicionar ao segmento: {addToSegmentContent}");
                    return (true,
                        "Inscrição realizada, mas houve um problema ao adicionar à newsletter. Nossa equipe será notificada.");
                }

                return (true, "Inscrição realizada com sucesso! Em breve você receberá nossas novidades.");
            }

            return (false, "Não foi possível processar sua inscrição no momento. Por favor, tente novamente.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exceção: {ex.Message}");
            return (false, "Ocorreu um erro ao processar sua inscrição. Por favor, tente novamente mais tarde.");
        }
    }

    public async Task<(bool success, string message)> AdicionarContatoAsync(MauticContact contact, int segmentId)
    {
        try
        {
            var authString = $"{_username}:{_password}";
            var base64Auth = Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes(authString));

            // Primeiro, verifica se o contato já existe
            var searchEndpoint = $"{_mauticBaseUrl}/api/contacts?search=email:{contact.Email}";
            var searchRequest = new HttpRequestMessage(HttpMethod.Get, searchEndpoint);
            searchRequest.Headers.Authorization = new AuthenticationHeaderValue("Basic", base64Auth);

            var searchResponse = await _httpClient.SendAsync(searchRequest);
            var searchContent = await searchResponse.Content.ReadAsStringAsync();

            string contactId;

            if (searchResponse.IsSuccessStatusCode)
            {
                using var doc = JsonDocument.Parse(searchContent);
                var root = doc.RootElement;

                if (root.TryGetProperty("contacts", out var contacts) && contacts.EnumerateObject().Any())
                {
                    // Usa contato existente
                    contactId = contacts.EnumerateObject().First().Name;
                }
                else
                {
                    // Cria novo contato
                    var createEndpoint = $"{_mauticBaseUrl}/api/contacts/new";
                    var contactData = new Dictionary<string, object>
                    {
                        { "email", contact.Email },
                        { "firstname", contact.FirstName },
                        { "lastname", contact.LastName },
                        { "tags", new[] { "newsletter", "site" } }
                    };

                    var createRequest = new HttpRequestMessage(HttpMethod.Post, createEndpoint)
                    {
                        Content = new StringContent(
                            JsonSerializer.Serialize(contactData),
                            Encoding.UTF8,
                            "application/json"
                        )
                    };
                    createRequest.Headers.Authorization = new AuthenticationHeaderValue("Basic", base64Auth);

                    var createResponse = await _httpClient.SendAsync(createRequest);
                    var createContent = await createResponse.Content.ReadAsStringAsync();
                    Console.WriteLine($"Resposta da criação: {createContent}");

                    if (!createResponse.IsSuccessStatusCode)
                    {
                        Console.WriteLine($"Erro ao criar contato no Mautic: {createContent}");
                        return (false,
                            "Não foi possível processar sua inscrição no momento. Por favor, tente novamente.");
                    }

                    using var createDoc = JsonDocument.Parse(createContent);
                    contactId = createDoc.RootElement.GetProperty("contact").GetProperty("id").GetInt32().ToString();
                }

                // Adiciona ao segmento usando o endpoint específico
                var addToSegmentEndpoint = $"{_mauticBaseUrl}/api/segments/{segmentId}/contact/{contactId}/add";
                var addToSegmentRequest = new HttpRequestMessage(HttpMethod.Post, addToSegmentEndpoint);
                addToSegmentRequest.Headers.Authorization = new AuthenticationHeaderValue("Basic", base64Auth);

                var addToSegmentResponse = await _httpClient.SendAsync(addToSegmentRequest);
                var addToSegmentContent = await addToSegmentResponse.Content.ReadAsStringAsync();
                Console.WriteLine($"Resposta da adição ao segmento {segmentId}: {addToSegmentContent}");

                if (!addToSegmentResponse.IsSuccessStatusCode)
                {
                    Console.WriteLine($"Erro ao adicionar ao segmento {segmentId}: {addToSegmentContent}");
                    return (true,
                        "Inscrição realizada, mas houve um problema ao adicionar à newsletter. Nossa equipe será notificada.");
                }

                return (true, "Inscrição realizada com sucesso! Em breve você receberá nossas novidades.");
            }

            return (false, "Não foi possível processar sua inscrição no momento. Por favor, tente novamente.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exceção: {ex.Message}");
            return (false, "Ocorreu um erro ao processar sua inscrição. Por favor, tente novamente mais tarde.");
        }
    }
}