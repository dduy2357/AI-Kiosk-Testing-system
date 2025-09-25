using API.Cached;
using API.Commons;
using API.Helper;
using API.Models;
using API.Services.Interfaces;
using API.Utilities;
using API.ViewModels;
using AutoMapper;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.ComponentModel.DataAnnotations;

namespace API.Services
{
    public class UserService : IUserService
    {
        private readonly IMapper _mapper;
        private readonly Sep490Context _context;
        private readonly ILog _log;
        private readonly IAmazonS3Service _s3Service;

        public UserService(IMapper mapper, Sep490Context context, ILog log, IAmazonS3Service amazonS3Service)
        {
            _mapper = mapper;
            _context = context;
            _log = log;
            _s3Service = amazonS3Service;
        }

        public async Task<(string, UserListVM?)> GetById(string? userID)
        {
            if (userID == null) return ("User ID is not valid.", null);

            var user = await _context.Users.Select(x => new UserListVM
            {
                UserId = x.UserId,
                FullName = x.FullName,
                UserCode = x.UserCode,
                Phone = x.Phone,
                Email = x.Email,
                Dob = x.Dob,
                Sex = x.Sex,
                AvatarUrl = x.AvatarUrl,
                Address = x.Address,
                Campus = x.Campus.Id,
                Department = x.DepartmentId,
                Status = x.Status,
                Major = x.MajorId,
                Position = x.PositionId,
                Specialization = x.SpecializationId,
                CreateAt = x.CreateAt,
                UpdateAt = x.UpdateAt,
                LastLogin = x.LastLogin,
                LastLoginIp = x.LastLoginIp,
                CreateUser = x.CreateUser,
                UpdateUser = x.UpdateUser,
                RoleId = x.UserRoles.Select(ur => ur.RoleId).ToList()
            }).FirstOrDefaultAsync(x => x.UserId == userID);
            if (user == null) return ("User not found", null);

            return (string.Empty, user);
        }

        public async Task<string> DoToggleActive(string? usertoken, string? userId)
        {
            if (userId.IsEmpty()) return "User ID is not valid!";

            var user = await _context.Users.FirstOrDefaultAsync(x => x.UserId == userId);
            if (user == null) return "User not found!";

            user.Status = user.Status == (int)UserStatus.Active ? (int)UserStatus.Inactive : (int)UserStatus.Active;
            user.UpdateAt = DateTime.UtcNow;
            user.UpdateUser = usertoken;
            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            var msg = await _log.WriteActivity(new AddUserLogVM
            {
                UserId = usertoken,
                ActionType = user.Status == (int)UserStatus.Active ? "Activate" : "Deactivate",
                Description = $"User {user.FullName} with Usercode {user.UserCode} has been {(user.Status == (int)UserStatus.Active ? "activated" : "deactivated")}",
                Status = (int)LogStatus.Success,
                Metadata = $"User ID: {user.UserId}, Status: {user.Status}"
            });
            if (msg.Length > 0) return msg;
            return "";
        }
        public async Task<(string, SearchResult?)> GetList(SearchUserVM search, string? usertoken)
        {
            var query = _context.Users.Include(x => x.UserRoles).Include(x => x.Campus).Include(x => x.Department)
                .Include(x => x.Major).Include(x => x.Position).Include(x => x.Specialization).AsNoTracking()
                .AsQueryable();

            if (search.RoleId.HasValue)
                query = query.Where(u => u.UserRoles.Any(ur => ur.RoleId == (int)search.RoleId));

            if (search.Status.HasValue)
                query = query.Where(u => u.Status == (int)search.Status);

            if (!search.CampusId.IsEmpty())
                query = query.Where(u => u.CampusId == search.CampusId);

            if (!search.TextSearch.IsEmpty())
                query = query.Where(u => u.Email.ToLower().Contains(search.TextSearch.ToLower())
                || u.UserId.ToLower().Contains(search.TextSearch.ToLower())
                || u.FullName.ToLower().Contains(search.TextSearch.ToLower())
                || u.UserCode.ToLower().Contains(search.TextSearch.ToLower()));

            if (search.SortType.HasValue)
                query = search.SortType == (int)SortType.Ascending ? query.OrderBy(x => x.FullName) : query.OrderByDescending(x => x.FullName);

            var totalCount = await query.CountAsync();
            var totalPage = (int)Math.Ceiling(totalCount / (double)search.PageSize);

            var results = await query
            .Skip((search.CurrentPage - 1) * search.PageSize)
            .Take(search.PageSize)
            .Select(x => new UserListVM
            {
                UserId = x.UserId,
                FullName = x.FullName,
                UserCode = x.UserCode,
                Phone = x.Phone,
                Email = x.Email,
                Dob = x.Dob,
                Sex = x.Sex,
                AvatarUrl = x.AvatarUrl,
                Address = x.Address,
                Campus = x.Campus.Name,
                Department = x.Department.Name,
                Status = x.Status,
                Major = x.Major.Name,
                Position = x.Position.Name,
                Specialization = x.Specialization.Name,
                CreateAt = x.CreateAt,
                UpdateAt = x.UpdateAt,
                LastLogin = x.LastLogin,
                LastLoginIp = x.LastLoginIp,
                CreateUser = x.CreateUser,
                UpdateUser = x.UpdateUser,
                RoleId = x.UserRoles.Select(ur => ur.RoleId).ToList()
            }).ToListAsync();
            if (results.IsNullOrEmpty()) return ("No users found.", null);

            return ("", new SearchResult
            {
                Result = results,
                TotalPage = totalPage,
                PageSize = search.PageSize,
                CurrentPage = search.CurrentPage,
                Total = totalCount
            });
        }

