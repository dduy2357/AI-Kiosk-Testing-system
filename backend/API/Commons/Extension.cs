using System.Linq.Expressions;
using System.Reflection;

namespace API.Commons
{
    public static class PredicateBuilder 
    {
        private static readonly MethodInfo TrimMethod = typeof(string).GetMethod(nameof(string.Trim), Type.EmptyTypes)!;
        private static readonly MethodInfo ToLowerMethod = typeof(string).GetMethod(nameof(string.ToLower), Type.EmptyTypes)!;
        private static readonly MethodInfo ContainsMethod = typeof(string).GetMethod(nameof(string.Contains), new[] { typeof(string) })!;

        public static Expression ContainsIgnoreCase(
            ParameterExpression param,
            string propertyPath,
            string keyword)
        {
            // property path ví dụ: "User.UserCode"
            Expression? expr = param;
            foreach (var prop in propertyPath.Split('.'))
            {
                expr = Expression.PropertyOrField(expr!, prop);
            }

            // expr != null
            var notNull = Expression.NotEqual(expr!, Expression.Constant(null, typeof(string)));
            // expr.ToLower().Contains(keyword)
            var lower = Expression.Call(expr!, ToLowerMethod);
            var contains = Expression.Call(lower, ContainsMethod, Expression.Constant(keyword));

            return Expression.AndAlso(notNull, contains);
        }

        public static Expression OrElse(this Expression left, Expression right) => Expression.OrElse(left, right);
    }

    public static class DbContextExtensions
    {
        //public static async Task<string> TrySaveChangesAsync(this DbContext context)
        //{
        //    try
        //    {
        //        await context.SaveChangesAsync();
        //        return ""; 
        //    }
        //    catch (DbUpdateException ex)
        //    {
        //        return "Unable to process the entity. Please try again or contact support. " + ex.Message;
        //    }
        //    catch (Exception ex)
        //    {
        //        return "System error: " + ex.Message;
        //    }
        //}
    }
}
