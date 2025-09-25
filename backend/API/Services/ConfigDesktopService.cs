using API.Commons;
using API.Helper;
using API.Models;
using API.Services.Interfaces;
using API.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace API.Services
{
    public class ConfigDesktopService : IConfigDesktopService 
    {
        private readonly Sep490Context _context;
        public ConfigDesktopService(Sep490Context context)
        {
            _context = context;
        }
         
        public async Task<(string, ConfigsVM?)> GetConfigurations()
        {
            var processes = await _context.ProhibitedApps.Where(x => x.IsActive == true && x.TypeApp == (int)TypeApp.Prohibited)
                .Select(x => x.ProcessName).ToListAsync();
            if (processes.IsObjectEmpty()) return ("No prohibited applications found.", null);

            var whiteLists = await _context.ProhibitedApps.Where(x => x.IsActive == true && x.TypeApp == (int)TypeApp.Whitelisted)
                .Select(x => x.ProcessName).ToListAsync();
            if (processes.IsObjectEmpty()) return ("No prohibited applications found.", null);

            var keys = await _context.DisabledKeys.Where(x => x.IsActive == true).Select(x => x.KeyCode).ToListAsync();
            if (keys.IsObjectEmpty()) return ("No shortcut keys found.", null);

            var url = await _context.ConfigUrls.OrderByDescending(x=> x.CreatedAt).FirstOrDefaultAsync(x => x.IsActive == true);
            if (url == null) return ("No active URL configuration found.", null);

            var config = new ConfigsVM
            {
                BlockedApps = processes,
                ShortcutKeys = keys,
                WhiteListApps = whiteLists,
                ProtectedUrl = url?.Url ?? string.Empty
            };
            return ("", config);
        }
    }
}