        public string CheckImportData(IFormFile fileData, out List<ErrorImport> result)
        {
            result = new List<ErrorImport>();

            var msg = FileHandler.ImportFromExcel(fileData, out List<UserImportVM> importedData);
            if (!string.IsNullOrEmpty(msg)) return msg;

            if (importedData == null || importedData.Count == 0)
                return "No data found in the file.";

            for (int i = 0; i < importedData.Count; i++)
            {
                var importItem = importedData[i];
                var createModel = _mapper.Map<CreateUserVM>(importItem);

                var context = new ValidationContext(createModel);
                var validationResults = new List<ValidationResult>();

                Validator.TryValidateObject(createModel, context, validationResults, true);
                var error = new ErrorImport
                {
                    User = importItem,
                    Row = i + 2, // +2 vì dòng 1 là header
                    Errors = validationResults.Select(r => r.ErrorMessage ?? "").ToList()
                };
                result.Add(error);
            }
            return "";
        }

        public async Task<(string, List<(AddListUserVM, string)>)> AddListUser(List<AddListUserVM> users, string? userToken)
        {
            if (users == null || users.Count == 0) return ("List Users cannot null or empty.", null);
            var errorList = new List<(AddListUserVM, string)>();
            var newUsers = new List<User>();
            var newUserRoles = new List<UserRole>();

            using var trans = await _context.Database.BeginTransactionAsync();
            foreach (var input in users)
            {
                try
                {
                    var password = Utils.GenerateCharacter(8);
                    var msg = Converter.StringToMD5(password, out string mkMd5);
                    if (msg.Length > 0) { errorList.Add((input, msg)); continue; }

                    msg = await CheckEmailExisted(input.Email);
                    if (msg.Length > 0) { errorList.Add((input, msg)); continue; }

                    msg = await CheckPhoneExisted(input.Phone);
                    if (msg.Length > 0) { errorList.Add((input, msg)); continue; }

                    msg = await CheckUserCodeExisted(input.UserCode);
                    if (msg.Length > 0) { errorList.Add((input, msg)); continue; }

                    // Validate logic theo role
                    var validationError = await ValidateUserInput(input);
                    if (!string.IsNullOrEmpty(validationError)) { errorList.Add((input, validationError)); continue; }

                    // Lấy danh sách vai trò hợp lệ
                    var validRoles = await _context.Roles.Where(r => input.RoleId.Contains(r.Id)).Select(r => r.Id).ToListAsync();
                    if (validRoles.Count == 0) { errorList.Add((input, "No valid roles found.")); continue; }

                    // Tạo user
                    var newUserId = Guid.NewGuid().ToString();
                    var user = new User
                    {
                        UserId = newUserId,
                        FullName = input.FullName,
                        Phone = input.Phone,
                        Email = input.Email,
                        Address = input.Address,
                        CampusId = input.CampusId,
                        UserCode = input.UserCode,
                        CreateAt = DateTime.UtcNow,
                        CreateUser = userToken,
                        DepartmentId = input.DepartmentId,
                        MajorId = input.MajorId,
                        PositionId = input.PositionId,
                        Sex = input.Sex,
                        SpecializationId = input.SpecializationId,
                        Status = input.Status,
                        Dob = input.Dob,
                        Password = mkMd5,
                    };
                    newUsers.Add(user);
                    msg = await EmailHandler.SendEmailAsync(input.Email, "Mật khẩu mới", $"Đây là mật khẩu mới của bạn: {password}");
                    if (msg.Length > 0) { errorList.Add((input, "No valid roles found.")); continue; }

                    var roles = validRoles.Select(roleId => new UserRole
                    {
                        UserId = newUserId,
                        RoleId = roleId
                    });
                    newUserRoles.AddRange(roles);
                }
                catch (Exception ex)
                {
                    errorList.Add((input, $"Unexpected error: {ex.Message}"));
                }
            }

            try
            {
                if (newUsers.Any())
                {
                    await _context.Users.AddRangeAsync(newUsers);
                    await _context.UserRoles.AddRangeAsync(newUserRoles);
                    await _context.SaveChangesAsync();
                    await trans.CommitAsync();
                }
                else
                {
                    await trans.RollbackAsync();
                    return ("No users were created.", errorList);
                }

                var msg = await _log.WriteActivity(new AddUserLogVM
                {
                    UserId = userToken,
                    ActionType = "Import",
                    Description = "Imported user data",
                    Status = (int)LogStatus.Success,
                    Metadata = $"Imported {newUsers.Count} users"
                });
                return ("", errorList);
            }
            catch (Exception ex)
            {
                await trans.RollbackAsync();
                return ($"Transaction failed: {ex.Message}", users.Select(u => (u, "Rollback due to transaction failure")).ToList());
            }
        }

