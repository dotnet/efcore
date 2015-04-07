using System.Linq.Expressions;

namespace Microsoft.Data.Entity.Relational.Query.Expressions
{
    public static class AliasExpressionExtensions
    {
        public static AliasExpression AsAliasExpression(this Expression expression)
        {
            return expression as AliasExpression;
        }

        public static ColumnExpression ColumnExpression(this AliasExpression aliasExpression)
        {
            return aliasExpression.Expression as ColumnExpression;
        }
    }
}
