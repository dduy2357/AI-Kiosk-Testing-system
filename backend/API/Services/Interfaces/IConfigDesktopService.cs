using API.ViewModels;

namespace API.Services.Interfaces
{
    public interface IConfigDesktopService
    {
       public Task<(string, ConfigsVM?)> GetConfigurations();
    }
}