        public async Task<(string, MemoryStream?)> ExportData(string? userId)
        {
            var users = await GetUserList();
            if (users == null || users.Count == 0) return ("No users found.", null);
            var file = FileHandler.GenerateExcelFile(users);
            if (file == null) return ("Error export data.", null);

            string msg = await _log.WriteActivity(new AddUserLogVM
            {
                UserId = userId,
                ActionType = "Export",
                Description = "User data has been exported.",
                Status = (int)LogStatus.Success,
                Metadata = $"Exported {users.Count} users"
            });
            if (msg.Length > 0) return (msg, null);
            return ("", file);
        }
        private async Task<List<UserListVM>> GetUserList()
        {
            return await _context.Users.Include(x => x.UserRoles)
                .Include(x => x.Campus)
                .Include(x => x.Department)
                .Include(x => x.Major)
                .Include(x => x.Position)
                .Include(x => x.Specialization)
                .OrderBy(u => u.CreateAt)
                .Select(x => new UserListVM
                {
                    UserId = x.UserId,
                    FullName = x.FullName,
                    UserCode = x.UserCode,
                    Phone = x.Phone,
                    Email = x.Email,
                    Dob = x.Dob,
                    Sex = x.Sex,
                    AvatarUrl = x.AvatarUrl,
                    Address = x.Address,
                    Campus = x.Campus.Name,
                    Department = x.Department.Name,
                    Status = x.Status,
                    Major = x.Major.Name,
                    Position = x.Position.Name,
                    Specialization = x.Specialization.Name,
                    CreateAt = x.CreateAt,
                    UpdateAt = x.UpdateAt,
                    LastLogin = x.LastLogin,
                    CreateUser = x.CreateUser,
                    UpdateUser = x.UpdateUser,
                    LastLoginIp = x.LastLoginIp,
                    RoleId = x.UserRoles.Select(ur => ur.RoleId).ToList()
                }).ToListAsync();
        }

