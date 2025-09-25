using API.Commons;
using API.Helper;
using API.Models;
using API.ViewModels;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace API.Cached
{
    public interface IDataCached
    {
        public Task<(string, List<PermissionVM>?)> GetFunctions();
        public Task<(string, List<PermissionVM>?)> GetUserPermissions(string? userId);
        public Task<(string, List<CampusVM>?)> GetCampuses();
        public Task<(string, List<DepartmentVM>?)> GetDepartments();
        public Task<(string, List<SpecializationVM>?)> GetSpecializations();
        public Task<(string, List<PositionVM>?)> GetPositions();
        public Task<(string, List<MajorVM>?)> GetMajors();
        public Task ClearCache();
        public Task RemoveCache(string cachedName);
        public Task ClearAllUserPermissionCache();
    }

    public class DataCached : IDataCached
    {
        private readonly ICacheProvider _cacheProvider;
        private readonly Sep490Context _context;
        private readonly IMapper _mapper;

        private const int TIME_CACHE = 2;

        public DataCached(ICacheProvider cacheProvider, Sep490Context context, IMapper mapper)
        {
            _cacheProvider = cacheProvider ?? throw new ArgumentNullException(nameof(cacheProvider));
            _context = context;
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<(string, List<PermissionVM>?)> GetFunctions()
        {
            var cachedFunctions = await _cacheProvider.GetAsync<List<PermissionVM>>(Constant.PERMISSIONS);
            if (cachedFunctions != null) return ("", cachedFunctions);

            try
            {
                var permissions = await _context.Permissions.Where(x=> x.IsActive == true).ToListAsync();
                if (permissions == null || !permissions.Any()) return ("No permissions found.", new List<PermissionVM>());
                var mapper = _mapper.Map<List<PermissionVM>>(permissions);

                await _cacheProvider.SetAsync(Constant.PERMISSIONS, mapper, TimeSpan.FromHours(TIME_CACHE));
                return ("", mapper);
            }
            catch (Exception ex)
            {
                return ($"Error retrieving functions: {ex.Message}", null);
            }
        }

        public async Task<(string, List<PermissionVM>?)> GetUserPermissions(string? userId)
        {
            if (string.IsNullOrEmpty(userId)) return ("User ID cannot be empty", null);

            string cacheKey = $"{Constant.USER_PERMISSION}_{userId}";
            string keyListKey = Constant.USER_PERMISSION_KEYS;

            var cachedUserFunctions = await _cacheProvider.GetAsync<List<PermissionVM>>(cacheKey);
            if (cachedUserFunctions != null) return ("", cachedUserFunctions);
            try
            {
                var permissions = await (from ur in _context.UserRoles
                                         join rp in _context.RolePermissions on ur.RoleId equals rp.RoleId
                                         join p in _context.Permissions on rp.PermissionId equals p.Id
                                         where ur.UserId == userId && p.IsActive == true
                                         select p)
                                .Distinct()
                                .ToListAsync();
                var mapper = _mapper.Map<List<PermissionVM>>(permissions);

                await _cacheProvider.SetAsync(cacheKey, mapper, TimeSpan.FromHours(TIME_CACHE));
                // Thêm key vào danh sách cache keys
                var existingKeys = await _cacheProvider.GetAsync<List<string>>(keyListKey) ?? [];
                if (!existingKeys.Contains(cacheKey))
                {
                    existingKeys.Add(cacheKey);
                    await _cacheProvider.SetAsync(keyListKey, existingKeys, TimeSpan.FromHours(TIME_CACHE));
                }
                return ("", mapper);
            }
            catch (Exception ex)
            {
                return ($"Error retrieving user functions: {ex.Message}", null);
            }
        }

        public async Task<(string, List<CampusVM>?)> GetCampuses()
        {
            var cached = await _cacheProvider.GetAsync<List<CampusVM>>(Constant.CAMPUS);
            if (cached != null) return ("", cached);
            try
            {
                var data = await _context.Campuses.ToListAsync();
                if (data == null) return ("No found any campus", null);
                var mapper = _mapper.Map<List<CampusVM>>(data);

                await _cacheProvider.SetAsync(Constant.CAMPUS, mapper, TimeSpan.FromHours(TIME_CACHE));
                return ("", mapper);
            }
            catch (Exception ex) { return ($"Error retrieving campuses: {ex.Message}", null); }
        }

        public async Task<(string, List<DepartmentVM>?)> GetDepartments()
        {
            var cached = await _cacheProvider.GetAsync<List<DepartmentVM>>(Constant.DFEPARTMENT);
            if (cached != null) return ("", cached);
            try
            {
                var data = await _context.Departments.ToListAsync();
                if (data == null) return ("No found any department", null);
                var mapper = _mapper.Map<List<DepartmentVM>>(data);

                await _cacheProvider.SetAsync(Constant.DFEPARTMENT, mapper, TimeSpan.FromHours(TIME_CACHE));
                return ("", mapper);
            }
            catch (Exception ex) { return ($"Error retrieving departments: {ex.Message}", null); }
        }

        public async Task<(string, List<SpecializationVM>?)> GetSpecializations()
        {
            var cached = await _cacheProvider.GetAsync<List<SpecializationVM>>(Constant.SPECIALIZATION);
            if (cached != null) return ("", cached);
            try
            {
                var data = await _context.Specializations.ToListAsync();
                if (data == null) return ("No found any specialization", null);
                var mapper = _mapper.Map<List<SpecializationVM>>(data);

                await _cacheProvider.SetAsync(Constant.SPECIALIZATION, mapper, TimeSpan.FromHours(TIME_CACHE));
                return ("", mapper);
            }
            catch (Exception ex) { return ($"Error retrieving specialization: {ex.Message}", null); }
        }

        public async Task<(string, List<PositionVM>?)> GetPositions()
        {
            var cached = await _cacheProvider.GetAsync<List<PositionVM>>(Constant.POSITION);
            if (cached != null) return ("", cached);
            try
            {
                var data = await _context.Positions.ToListAsync();
                if (data == null) return ("No found any position", null);
                var mapper = _mapper.Map<List<PositionVM>>(data);

                await _cacheProvider.SetAsync(Constant.POSITION, mapper, TimeSpan.FromHours(TIME_CACHE));
                return ("", mapper);
            }
            catch (Exception ex) { return ($"Error retrieving positions: {ex.Message}", null); }
        }

        public async Task<(string, List<MajorVM>?)> GetMajors()
        {
            var cached = await _cacheProvider.GetAsync<List<MajorVM>>(Constant.MAJOR);
            if (cached != null) return ("", cached);
            try
            {
                var data = await _context.Majors.ToListAsync();
                if (data == null) return ("No found any major", null);
                var mapper = _mapper.Map<List<MajorVM>>(data);

                await _cacheProvider.SetAsync(Constant.MAJOR, mapper, TimeSpan.FromHours(TIME_CACHE));
                return ("", mapper);
            }
            catch (Exception ex) { return ($"Error retrieving majors: {ex.Message}", null); }
        }

        public async Task ClearCache()
        {
            await _cacheProvider.RemoveAsync(Constant.PERMISSIONS);
            await ClearAllUserPermissionCache();
        }

        public async Task ClearAllUserPermissionCache()
        {
            var keys = await _cacheProvider.GetAsync<List<string>>(Constant.USER_PERMISSION_KEYS);
            if (keys != null)
            {
                foreach (var key in keys)
                {
                    await _cacheProvider.RemoveAsync(key);
                }
                await _cacheProvider.RemoveAsync(Constant.USER_PERMISSION_KEYS);
            }
        }

        public async Task RemoveCache(string cachedName)
        {
            await _cacheProvider.RemoveAsync(cachedName);
        }
    }
}
