// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.XuGu.Query.Expressions.Internal
{
    /// <summary>
    /// Represents a MySQL JSON operator traversing a JSON document with a path (i.e. x->y or x->>y)
    /// </summary>
    public class XGJsonTraversalExpression : SqlExpression, IEquatable<XGJsonTraversalExpression>
    {
        private static ConstructorInfo _quotingConstructor;

        /// <summary>
        /// The JSON column.
        /// </summary>
        [NotNull]
        public virtual SqlExpression Expression { get; }

        /// <summary>
        /// The path inside the JSON column.
        /// </summary>
        [NotNull]
        public virtual IReadOnlyList<SqlExpression> Path { get; }

        /// <summary>
        /// Whether the text-returning operator (x->>y) or the object-returning operator (x->y) is used.
        /// </summary>
        public virtual bool ReturnsText { get; }

        public XGJsonTraversalExpression(
            [NotNull] SqlExpression expression,
            bool returnsText,
            [NotNull] Type type,
            [CanBeNull] RelationalTypeMapping typeMapping)
            : this(expression, new SqlExpression[0], returnsText, type, typeMapping)
        {
        }

        protected XGJsonTraversalExpression(
            [NotNull] SqlExpression expression,
            [NotNull] IReadOnlyList<SqlExpression> path,
            bool returnsText,
            [NotNull] Type type,
            [CanBeNull] RelationalTypeMapping typeMapping)
            : base(type, typeMapping)
        {
            if (returnsText && !TypeReturnsText(type))
                throw new ArgumentException($"{nameof(type)} is not a type that returns text", nameof(type));

            Expression = expression;
            Path = path;
            ReturnsText = returnsText;
        }

        public virtual XGJsonTraversalExpression Clone(bool returnsText, Type type, RelationalTypeMapping typeMapping)
            => new XGJsonTraversalExpression(Expression, Path, returnsText, type, typeMapping);

        protected override Expression VisitChildren(ExpressionVisitor visitor)
            => Update(
                (SqlExpression)visitor.Visit(Expression),
                Path.Select(p => (SqlExpression)visitor.Visit(p)).ToArray());

        /// <inheritdoc />
        public override Expression Quote()
            => New(
                _quotingConstructor ??= typeof(XGInlinedParameterExpression).GetConstructor(
                    [typeof(SqlExpression), typeof(IReadOnlyList<SqlExpression>), typeof(bool), typeof(Type), typeof(RelationalTypeMapping)])!,
                Expression.Quote(),
                NewArrayInit(typeof(SqlExpression), Path.Select(p => p.Quote())),
                Constant(ReturnsText),
                RelationalExpressionQuotingUtilities.QuoteTypeMapping(TypeMapping));

        public virtual XGJsonTraversalExpression Update(
            [NotNull] SqlExpression expression,
            [NotNull] IReadOnlyList<SqlExpression> path)
            => expression == Expression &&
               path.Count == Path.Count &&
               path.Zip(Path, (x, y) => (x, y)).All(tup => tup.x == tup.y)
                ? this
                : new XGJsonTraversalExpression(expression, path, ReturnsText, Type, TypeMapping);

        public virtual XGJsonTraversalExpression Append([NotNull] SqlExpression pathComponent, Type returnType, RelationalTypeMapping typeMapping)
        {
            var newPath = new SqlExpression[Path.Count + 1];
            for (var i = 0; i < Path.Count; i++)
            {
                newPath[i] = Path[i];
            }
            newPath[newPath.Length - 1] = pathComponent;
            return new XGJsonTraversalExpression(Expression, newPath, ReturnsText, returnType.UnwrapNullableType(), typeMapping);
        }

        public override bool Equals(object obj) => Equals(obj as XGJsonTraversalExpression);

        public virtual bool Equals(XGJsonTraversalExpression other)
            => ReferenceEquals(this, other) ||
               other is object &&
               base.Equals(other) &&
               Equals(Expression, other.Expression) &&
               Path.Count == other.Path.Count &&
               Path.Zip(other.Path, (x, y) => (x, y)).All(tup => tup.x == tup.y);

        public override int GetHashCode() => HashCode.Combine(base.GetHashCode(), Expression, Path);

        protected override void Print(ExpressionPrinter expressionPrinter)
        {
            expressionPrinter.Visit(Expression);
            expressionPrinter.Append(ReturnsText ? "->>" : "->");
            expressionPrinter.Append("'$");
            foreach (var location in Path)
            {
                expressionPrinter.Append(".");
                expressionPrinter.Visit(location);
            }
            expressionPrinter.Append("'");
        }

        public override string ToString() => $"{Expression}{(ReturnsText ? "->>" : "->")}{Path}";

        public static bool TypeReturnsText(Type type)
            => type == typeof(string) ||
               type == typeof(Guid);
    }
}