        public async Task<string> Create(CreateUserVM input, string? usertoken)
        {
            if (input == null) return "User data cannot be null.";
            var password = Utils.GenerateCharacter(8);
            var msg = Converter.StringToMD5(password, out string mkMd5);
            if (msg.Length > 0) return $"Error convert to MD5: {msg}";

            msg = await CheckEmailExisted(input.Email);
            if (msg.Length > 0) return msg;

            msg = await CheckPhoneExisted(input.Phone);
            if (msg.Length > 0) return msg;

            msg = await CheckUserCodeExisted(input.UserCode);
            if (msg.Length > 0) return msg;

            var validRoles = await _context.Roles.Where(r => input.RoleId.Contains(r.Id)).Select(r => r.Id).ToListAsync();
            if (validRoles.Count == 0) { return "No valid roles found."; }

            var validationError = await ValidateUserInput(input);
            if (!string.IsNullOrEmpty(validationError)) return validationError;

            var userId = Guid.NewGuid().ToString();
            var newFileKey = $"{UrlS3.Avatar}/{userId}/{input.Avatar.FileName}";
            var newUrl = await _s3Service.UploadFileAsync(newFileKey, input.Avatar);
            if (string.IsNullOrEmpty(newUrl)) return "Failed to upload new avatar.";

            using var trans = await _context.Database.BeginTransactionAsync();
            try
            {
                var newUser = new User
                {
                    UserId = userId,
                    FullName = input.FullName,
                    Phone = input.Phone,
                    UserCode = input.UserCode,
                    Email = input.Email,
                    Address = input.Address,
                    CampusId = input.CampusId,
                    CreateAt = DateTime.UtcNow,
                    CreateUser = usertoken,
                    DepartmentId = input.DepartmentId.IsEmpty() ? null : input.DepartmentId,
                    MajorId = input.MajorId.IsEmpty() ? null : input.MajorId,
                    PositionId = input.PositionId.IsEmpty() ? null : input.PositionId,
                    Sex = input.Sex,
                    SpecializationId = input.SpecializationId.IsEmpty() ? null : input.SpecializationId,
                    Status = input.Status,
                    Dob = input.Dob,
                    AvatarUrl = newUrl,
                    Password = mkMd5,
                };
                await _context.Users.AddAsync(newUser);
                await _context.SaveChangesAsync();

                msg = await EmailHandler.SendEmailAsync(input.Email, "Mật khẩu mới", $"Đây là mật khẩu mới của bạn: {password}");
                if (msg.Length > 0) { await trans.RollbackAsync(); return "Error send email: " + msg; }

                var userRoles = validRoles.Select(roleId => new UserRole
                {
                    UserId = newUser.UserId,
                    RoleId = roleId
                }).ToList();

                await _context.UserRoles.AddRangeAsync(userRoles);
                await _context.SaveChangesAsync();
                await trans.CommitAsync();

                var logMsg = await _log.WriteActivity(new AddUserLogVM
                {
                    UserId = usertoken,
                    ActionType = "Create",
                    Description = $"User {newUser.FullName} with UserCode {newUser.UserCode} has been created.",
                    Status = (int)LogStatus.Success,
                    Metadata = $"User ID: {newUser.UserId}, Roles: {string.Join(", ", validRoles)}"
                });
                if (logMsg.Length > 0) return $"Error when log activity: {logMsg}";
                return "";
            }
            catch (Exception ex)
            {
                await trans.RollbackAsync();
                return $"Unable to process the entity. Please try again or contact support: {ex.Message}";
            }
        }

