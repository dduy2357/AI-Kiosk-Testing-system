using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace API.Filter
{
    public class DateOnlySchemaFilter : ISchemaFilter
    {
        public void Apply(OpenApiSchema schema, SchemaFilterContext context)
        {
            foreach (var prop in context.Type.GetProperties())
            {
                if (prop.PropertyType == typeof(DateTime?) || prop.PropertyType == typeof(DateTime))
                {
                    if (schema.Properties.ContainsKey(prop.Name))
                    {
                        schema.Properties[prop.Name].Format = "date"; // chỉ ngày
                    }
                }
            }
        }
    }
}
