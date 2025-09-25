using API.Commons;
using API.Helper;
using API.Models;
using API.Services.Interfaces;
using API.ViewModels;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace API.Services
{
    public class ProhibitedAppService : IProhibitedAppService
    {
        private readonly Sep490Context _context;
        private readonly IMapper _mapper;
        private readonly ILog _log;

        public ProhibitedAppService(Sep490Context context, IMapper mapper, ILog log)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _mapper = mapper;
            _log = log;
        }

        public async Task<(string, SearchResult?)> GetAll(ProhibitedAppSearchVM search)
        {
            var query = _context.ProhibitedApps.AsQueryable();

            if (!search.TextSearch.IsEmpty())
            {
                var loweredText = search.TextSearch.ToLower();
                query = query.Where(a => a.AppName.ToLower().Contains(loweredText) || a.ProcessName.ToLower().Contains(loweredText));
            }
            if (search.IsActive.HasValue)
                query = query.Where(a => a.IsActive == search.IsActive.Value);

            if (search.TypeApp.HasValue)
                query = query.Where(x => x.TypeApp == (int)search.TypeApp);

            if (search.RiskLevel.HasValue)
                query = query.Where(x => x.RiskLevel == (int)search.RiskLevel);

            if (search.Category.HasValue)
                query = query.Where(x => x.Category == (int)search.Category);

            var totalCount = await query.CountAsync();
            var totalPage = (int)Math.Ceiling(totalCount / (double)search.PageSize);

            var result = await query
                .OrderByDescending(a => a.IsActive == false)
                .ThenByDescending(a => a.UpdatedAt)
                .Skip((search.CurrentPage - 1) * search.PageSize)
                .Take(search.PageSize)
                .ToListAsync();
            if (result.IsObjectEmpty())
                return ("No prohibited apps found.", new SearchResult { Result = null, TotalPage = 0 });

            var mapperResult = _mapper.Map<List<ProhibitedAppVM>>(result);
            return ("", new SearchResult
            {
                Result = mapperResult,
                TotalPage = totalPage,
                CurrentPage = search.CurrentPage,
                PageSize = search.PageSize,
                Total = totalCount
            });
        }

        public async Task<(string, ProhibitedAppVM?)> GetOne(string appId)
        {
            if (string.IsNullOrEmpty(appId))
                return ("App ID cannot be null or empty.", null);

            var app = await _context.ProhibitedApps.FirstOrDefaultAsync(a => a.AppId == appId || a.AppName == appId || a.ProcessName == appId);
            if (app == null) return ("Prohibited app not found.", null);

            var appVM = _mapper.Map<ProhibitedAppVM>(app);
            return ("", appVM);
        }

        public async Task<(string, List<string>?)> ChangeActivate(List<string> appIds, string usertoken)
        {
            if (appIds.IsObjectEmpty()) return ("App Id cannot be null or empty.", null);

            var apps = await _context.ProhibitedApps.Where(a => appIds.Contains(a.AppId)).ToListAsync();
            if (apps.IsObjectEmpty()) return ("No prohibited apps found for the provided IDs.", null);

            foreach (var app in apps)
            {
                app.IsActive = !app.IsActive;
                app.UpdatedUser = usertoken;
                app.UpdatedAt = DateTime.UtcNow;
            }
            _context.ProhibitedApps.UpdateRange(apps);
            await _context.SaveChangesAsync();

            var msg = await _log.WriteActivity(new AddUserLogVM
            {
                UserId = usertoken,
                ActionType = apps.All(a => a.IsActive) ? "Activated prohibited apps" : "Deactivated prohibited apps",
                Description = $"Changed activation status for {apps.Count} prohibited app(s) [{apps.First().AppName}].",
                Metadata = string.Join(", ", apps.Select(a => $"{a.AppName} ({a.ProcessName})")),
                ObjectId = string.Join(", ", apps.Select(a => a.AppId)),
                Status = (int)LogStatus.Success
            });
            if (msg.Length > 0) return (msg, null);

            return ("", apps.Select(a => a.AppName).ToList());
        }

        public async Task<string> CreateUpdate(CreateUpdateProhibitedAppVM input, string usertoken)
        {
            if (input.AppId.IsEmpty())
            {
                var existingApp = await _context.ProhibitedApps.AnyAsync(a => a.AppName.ToLower() == input.AppName.ToLower() || a.ProcessName.ToLower() == input.ProcessName.ToLower());
                if (existingApp) return "Prohibited app name or process is already in use. Please enter a different one.";

                var newApp = new ProhibitedApp
                {
                    AppId = Guid.NewGuid().ToString(),
                    AppName = input.AppName,
                    ProcessName = input.ProcessName,
                    IsActive = input.IsActive,
                    TypeApp = (int)input.TypeApp,
                    RiskLevel = (int)input.RiskLevel,
                    Category = (int)input.Category,
                    Description = input.Description,
                    CreatedAt = DateTime.UtcNow,
                    CreatedUser = usertoken
                };
                _context.ProhibitedApps.Add(newApp);
            }
            else
            {
                var app = await _context.ProhibitedApps.FindAsync(input.AppId);
                if (app == null) return "Prohibited app not found.";

                var duplicate = await _context.ProhibitedApps.AnyAsync(a => a.AppId != input.AppId && (a.AppName.ToLower() == input.AppName.ToLower() || a.ProcessName.ToLower() == input.ProcessName.ToLower()));
                if (duplicate) return "Another prohibited app already uses this name or process. Please choose a different one.";

                app.AppName = input.AppName;
                app.ProcessName = input.ProcessName;
                app.AppIconUrl = input.AppIconUrl;
                app.TypeApp = (int)input.TypeApp;
                app.IsActive = input.IsActive;
                app.RiskLevel = (int)input.RiskLevel;
                app.Category = (int)input.Category;
                app.Description = input.Description;
                app.UpdatedUser = usertoken;
                app.UpdatedAt = DateTime.UtcNow;

                _context.ProhibitedApps.Update(app);
            }
            await _context.SaveChangesAsync();
            var msg = await _log.WriteActivity(new AddUserLogVM
            {
                UserId = usertoken,
                ActionType = input.AppId.IsEmpty() ? "Created" : "Updated",
                Description = $"Prohibited app '{input.AppName}' has been {(input.AppId.IsEmpty() ? "created" : "updated")}.",
                Metadata = $"{input.AppName} ({input.ProcessName})",
                ObjectId = input.AppId,
                Status = (int)LogStatus.Success
            });
            if (msg.Length > 0) return msg;
            return "";
        }

        public async Task<(string, List<string>?)> DoRemove(List<string> appIds, string usertoken)
        {
            if (appIds.IsObjectEmpty()) return ("App Id cannot be null or empty.", null);

            var apps = await _context.ProhibitedApps.Where(a => appIds.Contains(a.AppId)).ToListAsync();
            if (apps.IsObjectEmpty()) return ("No prohibited apps found for the provided IDs.", null);

            _context.ProhibitedApps.RemoveRange(apps);
            await _context.SaveChangesAsync();

            var msg = await _log.WriteActivity(new AddUserLogVM
            {
                UserId = usertoken, // Assuming system user for deletion
                ActionType = "Removed",
                Description = $"Removed {apps.Count} prohibited app(s).",
                Metadata = string.Join(", ", apps.Select(a => $"{a.AppName} ({a.ProcessName})")),
                ObjectId = string.Join(", ", apps.Select(a => a.AppId)),
                Status = (int)LogStatus.Success
            });
            if (msg.Length > 0) return (msg, null);

            return ("", apps.Select(a => a.AppName).ToList());
        }
    }
}
