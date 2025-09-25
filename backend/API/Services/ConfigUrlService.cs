using API.Commons;
using API.Helper;
using API.Models;
using API.Services.Interfaces;
using API.ViewModels;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace API.Services
{
    public class ConfigUrlService : IConfigUrlService
    {
        private readonly Sep490Context _context;
        private readonly IMapper _mapper;
        private readonly ILog _log;
        public ConfigUrlService(Sep490Context context, IMapper mapper, ILog log)
        {
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _log = log;
        }

        public async Task<(string, ConfigUrlVM?)> GetOne(string id)
        {
            if (id.IsEmpty()) return ("Id cannot be null or empty", null);

            var configUrl = await _context.ConfigUrls.FindAsync(id);
            if (configUrl == null) return ("ConfigUrl not found", null);

            var configUrlVM = _mapper.Map<ConfigUrlVM>(configUrl);
            return ("", configUrlVM);
        }

        public async Task<(string, List<ConfigUrlVM>?)> GetAll()
        {
            var configUrls = await _context.ConfigUrls.ToListAsync();
            if (configUrls == null || !configUrls.Any()) return ("No ConfigUrls found", null);

            var configUrlsVM = _mapper.Map<List<ConfigUrlVM>>(configUrls);
            return ("", configUrlsVM);
        }

        public async Task<string> ToggleConfigUrl(string id, string? usertoken)
        {
            if (id.IsEmpty()) return "Id cannot be null or empty";

            var configUrl = await _context.ConfigUrls.FindAsync(id);
            if (configUrl == null) return "ConfigUrl not found";

            configUrl.IsActive = !configUrl.IsActive;
            configUrl.UpdatedAt = DateTime.UtcNow;

            _context.ConfigUrls.Update(configUrl);
            await _context.SaveChangesAsync();

            var msg = await _log.WriteActivity(new AddUserLogVM
            {
                ActionType = configUrl.IsActive ? "Activated ConfigUrl" : "Deactivated ConfigUrl",
                UserId = usertoken,
                Description = $"ConfigUrl {configUrl.Name} has been {(configUrl.IsActive ? "activated" : "deactivated")}.",
                Metadata = $"ConfigUrl Id: {configUrl.Id}, Url: {configUrl.Url}",
                ObjectId = configUrl.Id,
                Status = (int)LogStatus.Success
            });
            if (msg.Length > 0) return msg;
            return "";
        }

        public async Task<(string, object?)> CreateUpdate(CreateUpdateConfigUrlVM model, string? usertoken)
        {
            if (model.Id.IsEmpty())
            {
                var existingConfig = await _context.ConfigUrls.AnyAsync(c => c.Url == model.Url);
                if (existingConfig) return ("A ConfigUrl with this URL already exists.", null);

                var newConfig = new ConfigUrl
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = model.Name,
                    Url = model.Url,
                    Version = model.Version,
                    Description = model.Description,
                    IsActive = model.IsActive,
                    CreatedAt = DateTime.UtcNow,
                };
                _context.ConfigUrls.Add(newConfig);
            }
            else
            {
                var oldConfig = await _context.ConfigUrls.FindAsync(model.Id);
                if (oldConfig == null) return ("ConfigUrl not found", null);

                var existingConfig = await _context.ConfigUrls.AnyAsync(c => c.Url == model.Url && c.Id != model.Id);
                if (existingConfig) return ("This Url is already in use. Please enter a different one.", null);

                oldConfig.Name = model.Name;
                oldConfig.Url = model.Url;
                oldConfig.Version = model.Version;
                oldConfig.Description = model.Description;
                oldConfig.IsActive = model.IsActive;
                oldConfig.UpdatedAt = DateTime.UtcNow;

                _context.ConfigUrls.Update(oldConfig);
            }
            await _context.SaveChangesAsync();

            var msg = await _log.WriteActivity(new AddUserLogVM
            {
                UserId = usertoken,
                ActionType = model.Id.IsEmpty() ? "Created ConfigUrl" : "Updated ConfigUrl",
                Description = $"ConfigUrl '{model.Name}' has been {(model.Id.IsEmpty() ? "created" : "updated")}.",
                Metadata = $"{model.Name} ({model.Url})",
                ObjectId = model.Id,
                Status = (int)LogStatus.Success
            });
            if (msg.Length > 0) return (msg, null);
            return ("", model);
        }

        public async Task<(string, List<string>?)> DoRemove(List<string> urls, string? usertoken)
        {
            if (urls.IsObjectEmpty()) return ("App Id cannot be null or empty.", null);

            var existUrl = await _context.ConfigUrls.Where(a => urls.Contains(a.Url ?? "")).ToListAsync();
            if (existUrl.IsObjectEmpty()) return ("No prohibited apps found for the provided IDs.", null);

            _context.ConfigUrls.RemoveRange(existUrl);
            await _context.SaveChangesAsync();

            var msg = await _log.WriteActivity(new AddUserLogVM
            {
                ActionType = "Deleted ConfigUrls",
                UserId = usertoken,
                Description = $"Deleted {existUrl.Count} ConfigUrl(s).",
                Metadata = "Urls: " + string.Join(", ", existUrl.Select(c => c.Url)),
                ObjectId = string.Join(", ", existUrl.Select(c => c.Id)),
                Status = (int)LogStatus.Success
            });
            if (msg.Length > 0) return (msg, null);
            return ("", existUrl.Select(a => a.Url ?? "").ToList());
        }
    }
}