        public async Task<string> Update(UpdateUserVM input, string? userToken)
        {
            if (userToken.IsEmpty()) return "Usertoken not found.";
            var existingUser = await _context.Users.Include(u => u.UserRoles).FirstOrDefaultAsync(u => u.UserId == input.UserId);
            if (existingUser == null) return "User not found.";

            var msg = await CheckPhoneExisted(input.Phone, input.UserId);
            if (msg.Length > 0) return msg;

            msg = await CheckUserCodeExisted(input.UserCode, input.UserId);
            if (msg.Length > 0) return msg;

            var validationError = await ValidateUserInput(input);
            if (!string.IsNullOrEmpty(validationError)) return validationError;

            using var trans = await _context.Database.BeginTransactionAsync();
            try
            {
                existingUser.UserCode = input.UserCode;
                existingUser.FullName = input.FullName;
                existingUser.Phone = input.Phone;
                existingUser.Address = input.Address;
                existingUser.CampusId = input.CampusId;
                existingUser.UpdateAt = DateTime.UtcNow;
                existingUser.UpdateUser = userToken;
                existingUser.DepartmentId = input.DepartmentId.IsEmpty() ? null : input.DepartmentId;
                existingUser.MajorId = input.MajorId.IsEmpty() ? null : input.MajorId;
                existingUser.PositionId = input.PositionId.IsEmpty() ? null : input.PositionId;
                existingUser.Sex = input.Sex;
                existingUser.SpecializationId = input.SpecializationId.IsEmpty() ? null : input.SpecializationId;
                existingUser.Status = input.Status;
                existingUser.Dob = input.Dob;

                if (!string.IsNullOrEmpty(existingUser.AvatarUrl))
                {
                    var fileKey = Helper.Common.ExtractKeyFromUrl(existingUser.AvatarUrl);
                    if (!string.IsNullOrEmpty(fileKey))
                    {
                        await _s3Service.DeleteFileAsync(fileKey);
                    }
                }
                var newFileKey = $"{UrlS3.Avatar}/{existingUser.UserId}/{input.Avatar.FileName}";
                var newUrl = await _s3Service.UploadFileAsync(newFileKey, input.Avatar);
                if (string.IsNullOrEmpty(newUrl)) return "Failed to upload new avatar.";
                existingUser.AvatarUrl = newUrl;

                var oldRoles = _context.UserRoles.Where(ur => ur.UserId == existingUser.UserId);
                _context.UserRoles.RemoveRange(oldRoles);

                var validRoles = await _context.Roles.Where(r => input.RoleId.Contains(r.Id)).Select(r => r.Id).ToListAsync();
                if (validRoles.Count == 0) { await trans.RollbackAsync(); return "Không tìm thấy vai trò hợp lệ."; }

                var newUserRoles = validRoles.Select(roleId => new UserRole
                {
                    UserId = existingUser.UserId,
                    RoleId = roleId
                }).ToList();

                await _context.UserRoles.AddRangeAsync(newUserRoles);
                await _context.SaveChangesAsync();
                await trans.CommitAsync();

                var logMsg = await _log.WriteActivity(new AddUserLogVM
                {
                    UserId = userToken,
                    ActionType = "Update User",
                    Description = $"User {existingUser.FullName} with Usercode {existingUser.UserCode} has been updated.",
                    Status = (int)LogStatus.Success,
                    Metadata = $"User ID: {existingUser.UserId}, Roles: {string.Join(", ", validRoles)}"
                });
                return "";
            }
            catch (Exception ex)
            {
                await trans.RollbackAsync();
                return $"Error when update: {ex.Message}";
            }
        }

