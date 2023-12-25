using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Northwind.Mvc.GraphQLClient.Models;
using System.Text;

namespace Northwind.Mvc.GraphQLClient.Controllers;

public class HomeController : Controller
{
    protected readonly IHttpClientFactory clientFactory;

    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger,
        IHttpClientFactory clientFactory)
    {
        _logger = logger;
        this.clientFactory = clientFactory;
    }

    public async Task<IActionResult> Index(string id = "1")
    {
        IndexViewModel model = new();
        try
        {
            HttpClient client = clientFactory.CreateClient("Northwind.GraphQL");
            HttpRequestMessage request = new(HttpMethod.Get, "/");
            HttpResponseMessage response = await client.SendAsync(request);

            // Check for NO connection        
            if (!response.IsSuccessStatusCode)
            {
                model.Code = response.StatusCode;
                model.Errors = new[] {
                    new Error
                    {
                        Message = "Service is not successfully responding to GET requests."
                    }
                };
                return View(model);
            }

            // Peform query to graphql
            request = new(HttpMethod.Post, "graphql");
            request.Content = new StringContent($$$"""
                {
                  "query": "{productsInCategory(categoryId:{{{id}}}){productId productName unitsInStock}}"
                }
                """,
                encoding: Encoding.UTF8,
                mediaType: "application/json");

            response = await client.SendAsync(request);

            // Perform operations on response from graphql
            model.Code = response.StatusCode;
            model.RawResponseBody = await response.Content.ReadAsStringAsync();
            _logger.LogInformation($"   *****   status code is {model.Code}");
            if (!model.RawResponseBody.Contains("errors"))
            {
                model.Products = (await response.Content.ReadFromJsonAsync<ResponseProducts>())?.Data?.ProductsInCategory;
                _logger.LogInformation($"model.Products is {model.Products}");
            }
            else
            {
                model.Errors = (await response.Content.ReadFromJsonAsync<ResponseErrors>())?.Errors;
                _logger.LogInformation($"model.Errors is {model.Errors}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning($"Northwind.GraphQL service exception: {ex.Message}");
            model.Errors = new[] { new Error { Message = ex.Message } };
        }
        return View(model);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
