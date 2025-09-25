using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace API.Filter
{
    public class EnumSchemaFilter : ISchemaFilter
    {
        public void Apply(OpenApiSchema schema, SchemaFilterContext context)
        {
            if (!context.Type.IsEnum) return;

            schema.Enum.Clear(); // Xóa số mặc định

            var enumType = context.Type;
            var enumNames = Enum.GetNames(enumType);

            foreach (var name in enumNames)
            {
                var field = enumType.GetField(name);
                var displayAttr = field?.GetCustomAttribute<DisplayAttribute>();
                var label = displayAttr?.Name ?? name;

                schema.Enum.Add(new OpenApiString(label));
            }

            schema.Type = "string";
            schema.Format = null;
        }
    }
}
