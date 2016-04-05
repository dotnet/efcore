// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query.Sql;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Query.Expressions
{
    public class PropertyParameterExpression : Expression
    {
        public PropertyParameterExpression([NotNull] string name, [NotNull] IProperty property)
        {
            Check.NotEmpty(name, nameof(name));
            Check.NotNull(property, nameof(property));

            Name = name;
            Property = property;
        }

        public virtual string Name { get; }

#pragma warning disable 108
        public virtual IProperty Property { get; }
#pragma warning restore 108

        public virtual string PropertyParameterName => $"{Name}_{Property.Name}";

        public override ExpressionType NodeType => ExpressionType.Extension;

        public override Type Type => Property.ClrType;

        public override string ToString() => Name;

        protected override Expression VisitChildren(ExpressionVisitor visitor) => this;

        protected override Expression Accept(ExpressionVisitor visitor)
        {
            Check.NotNull(visitor, nameof(visitor));

            var specificVisitor = visitor as ISqlExpressionVisitor;

            return specificVisitor != null
                ? specificVisitor.VisitPropertyParameter(this)
                : base.Accept(visitor);
        }
    }
}
