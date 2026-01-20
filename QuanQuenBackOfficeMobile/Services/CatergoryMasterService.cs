using Microsoft.Extensions.Configuration;
using QuanQuenBackOfficeMobile.Models;
using QuanQuenBackOfficeMobile.Services.Interfaces;
using System.Text.Json;

namespace QuanQuenBackOfficeMobile.Services
{
    public class CatergoryMasterService : ICatergoryMasterService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public CatergoryMasterService(
            HttpClient httpClient,
            IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task<List<CategoryMasterModel>> GetCategoryMastersAsync()
        {
            var baseUrl = _configuration["API:Store:BaseUrl"];
            var endpoint = _configuration["API:Store:CategoryMaster"];
            var url = $"{baseUrl}{endpoint}";

            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();

            var wrapper = JsonSerializer.Deserialize<CategoryMasterResponse>(
                json,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

            var result = wrapper?.Data?.Result ?? new List<CategoryMasterModel>();

            // Sort theo position
            return result.ToList();
        }
    }
}
