using System.Linq.Expressions;
using System.Reflection;
using Microsoft.Data.Entity.Relational.Query.Expressions;

namespace Microsoft.Data.Entity.Relational.Query.Methods
{
    public class ContainsTranslator : IMethodCallTranslator
    {
        private static readonly MethodInfo _methodInfo
            = typeof(string).GetRuntimeMethod("Contains", new[] { typeof(string) });

        private static readonly MethodInfo _concat
            = typeof(string).GetRuntimeMethod("Concat", new[] { typeof(string), typeof(string) });

        public virtual Expression Translate(MethodCallExpression methodCallExpression)
        {
            if (ReferenceEquals(methodCallExpression.Method, _methodInfo))
            {
                var pattern = Expression.Add(new LiteralExpression("%"), methodCallExpression.Arguments[0], _concat);
                pattern = Expression.Add(pattern, new LiteralExpression("%"), _concat);
                return new LikeExpression(methodCallExpression.Object, pattern);
            }

            return null;
        }
    }
}