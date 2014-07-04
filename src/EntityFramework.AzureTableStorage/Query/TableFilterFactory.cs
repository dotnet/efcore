// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.Data.Entity.AzureTableStorage.Metadata;
using Microsoft.Data.Entity.AzureTableStorage.Utilities;
using Microsoft.Data.Entity.Metadata;

namespace Microsoft.Data.Entity.AzureTableStorage.Query
{
    public class TableFilterFactory
    {
        public virtual TableFilter TryCreate([CanBeNull] MemberExpression expression, [CanBeNull] IEntityType entityType)
        {
            try
            {
                return Create(expression, entityType);
            }
            catch
            {
                return null;
            }
        }

        public virtual TableFilter TryCreate([CanBeNull] UnaryExpression expression, [CanBeNull] IEntityType entityType)
        {
            try
            {
                return Create(expression, entityType);
            }
            catch
            {
                return null;
            }
        }

        public virtual TableFilter TryCreate([CanBeNull] BinaryExpression expression, [CanBeNull] IEntityType entityType)
        {
            try
            {
                return Create(expression, entityType);
            }
            catch
            {
                return null;
            }
        }

        public virtual TableFilter Create([NotNull] MemberExpression expression, [NotNull] IEntityType entityType)
        {
            Check.NotNull(expression, "expression");
            Check.NotNull(entityType, "entityType");
            if (expression.Type != typeof(bool))
            {
                throw new ArgumentOutOfRangeException("expression", "Can only create for boolean member expressions");
            }

            return new TableFilter.ConstantTableFilter(GetStorageName(expression, entityType), FilterComparisonOperator.Equal, Expression.Constant(true));
        }

        public virtual TableFilter Create([NotNull] UnaryExpression expression, [NotNull] IEntityType entityType)
        {
            Check.NotNull(expression, "expression");
            Check.NotNull(entityType, "entityType");

            if (!(expression.Operand is MemberExpression)
                || expression.Operand.Type != typeof(bool))
            {
                throw new ArgumentOutOfRangeException("expression", "Can only create for boolean unary expressions");
            }
            return new TableFilter.ConstantTableFilter(
                GetStorageName((MemberExpression)expression.Operand, entityType),
                FilterComparisonOperator.Equal,
                Expression.Constant(expression.NodeType != ExpressionType.Not)
                );
        }

        public virtual TableFilter Create([NotNull] BinaryExpression expression, [NotNull] IEntityType entityType)
        {
            Check.NotNull(expression, "expression");
            Check.NotNull(entityType, "entityType");

            var op = FilterComparison.FromNodeType(expression.NodeType);
            try
            {
                var storageName = GetStorageName((MemberExpression)expression.Left, entityType);
                return CreateFromBinaryExpression(storageName, op, expression.Right);
            }
            catch (Exception)
            {
                // swallow and try the reversed order
                FilterComparison.FlipInequalities(ref op);
                var storageName = GetStorageName((MemberExpression)expression.Right, entityType);
                return CreateFromBinaryExpression(storageName, op, expression.Left);
            }
        }

        private static TableFilter CreateFromBinaryExpression(string storageName, FilterComparisonOperator op, Expression right)
        {
            if (right is ConstantExpression)
            {
                return new TableFilter.ConstantTableFilter(storageName, op, (ConstantExpression)right);
            }
            else if (right is MemberExpression)
            {
                return new TableFilter.MemberTableFilter(storageName, op, (MemberExpression)right);
            }
            else if (right is NewExpression)
            {
                return new TableFilter.NewObjTableFilter(storageName, op, (NewExpression)right);
            }
            return null;
        }

        private static string GetStorageName(MemberExpression memberExpression, IEntityType entityType)
        {
            if (memberExpression.Expression.NodeType == ExpressionType.MemberAccess)
            {
                throw new ArgumentException("Nested member access not supported", "memberExpression");
            }
            var typeName = memberExpression.Member.Name;
            return entityType.GetProperty(typeName).ColumnName();
        }
    }
}
