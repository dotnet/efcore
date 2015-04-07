using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Relational.Query.Sql;
using Microsoft.Data.Entity.Utilities;
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.Parsing;

namespace Microsoft.Data.Entity.Relational.Query.Expressions
{
    public class AliasExpression : ExtensionExpression
    {
        public AliasExpression([NotNull] Expression expression)
            : base(Check.NotNull(expression, nameof(expression)).Type)
        {
            Expression = expression;
        }

        public AliasExpression([CanBeNull] string alias, [NotNull] Expression expression)
            : base(Check.NotNull(expression, nameof(expression)).Type)
        {
            Alias = alias;
            Expression = expression;
        }

        public virtual string Alias{ get; [param: NotNull] set; }

        public virtual Expression Expression { get; [param: NotNull] set; }

        public virtual bool Projected { get; set; } = false;


        public override Expression Accept([NotNull] ExpressionTreeVisitor visitor)
        {
            Check.NotNull(visitor, nameof(visitor));

            var specificVisitor = visitor as ISqlExpressionVisitor;

            return specificVisitor != null
                ? specificVisitor.VisitAliasExpression(this)
                : base.Accept(visitor);
        }


        protected override Expression VisitChildren(ExpressionTreeVisitor visitor)
        {
            return this;
        }

        public override string ToString()
        {
            return this.ColumnExpression()?.ToString() ?? Expression.NodeType + " " + Alias;
        }
    }
}
