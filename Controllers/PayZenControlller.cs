using System.Diagnostics;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using SimpleMVCForTests.Models;
using static System.Net.Mime.MediaTypeNames;

namespace SimpleMVCForTests.Controllers;

public class PayZenController : Controller
{
    private readonly ILogger<PayZenController> _logger;
    static readonly HttpClient client = new HttpClient();

    public PayZenController(ILogger<PayZenController> logger)
    {
        _logger = logger;
    }

    public async Task<IActionResult> Index()
    {
        try
        {
            var content = new StringContent(
                JsonSerializer.Serialize(new { amount = 990, currency = "EUR", orderId = "myOrderId-999999", customer = new { email = "sample@example.com" } }),
                Encoding.UTF8,
                Application.Json);

            var access_token = "Njk4NzYzNTc6dGVzdHBhc3N3b3JkX0RFTU9QUklWQVRFS0VZMjNHNDQ3NXpYWlEyVUE1eDdN";

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", access_token);

            HttpResponseMessage response = await client.PostAsync("https://api.payzen.eu/api-payment/V4/Charge/CreatePayment", content);
            response.EnsureSuccessStatusCode();
            string responseBody = await response.Content.ReadAsStringAsync();

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var payment = JsonSerializer.Deserialize<CreatePaymentResponse>(responseBody, options);

            _logger.LogInformation("SUCCESS");
            
            return View(payment);
        }
        catch (HttpRequestException e)
        {
            _logger.LogError(e.Message);

            return BadRequest();
        }
    }

    public IActionResult GetForm(string formToken)
    {
        return PartialView("_PayZenForm", formToken);
    }

    [HttpPost]
    public IActionResult Success()
    {
        return View();
    }
    
    [HttpPost]
    public IActionResult Refused()
    {
        return View();
    }
}

public class CreatePaymentResponse
{
    public Answer Answer { get; set; }
}

public class Answer
{
    public string FormToken { get; set; }
}