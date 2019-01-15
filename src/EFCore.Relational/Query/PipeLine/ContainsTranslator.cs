// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Relational.Query.Pipeline.SqlExpressions;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.Relational.Query.Pipeline
{
    public class ContainsTranslator : IMethodCallTranslator
    {
        private static MethodInfo _containsMethod = typeof(Enumerable).GetTypeInfo()
            .GetDeclaredMethods(nameof(Enumerable.Contains))
            .Single(mi => mi.GetParameters().Length == 2)
            .GetGenericMethodDefinition();

        private readonly IRelationalTypeMappingSource _typeMappingSource;
        private readonly ITypeMappingApplyingExpressionVisitor _typeMappingApplyingExpressionVisitor;

        public ContainsTranslator(
            IRelationalTypeMappingSource typeMappingSource,
            ITypeMappingApplyingExpressionVisitor typeMappingApplyingExpressionVisitor)
        {
            _typeMappingSource = typeMappingSource;
            _typeMappingApplyingExpressionVisitor = typeMappingApplyingExpressionVisitor;
        }

        public SqlExpression Translate(SqlExpression instance, MethodInfo method, IList<SqlExpression> arguments)
        {
            if (method.IsGenericMethod
                && method.GetGenericMethodDefinition().Equals(_containsMethod))
            {
                var source = arguments[0];
                var item = arguments[1];

                var typeMapping = ExpressionExtensions.InferTypeMapping(source, item);

                source = _typeMappingApplyingExpressionVisitor.ApplyTypeMapping(source, typeMapping);
                item = _typeMappingApplyingExpressionVisitor.ApplyTypeMapping(item, typeMapping);

                return new InExpression(
                    item,
                    false,
                    source,
                    _typeMappingSource.FindMapping(typeof(bool)));
            }
            else if (method.DeclaringType.GetInterfaces().Contains(typeof(IList))
                && string.Equals(method.Name, nameof(IList.Contains)))
            {
                var source = instance;
                var item = arguments[0];

                var typeMapping = ExpressionExtensions.InferTypeMapping(source, item);

                source = _typeMappingApplyingExpressionVisitor.ApplyTypeMapping(source, typeMapping);
                item = _typeMappingApplyingExpressionVisitor.ApplyTypeMapping(item, typeMapping);

                return new InExpression(
                    item,
                    false,
                    source,
                    _typeMappingSource.FindMapping(typeof(bool)));
            }

            return null;
        }
    }
}
