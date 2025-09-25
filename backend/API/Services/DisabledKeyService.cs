using API.Commons;
using API.Helper;
using API.Models;
using API.Services.Interfaces;
using API.ViewModels;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace API.Services
{
    public class DisabledKeyService : IDisabledKeyService
    {
        private readonly Sep490Context _context;
        private readonly IMapper _mapper;
        private readonly ILog _log;

        public DisabledKeyService(Sep490Context context, IMapper mapper, ILog log)
        {
            _mapper = mapper;
            _context = context;
            _log = log;
        }

        public async Task<(string, SearchResult)> GetAll(DisabledKeySearchVM search)
        {
            var query = _context.DisabledKeys.AsQueryable();

            if (!search.TextSearch.IsEmpty())
            {
                var loweredText = search.TextSearch.ToLower();
                query = query.Where(a => (a.KeyCode ?? "").ToLower().Contains(loweredText) || (a.KeyCombination ?? "").ToLower().Contains(loweredText));
            }

            if (search.IsActive.HasValue)
                query = query.Where(x => x.IsActive == search.IsActive.Value);

            if (search.RiskLevel.HasValue)
                query = query.Where(x => x.RiskLevel == (int)search.RiskLevel);

            var totalCount = await query.CountAsync();
            var totalPage = (int)Math.Ceiling(totalCount / (double)search.PageSize);

            var result = await query
                .OrderByDescending(x => x.IsActive == false)
                .ThenByDescending(x => x.UpdatedAt)
                .Skip((search.CurrentPage - 1) * search.PageSize)
                .Take(search.PageSize)
                .ToListAsync();
            
            if (result.IsObjectEmpty())
                return ("No disabled keys found.", new SearchResult { Result = null, TotalPage = 0 });

            var mapper = _mapper.Map<List<DisabledKeyVM>>(result);
            return ("", new SearchResult
            {
                Result = mapper,
                TotalPage = totalPage,
                CurrentPage = search.CurrentPage,
                PageSize = search.PageSize,
                Total = totalCount
            });
        }

        public async Task<(string, DisabledKeyVM?)> GetOne(string keyId)
        {
            if (keyId.IsEmpty()) return ("Key Id cannot null or empty.", null);

            var key = await _context.DisabledKeys.FindAsync(keyId);
            if (key == null) return ("Key not found!", null);

            var mapper = _mapper.Map<DisabledKeyVM>(key);
            return ("", mapper);
        }

        public async Task<(string, List<string>?)> ChangeActivate(List<string> keyIds, string usertoken)
        {
            if (usertoken.IsEmpty()) return ("Current user ID is required.", null);
            if (keyIds.IsObjectEmpty()) return ("Key Id cannot be null or empty.", null);

            var keys = await _context.DisabledKeys
                .Where(k => keyIds.Contains(k.KeyId))
                .ToListAsync();

            if (keys.Count == 0) return ("No matching keys found.", null);

            foreach (var key in keys)
            {
                key.IsActive = !key.IsActive;
                key.UpdatedAt = DateTime.UtcNow;
                key.UpdatedUser = usertoken;
            }

            _context.DisabledKeys.UpdateRange(keys);
            await _context.SaveChangesAsync();

            var msg = await _log.WriteActivity(new AddUserLogVM
            {
                UserId = usertoken,
                ActionType = keys.All(k => k.IsActive) ? "Activated disabled keys" : "Deactivated disabled keys",
                Description = $"Changed activation status for {keys.Count} disabled key(s).",
                Metadata = "Keys: " + string.Join(", ", keys.Select(k => k.KeyCode)),
                ObjectId = string.Join(", ", keys.Select(k => k.KeyId)),
                Status = (int)LogStatus.Success
            });
            if (msg.Length > 0) return (msg, null);

            return ("", keys.Select(k => k.KeyCode).ToList());
        }

        public async Task<(string, List<string>?)> DoDelete(List<string> keyIds, string usertoken)
        {
           if (keyIds.IsObjectEmpty()) return ("Key Id cannot be null or empty.", null);

           var keys = await _context.DisabledKeys
                .Where(k => keyIds.Contains(k.KeyId))
                .ToListAsync();
            if (keys.IsObjectEmpty()) return ("No matching keys found.", null);
            
            _context.DisabledKeys.RemoveRange(keys);
            await _context.SaveChangesAsync();

            var msg = await _log.WriteActivity(new AddUserLogVM
            {
                UserId = usertoken, // Assuming system user for deletion
                ActionType = "Deleted disabled keys",
                Description = $"Deleted {keys.Count} disabled key(s).",
                Metadata = "Keys: " + string.Join(", ", keys.Select(k => k.KeyCode)),
                ObjectId = string.Join(", ", keys.Select(k => k.KeyId)),
                Status = (int)LogStatus.Success
            });
            if (msg.Length > 0) return (msg, null);

            return ("", keys.Select(x=> x.KeyCode).ToList());
        }

        public async Task<string> CreateUpdate(CreateUpdateDisabledKeyVM input, string usertoken)
        {
            if (usertoken.IsEmpty()) return ("Current user ID is required.");
            if (input == null) return "Input cannot be null.";

            if (input.KeyId.IsEmpty())
            {
                var exitKeyCode = await _context.DisabledKeys.FirstOrDefaultAsync(x=> x.KeyCode == input.KeyCode);
                if (exitKeyCode != null) return ("This KeyCode is already exists! Try another name.");

                var newKey = new DisabledKey
                {
                    KeyId = Guid.NewGuid().ToString(),
                    KeyCode = input.KeyCode,
                    Description = input.Description,
                    KeyCombination = input.KeyCombination,
                    RiskLevel =(int)input.RiskLevel,
                    IsActive = input.IsActive,
                    CreatedAt = DateTime.UtcNow,
                    CreatedUser = usertoken
                };
                await _context.DisabledKeys.AddAsync(newKey);
            }
            else
            {
                var existingKey = await _context.DisabledKeys.FindAsync(input.KeyId);
                if (existingKey == null) return "Key not found.";

                existingKey.KeyCode = input.KeyCode;
                existingKey.RiskLevel = (int)input.RiskLevel;
                existingKey.KeyCombination = input.KeyCombination;
                existingKey.Description = input.Description;
                existingKey.IsActive = input.IsActive;
                existingKey.UpdatedAt = DateTime.UtcNow;
                existingKey.UpdatedUser = usertoken;

                _context.DisabledKeys.Update(existingKey);
            }
            await _context.SaveChangesAsync();
            var msg = await _log.WriteActivity(new AddUserLogVM
            {
                UserId = usertoken,
                ActionType = input.KeyId.IsEmpty() ? "Created" : "Updated",
                Description = $"Disabled key '{input.KeyCode}' has been {(input.KeyId.IsEmpty() ? "created" : "updated")}.",
                Metadata = $"{input.KeyCode} ({input.KeyCombination})",
                ObjectId = input.KeyId,
                Status = (int)LogStatus.Success
            });
            if (msg.Length > 0) return msg;
            return "";
        }
    }
}
