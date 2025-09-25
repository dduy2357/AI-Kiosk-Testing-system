using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using API.Commons;
using API.ViewModels.Token;
using API.Cached;
using API.Controllers.Authentication;
using System.Text.RegularExpressions;

namespace API.Attributes
{
    public class AuthenPermissionAttribute : Attribute, IAsyncActionFilter
    {
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            // ✅ Kiểm tra xem action có attribute [SkipPermission] không
            var endpoint = context.HttpContext.GetEndpoint();
            var hasSkipPermission = endpoint?.Metadata.GetMetadata<SkipPermissionAttribute>() != null;

            if (hasSkipPermission)
            {
                await next();
                return;
            }

            if (context.Controller is not Authentication authController)
            {
                await next();
                return;
            }

            var result = authController.ResultCheckToken;
            if (!result.isOk)
            {
                context.Result = Utils.GetCacheResponse(result);
                return;
            }

            var userToken = authController.UserToken;

            // ✅ Lấy DataCached từ DI container
            var dataCached = context.HttpContext.RequestServices.GetRequiredService<IDataCached>();
            if (dataCached == null)
            {
                context.Result = new ContentResult
                {
                    Content = "{\"Status\":1,\"StatusCode\":500,\"Object\":\"Internal Server Error\",\"isOk\":false,\"isError\":true}",
                    ContentType = "application/json",
                    StatusCode = 500
                };
                return;
            }

            // Kiểm tra quyền
            string msg = await CheckPermission(context, userToken, dataCached);
            if (!string.IsNullOrEmpty(msg))
            {
                context.Result = new ContentResult
                {
                    Content = "{\"Status\":1,\"StatusCode\":403,\"Object\":\"You do not have permission to perform this function..\",\"isOk\":false,\"isError\":true}",
                    ContentType = "application/json",
                    StatusCode = 403
                };
                return;
            }

            await next();
        }

        private async Task<string> CheckPermission(ActionExecutingContext context, UserToken? userToken, IDataCached dataCached)
        {
            string pathApi = context.HttpContext.Request.Path;

            var (msg, data) = await dataCached.GetFunctions();
            if (!string.IsNullOrEmpty(msg)) return msg;

            // Nếu API không cần phân quyền thì bỏ qua
            if (data != null && !data.Any(r => Regex.IsMatch(pathApi, GetPathRegex(r.Resource)))) 
                return "";

            var userPemissions = await dataCached.GetUserPermissions(userToken?.UserID);
            if (!string.IsNullOrEmpty(userPemissions.Item1)) return msg;

            if (userPemissions.Item2.IsObjectEmpty())
                return "ERROR";

            var userFunction = userPemissions.Item2.FirstOrDefault(r => Regex.IsMatch(pathApi, GetPathRegex(r.Resource)));
            if (userFunction == null)
                return "ERROR";

            return "";
        }

        public string GetPathRegex(string path)
        {
            if (!path.StartsWith("/"))
                path = "/" + path;
            return "^" + Regex.Replace(path, "\\{[^/]+\\}", "[^/]+") + "$";
        }
    }

}
