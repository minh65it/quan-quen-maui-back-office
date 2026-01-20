using System.Threading.Tasks;

namespace QuanQuenBackOfficeMobile.Services.Interfaces;

public interface IAiChatbotService
{
    Task<string> GetResponseAsync(string message, CancellationToken cancellationToken = default);
}


