using System;
using System.Linq.Expressions;
using JetBrains.Annotations;

namespace Microsoft.Data.Entity
{
    public abstract class ExtensionExpression : Expression
    {
        private readonly Type _type;

        public ExtensionExpression([NotNull]Type type)
        {
            _type = type;
        }

        public override ExpressionType NodeType
        {
            get { return ExpressionType.Extension; }
        }

        public override Type Type
        {
            get { return _type; }
        }

        protected abstract override Expression VisitChildren(ExpressionVisitor visitor);
    }
}