using API.Cached;
using API.Commons;
using API.Helper;
using API.Models;
using API.Services.Interfaces;
using API.ViewModels;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace API.Services
{
    public class AuthorizeService : IAuthorizeService
    {
        private readonly Sep490Context _context;
        private readonly IMapper _mapper;
        private readonly ILog _log;
        private readonly IDataCached _dataCached;

        public AuthorizeService(Sep490Context context, IMapper mapper, ILog log, IDataCached dataCached)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _dataCached = dataCached ?? throw new ArgumentNullException(nameof(dataCached));
            _log = log;
        }

        #region Permissions
        public async Task<(string, SearchResult?)> GetAllPermissions(SearchPermissionVM search)
        {
            // 1. Lấy tất cả permissions với điều kiện tìm kiếm
            var query = _context.Permissions.Include(p => p.Category).AsQueryable();
            if (!search.TextSearch.IsEmpty())
            {
                var text = search.TextSearch.ToLower();
                query = query.Where(p => p.Name.ToLower().Contains(text) || p.Resource.ToLower().Contains(text) ||
                    p.Action.ToLower().Contains(text) || (p.Category != null && p.Category.Description.ToLower().Contains(text)));
            }
            if (search.SortType.HasValue)
                query = search.SortType == (int)SortType.Ascending ? query.OrderBy(x => x.Name) : query.OrderByDescending(x => x.Name);

            // 2. Lấy tất cả permissions phù hợp với điều kiện tìm kiếm
            var allPermissions = await query.ToListAsync();
            if (!allPermissions.Any()) return ("No permission found.", null);

            // 3. Lấy tất cả categories
            var allCategories = await _context.PermissionCategories
                .Select(c => new { c.CategoryID, c.Description })
                .OrderBy(c => c.Description)
                .ToListAsync();

            // 4. Nhóm permissions theo category
            var grouped = allCategories.Select(cat =>
            {
                var permissionsInCat = allPermissions.Where(p => p.CategoryID == cat.CategoryID).ToList();
                return new CategoryPermissionsVM
                {
                    CategoryId = cat.CategoryID,
                    Description = cat.Description,
                    Permissions = _mapper.Map<List<PermissionVM>>(permissionsInCat)
                };
            }).ToList();

            // 5. Áp dụng phân trang trên danh sách grouped
            var totalCount = grouped.Count();
            var totalPage = (int)Math.Ceiling(totalCount / (double)search.PageSize);

            var pagedGrouped = grouped
                .Skip((search.CurrentPage - 1) * search.PageSize)
                .Take(search.PageSize)
                .ToList();

            return ("", new SearchResult
            {
                Result = pagedGrouped,
                TotalPage = totalPage,
                PageSize = search.PageSize,
                CurrentPage = search.CurrentPage,
                Total = totalCount
            });
        }

        public async Task<(string, List<PermissionVM>)> GetAllPermissions()
        {
            var permissions = await _context.Permissions.ToListAsync();

            if (permissions == null || !permissions.Any()) return ("No permissions found.", new List<PermissionVM>());
            var mappedPermission = _mapper.Map<List<PermissionVM>>(permissions);
            return ("", mappedPermission);
        }

        public async Task<(string, PermissionVM?)> GetOnePermission(string permissionId)
        {
            if (permissionId.IsEmpty()) return ("Permission ID cannot be empty.", null);
            var permission = await _context.Permissions.FindAsync(permissionId);

            if (permission == null) return ("Permission not found.", null);

            var mappedPermission = _mapper.Map<PermissionVM>(permission);
            return ("", mappedPermission);
        }
        public async Task<string> CreateUpdatePermission(CreateUpdatePermissionVM permission, string usertoken)
        {
            var existedCategory = await _context.PermissionCategories.FirstOrDefaultAsync(c => c.CategoryID == permission.CategoryID);
            if (existedCategory == null) return "Category permission is not valid.";

            string normalizedName = permission.Name.Trim().ToLower();
            string normalizedResource = permission.Resource.Trim().ToLower();
            bool nameExists = await _context.Permissions.AnyAsync(p => (permission.Id.IsEmpty() || p.Id != permission.Id)
                && (p.Name.ToLower() == normalizedName || p.Resource.ToLower() == normalizedResource));
            if (nameExists) return $"Permission name or resource already exists.";

            if (permission.Id.IsEmpty())
            {
                var newPermission = new Permission
                {
                    Id = Guid.NewGuid().ToString(),
                    Action = permission.Action,
                    Name = permission.Name,
                    Resource = permission.Resource,
                    CategoryID = permission.CategoryID,
                    IsActive = true,
                    Description = permission.Description,
                    CreatedAt = DateTime.UtcNow,
                };
                await _context.Permissions.AddAsync(newPermission);
            }
            else
            {
                var existingPermission = await _context.Permissions.FindAsync(permission.Id);
                if (existingPermission == null) return "Permission not found.";

                existingPermission.Action = permission.Action;
                existingPermission.Name = permission.Name;
                existingPermission.Resource = permission.Resource;
                existingPermission.CategoryID = permission.CategoryID;
                existingPermission.Description = permission.Description;
                existingPermission.IsActive = permission.IsActive;
                existingPermission.UpdatedAt = DateTime.UtcNow;

                _context.Permissions.Update(existingPermission);
            }
            await _context.SaveChangesAsync();
            await _dataCached.RemoveCache(Constant.PERMISSIONS);

            var msg = await _log.WriteActivity(new AddUserLogVM
            {
                ActionType = (permission.Id.IsEmpty() ? "Created" : "Updated") + " permission",
                UserId = usertoken,
                Description = "A permission has been " + (permission.Id.IsEmpty() ? "created." : "updated."),
                Metadata = "Action name: " + permission.Action + ", Resource: " + permission.Resource,
                ObjectId = existedCategory?.CategoryID ?? string.Empty,
                Status = (int)LogStatus.Success
            });
            if (msg.Length > 0) return msg;
            return "";
        }

        public async Task<string> DeletePermission(string permissionId, string usertoken)
        {
            if (permissionId.IsEmpty()) return "Permission ID cannot be empty.";

            var permission = await _context.Permissions.FindAsync(permissionId);
            if (permission == null) return "Permission not found.";

            _context.Permissions.Remove(permission);
            await _context.SaveChangesAsync();
            await _dataCached.RemoveCache(Constant.PERMISSIONS);
            await _dataCached.ClearAllUserPermissionCache();

            var msg = await _log.WriteActivity(new AddUserLogVM
            {
                ActionType = "Deleted",
                Description = "A permission has deleted.",
                UserId = usertoken,
                Metadata = "Name: " + permission.Name + ", CategoryId: " + permission.CategoryID,
                ObjectId = permissionId,
                Status = (int)LogStatus.Success
            });
            return "";
        }

        public async Task<string> ToggleActivePermission(string permissionId, string usertoken)
        {
            if (permissionId.IsEmpty()) return "Permission ID cannot be empty.";

            var permission = await _context.Permissions.FindAsync(permissionId);
            if (permission == null) return "Permission not found.";

            permission.IsActive = !permission.IsActive;
            _context.Permissions.Update(permission);
            await _context.SaveChangesAsync();
            await _dataCached.RemoveCache(Constant.PERMISSIONS);
            await _dataCached.ClearAllUserPermissionCache();

            var msg = await _log.WriteActivity(new AddUserLogVM
            {
                ActionType = permission.IsActive ? "Activated" : "Deactivated",
                Description = "Permission has been " + (permission.IsActive ? "activated" : "deactivated") + ".",
                UserId = usertoken,
                Metadata = "",
                ObjectId = permissionId,
                Status = (int)LogStatus.Success
            });
            return msg.Length > 0 ? msg : "";
        }

        #endregion

        #region Roles
        public async Task<(string, List<RoleVM>?)> GetAllRoles()
        {
            var roles = await _context.Roles
                .Select(r => new RoleVM
                {
                    Id = r.Id,
                    Name = r.Name
                }).ToListAsync();

            if (roles == null || !roles.Any()) return ("No roles found.", null);
            return ("", roles);
        }

        public async Task<(string, SearchResult)> GetAllRolesPermissions(RoleSearchVM search)
        {
            var query = _context.Roles.Include(r => r.RolePermissions)
                .ThenInclude(rp => rp.Permission).ThenInclude(p => p.Category)
                .AsQueryable();

            if (search.RoleId > 0)
                query = query.Where(r => r.Id == search.RoleId);

            if (!search.TextSearch.IsEmpty())
            {
                var text = search.TextSearch!.ToLower().Trim();
                query = query.Where(r => r.Name.ToLower().Contains(text) || r.RolePermissions.Any(rp => rp.Permission != null
                && ((rp.Permission.Name ?? "").ToLower().Contains(text)
                || (rp.Permission.Resource ?? "").ToLower().Contains(text)
                || (rp.Permission.Action ?? "").ToLower().Contains(text))));
            }

            var totalCount = await query.CountAsync();
            var totalPage = (int)Math.Ceiling(totalCount / (double)search.PageSize);

            var pagedRoles = await query
                .OrderBy(r => r.Name)
                .Skip((search.CurrentPage - 1) * search.PageSize)
                .Take(search.PageSize)
                .ToListAsync();

            // Mapping
            var result = pagedRoles.Select(role => new RoleWithPermissionsVM
            {
                RoleId = role.Id,
                RoleName = role.Name,
                Description = role.Description,
                IsActive = role.IsActive,
                Categories = role.RolePermissions
                    .Where(rp => rp.Permission != null && rp.Permission.Category != null)
                    .GroupBy(rp => new { rp.Permission.CategoryID, rp.Permission.Category.Description })
                    .Select(g => new CategoryPermissionsVM
                    {
                        CategoryId = g.Key.CategoryID,
                        Description = g.Key.Description,
                        Permissions = g.Select(rp => new PermissionVM
                        {
                            Id = rp.Permission.Id,
                            Name = rp.Permission.Name,
                            Action = rp.Permission.Action,
                            Resource = rp.Permission.Resource,
                            IsActive = rp.Permission.IsActive,
                            Description = rp.Permission.Description,
                        }).ToList()
                    }).ToList()
            }).ToList();

            return ("", new SearchResult
            {
                Result = result,
                TotalPage = totalPage,
                PageSize = search.PageSize,
                CurrentPage = search.CurrentPage,
                Total = totalCount
            });
        }

        public async Task<string> CreateUpdateRole(CreateUpdateRoleVM role, string usertoken)
        {
            if (role == null) return "Invalid role data.";

            var roles = await _context.Roles.ToListAsync();
            if (roles == null) return "No role found.";

            if (role.Id <= 0)
            {
                var existingRole = roles.FirstOrDefault(r => r.Name.ToLower() == role.Name.ToLower());
                if (existingRole != null) return "Role name already exists.";

                var newRole = new Role
                {
                    Name = role.Name,
                    Description = role.Description,
                    IsActive = role.IsActive,
                };
                await _context.Roles.AddAsync(newRole);
            }
            else
            {
                var existingRole = await _context.Roles.FindAsync(role.Id);
                if (existingRole == null) return "Role not found.";

                var existingName = roles.Any(r => r.Id != role.Id && r.Name == role.Name);
                if (existingName) return "Role name already exists.";

                existingRole.Name = role.Name;
                existingRole.Description = role.Description;
                existingRole.IsActive = role.IsActive;
                _context.Roles.Update(existingRole);
            }
            await _context.SaveChangesAsync();
            var msg = await _log.WriteActivity(new AddUserLogVM
            {
                ActionType = "",
                Description = "Role " + (role.Id <= 0 ? "created" : "updated") + " successfully.",
                UserId = usertoken ?? string.Empty,
                Metadata = "",
                ObjectId = role.Id.ToString(),
                Status = (int)LogStatus.Success
            });
            if (msg.Length > 0) return msg;
            return "";
        }

        public async Task<string> ToggleActive(int roleId, string usertoken)
        {
            if (roleId <= 0) return "Role ID must be greater than 0.";

            var role = await _context.Roles.FindAsync(roleId);
            if (role == null) return "Role not found.";

            role.IsActive = !role.IsActive;
            _context.Roles.Update(role);
            await _context.SaveChangesAsync();

            var msg = await _log.WriteActivity(new AddUserLogVM
            {
                ActionType = "Updated Role",
                Description = $"Role [{role.Id}] has been {(role.IsActive == true ? "active" : "deactivate")}.",
                UserId = usertoken,
                Metadata = "",
                ObjectId = roleId.ToString(),
                Status = (int)LogStatus.Success
            });
            if (msg.Length > 0) return msg;
            return "";
        }

        public async Task<string> DeleteRole(int roleId, string usertoken)
        {
            if (roleId <= 0) return "Role ID must be greater than 0.";

            var role = await _context.Roles.FindAsync(roleId);
            if (role == null) return "Role not found.";

            _context.Roles.Remove(role);
            await _context.SaveChangesAsync();

            var msg = await _log.WriteActivity(new AddUserLogVM
            {
                ActionType = "Deleted",
                Description = "A role has deleted.",
                UserId = usertoken,
                Metadata = "",
                ObjectId = roleId.ToString(),
                Status = (int)LogStatus.Success
            });
            if (msg.Length > 0) return msg;
            return "";
        }
        #endregion

        #region User Role Permissions

        public async Task<string> AssignRolesToUser(AddRoleToUserVM userRole, string usertoken)
        {
            if (string.IsNullOrEmpty(userRole.UserId) || userRole.RoleId.IsObjectEmpty())
                return "User ID cannot be empty and Role ID must be greater than 0.";

            var existingUser = await _context.Users.FindAsync(userRole.UserId);
            if (existingUser == null) return "User not found.";

            var validRoles = await _context.Roles.Where(r => userRole.RoleId.Contains(r.Id)).Select(r => r.Id).ToListAsync();
            if (validRoles.Count == 0) return "No valid roles found.";

            var currentUserRoles = await _context.UserRoles.Where(ur => ur.UserId == userRole.UserId).ToListAsync();
            if (currentUserRoles.Any())
                _context.UserRoles.RemoveRange(currentUserRoles);

            _context.UserRoles.AddRange(validRoles.Select(roleId => new UserRole
            {
                UserId = userRole.UserId,
                RoleId = roleId
            }));
            await _context.SaveChangesAsync();
            await _dataCached.RemoveCache($"{Constant.USER_PERMISSION}_{userRole.UserId}");

            await _context.SaveChangesAsync();
            var msg = await _log.WriteActivity(new AddUserLogVM
            {
                ActionType = "Assigned Roles",
                Description = "Roles assigned to user: " + existingUser.UserCode,
                UserId = usertoken,
                Metadata = "RoleId: " + string.Join(", ", userRole.RoleId.ToString()),
                ObjectId = userRole.UserId,
                Status = (int)LogStatus.Success
            });
            return "";
        }

        public async Task<string> AddPermissionsToRole(AddPermissionToRoleVM rolePermission, string usertoken)
        {
            if (rolePermission.RoleId <= 0 || rolePermission.Permissions.Count == 0)
                return "Role ID must be greater than 0 and Permission ID cannot be empty.";

            var existingRole = await _context.Roles.FindAsync(rolePermission.RoleId);
            if (existingRole == null) return "Role not found.";

            var currentPermissions = await _context.RolePermissions
                   .Where(rp => rp.RoleId == rolePermission.RoleId)
                   .Select(rp => rp.PermissionId)
                   .ToListAsync();

            var newPermissionIds = rolePermission.Permissions.Distinct().ToList();
            var permissionsToAdd = newPermissionIds.Except(currentPermissions).ToList();
            if (!permissionsToAdd.Any())
                return "All these permissions are already assigned to this role.";

            if (permissionsToAdd.Any())
            {
                var newRolePermissions = permissionsToAdd.Select(pid => new RolePermission
                {
                    RoleId = rolePermission.RoleId,
                    PermissionId = pid
                });
                _context.RolePermissions.AddRange(newRolePermissions);
            }

            await _context.SaveChangesAsync();
            await _dataCached.ClearCache();
            var msg = await _log.WriteActivity(new AddUserLogVM
            {
                ActionType = "Added Permissions",
                Description = "Permissions added to role.",
                UserId = usertoken,
                Metadata = "",
                ObjectId = rolePermission.RoleId.ToString(),
                Status = (int)LogStatus.Success
            });
            if (msg.Length > 0) return msg;
            return "";
        }

        public async Task<string> RemovePermissionsFromRole(AddPermissionToRoleVM rolePermission, string usertoken)
        {
            if (rolePermission.RoleId <= 0 || rolePermission.Permissions.Count == 0)
                return "Role ID must be greater than 0 and Permission list cannot be empty.";

            var existingRole = await _context.Roles.FindAsync(rolePermission.RoleId);
            if (existingRole == null) return "Role not found.";

            var rolePermissionsToRemove = await _context.RolePermissions
                .Where(rp => rp.RoleId == rolePermission.RoleId && rolePermission.Permissions.Contains(rp.PermissionId))
                .ToListAsync();

            if (!rolePermissionsToRemove.Any())
                return "No matching permissions found to remove.";

            _context.RolePermissions.RemoveRange(rolePermissionsToRemove);
            await _context.SaveChangesAsync();
            await _dataCached.ClearCache();

            string msg = await _log.WriteActivity(new AddUserLogVM
            {
                ActionType = "Removed Permissions",
                Description = "Permissions removed from role.",
                UserId = usertoken,
                Metadata = "",
                ObjectId = rolePermission.RoleId.ToString(),
                Status = (int)LogStatus.Success
            });
            if (msg.Length > 0) return msg;
            return "";
        }

        public async Task<(string, List<PermissionVM>?)> GetPermissionsFromUserId(string? userId)
        {
            if (userId.IsEmpty()) return ("User ID cannot be empty.", null);
            var permissions = await (from ur in _context.UserRoles
                                     join rp in _context.RolePermissions on ur.RoleId equals rp.RoleId
                                     join p in _context.Permissions on rp.PermissionId equals p.Id
                                     where ur.UserId == userId && p.IsActive == true
                                     select p)
                           .Distinct()
                           .ToListAsync();

            var mapper = _mapper.Map<List<PermissionVM>>(permissions);
            return ("", mapper);
        }

        public async Task<(string, UserRoleVM?)> GetUserRoles(string userId)
        {
            if (userId.IsEmpty()) return ("User ID cannot be empty.", null);
            var userRoles = await _context.UserRoles
                .Where(ur => ur.UserId == userId)
                .Select(ur => new RoleVM
                {
                    Id = ur.Role.Id,
                    Name = ur.Role.Name
                }).ToListAsync();
            if (userRoles.Count == 0) return ("User has no roles assigned.", null);
            return ("", new UserRoleVM
            {
                UserId = userId,
                Roles = userRoles
            });
        }
        #endregion

        #region Category Permisison 
        public async Task<string> AddCategoryPermissionsToRole(AddCategoryPermissionToRoleVM permission, string userToken)
        {
            var role = await _context.Roles.FindAsync(permission.RoleId);
            if (role == null) return "Role not found.";

            var allPermissionsInCategory = await _context.Permissions
                .Where(p => p.CategoryID == permission.CategoryId && p.IsActive)
                .Select(p => p.Id)
                .ToListAsync();

            if (!allPermissionsInCategory.Any())
                return "No active permissions found in this category.";

            var existingPermissionIds = await _context.RolePermissions
                .Where(rp => rp.RoleId == permission.RoleId)
                .Select(rp => rp.PermissionId)
                .ToListAsync();

            var newPermissionIds = allPermissionsInCategory.Except(existingPermissionIds).ToList();
            if (!newPermissionIds.Any())
                return "All permissions in this category are already assigned to this role.";

            var rolePermissionsToAdd = newPermissionIds.Select(pid => new RolePermission
            {
                RoleId = permission.RoleId,
                PermissionId = pid
            });

            _context.RolePermissions.AddRange(rolePermissionsToAdd);
            await _context.SaveChangesAsync();
            await _dataCached.ClearCache();

            string msg = await _log.WriteActivity(new AddUserLogVM
            {
                ActionType = "Added Category Permissions",
                Description = $"Added permissions from category {permission.CategoryId} to role {permission.RoleId}.",
                UserId = userToken,
                Metadata = "",
                ObjectId = permission.RoleId.ToString(),
                Status = (int)LogStatus.Success
            });
            if (msg.Length > 0) return msg;
            return "";
        }

        public async Task<(string, SearchResult?)> GetAllCategoryPermissions(SearchRequestVM search)
        {
            var query = _context.PermissionCategories.AsNoTracking().AsQueryable();
            if (!search.TextSearch.IsEmpty())
            {
                var text = search.TextSearch.ToLower();
                query = query.Where(c => c.Description.ToLower().Contains(text));
            }

            var totalCount = await query.CountAsync();
            var totalPage = (int)Math.Ceiling(totalCount / (double)search.PageSize);

            var categories = await query
                .OrderBy(c => c.Description)
                .Skip((search.CurrentPage - 1) * search.PageSize)
                .Take(search.PageSize)
                .ToListAsync();

            if (!categories.Any()) return ("No category permission found.", null);

            var result = categories.Select(c => new CategoryPermissionsVM
            {
                CategoryId = c.CategoryID,
                Description = c.Description,
            }).ToList();

            return ("", new SearchResult
            {
                Result = result,
                TotalPage = totalPage,
                PageSize = search.PageSize,
                CurrentPage = search.CurrentPage,
                Total = totalCount
            });
        }
        public async Task<(string, CategoryPermissionsVM?)> GetOneCategoryPermission(string categoryId)
        {
            if (categoryId.IsEmpty()) return ("Category ID cannot be empty.", null);
            var cate = await _context.PermissionCategories.Select(c => new CategoryPermissionsVM
            {
                CategoryId = c.CategoryID,
                Description = c.Description,
            }).FirstOrDefaultAsync(x => x.CategoryId == categoryId);
            if (cate == null) return ("Category permission not found.", null);

            return ("", cate);
        }

        public async Task<string> CreateUpdateCategoryPermission(CreateUpdateCategoryPermissionsVM category, string usertoken)
        {
            if (category == null) return "Invalid category permission data.";

            if (category.CategoryID.IsEmpty())
            {
                var existingCategory = await _context.PermissionCategories
                    .FirstOrDefaultAsync(c => c.Description == category.Description);
                if (existingCategory != null) return "Category permission description already exists.";

                var newCategory = new PermissionCategory
                {
                    CategoryID = Guid.NewGuid().ToString(),
                    Description = category.Description
                };
                await _context.PermissionCategories.AddAsync(newCategory);
            }
            else
            {
                var existingCategory = await _context.PermissionCategories.FindAsync(category.CategoryID);
                if (existingCategory == null) return "Category permission not found.";

                existingCategory.Description = category.Description;
                _context.PermissionCategories.Update(existingCategory);
            }
            await _context.SaveChangesAsync();
            await _dataCached.RemoveCache(Constant.PERMISSIONS);
            await _dataCached.ClearAllUserPermissionCache();

            var msg = await _log.WriteActivity(new AddUserLogVM
            {
                ActionType = "",
                UserId = usertoken ?? string.Empty,
                Description = string.IsNullOrEmpty(category.CategoryID) ? "Created" : "Updated" + " category permission",
                Metadata = "",
                ObjectId = category.CategoryID,
                Status = (int)LogStatus.Success
            });
            if (msg.Length > 0) return msg;
            return "";
        }

        public async Task<string> DeleteCategoryPermission(string categoryId, string usertoken)
        {
            if (categoryId.IsEmpty()) return "Category ID cannot be empty.";

            var category = await _context.PermissionCategories.FindAsync(categoryId);
            if (category == null) return "Category permission not found.";

            _context.PermissionCategories.Remove(category);
            await _context.SaveChangesAsync();
            await _dataCached.ClearCache();
            var msg = await _log.WriteActivity(new AddUserLogVM
            {
                ActionType = "Deleted",
                Description = "Category permission deleted.",
                UserId = usertoken,
                Metadata = "",
                ObjectId = categoryId,
                Status = (int)LogStatus.Success
            });
            return "";
        }
        #endregion

    }
}