        public async Task<string> ValidateUserInput(UserBaseVM input)
        {
            var roles = input.RoleId ?? new List<int>();

            bool hasTeacher = roles.Contains((int)RoleEnum.Lecture);
            bool hasSupervisor = roles.Contains((int)RoleEnum.Supervisor);
            bool hasAdmin = roles.Contains((int)RoleEnum.Admin);
            bool hasStudent = roles.Contains((int)RoleEnum.Student);

            bool isStaff = hasTeacher || hasSupervisor || hasAdmin;

            if (string.IsNullOrWhiteSpace(input.CampusId) || !await _context.Campuses.AnyAsync(c => c.Id == input.CampusId))
                return $"CampusId '{input.CampusId}' is invalid or required.";

            if (isStaff)
            {
                if (string.IsNullOrWhiteSpace(input.DepartmentId))
                    return "A DepartmentId is required for Teacher, Supervisor, or Admin.";
                if (!await _context.Departments.AnyAsync(d => d.Id == input.DepartmentId))
                    return $"DepartmentId '{input.DepartmentId}' is invalid.";

                if (string.IsNullOrWhiteSpace(input.PositionId))
                    return "A PositionId is required for Teacher, Supervisor, or Admin.";
                if (!await _context.Positions.AnyAsync(p => p.Id == input.PositionId))
                    return $"PositionId '{input.PositionId}' is invalid.";
            }
            if (hasStudent)
            {
                if (string.IsNullOrWhiteSpace(input.MajorId))
                    return "A MajorId is required for Student role.";
                if (!await _context.Majors.AnyAsync(m => m.Id == input.MajorId))
                    return $"MajorId '{input.MajorId}' is invalid.";
            }
            if (hasTeacher)
            {
                if (string.IsNullOrWhiteSpace(input.SpecializationId))
                    return "A SpecializationId is required for Teacher role.";
                if (!await _context.Specializations.AnyAsync(s => s.Id == input.SpecializationId))
                    return $"SpecializationId '{input.SpecializationId}' is invalid.";
            }
            return "";
        }

        public async Task<string> CheckEmailExisted(string? email)
        {
            if (string.IsNullOrEmpty(email) || !email.IsValidEmailFormat())
                return "Email is not valid";

            var exit = await _context.Users.AnyAsync(x => x.Email == email);
            if (exit) return "Email is already in use.";

            return string.Empty;
        }

        public async Task<string> CheckPhoneExisted(string? phone, string? userId = null)
        {
            if (string.IsNullOrEmpty(phone)) return "Phone number is not valid";

            var exit = await _context.Users.AnyAsync(u => u.Phone == phone && (userId == null || u.UserId != userId));
            if (exit) return "Phone number is already in use.";

            return string.Empty;
        }

        public async Task<string> CheckUserCodeExisted(string? code, string? userId = null)
        {
            if (string.IsNullOrEmpty(code)) return "UserCode is not valid";

            var exit = await _context.Users.AnyAsync(u => u.UserCode == code && (userId == null || u.UserId != userId));
            if (exit) return "UserCode is already in use.";

            return string.Empty;
        }

        public async Task<string> ChangeAvatar(ChangeAvatarVM avatarVM, string usertoken)
        {
            if (avatarVM == null || avatarVM.Avatar == null)
                return "Avatar data is not valid.";

            var user = await _context.Users.FindAsync(avatarVM.UserId);
            if (user == null) return "User not found.";

            if (!string.IsNullOrEmpty(user.AvatarUrl))
            {
                var fileKey = Helper.Common.ExtractKeyFromUrl(user.AvatarUrl);
                if (!string.IsNullOrEmpty(fileKey))
                {
                    await _s3Service.DeleteFileAsync(fileKey);
                }
            }
            var newFileKey = $"{UrlS3.Avatar}/{user.UserId}/{avatarVM.Avatar.FileName}";
            var newUrl = await _s3Service.UploadFileAsync(newFileKey, avatarVM.Avatar);
            if (string.IsNullOrEmpty(newUrl)) return "Failed to upload new avatar.";

            user.AvatarUrl = newUrl;
            user.UpdateAt = DateTime.UtcNow;
            user.UpdateUser = usertoken;

            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            return "";
        }
    }
}
