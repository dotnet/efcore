// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Relational
{
    public class TypedValueReaderFactoryFactory : IRelationalValueReaderFactoryFactory
    {
        private readonly MethodInfo _getFieldValueMethod
            = typeof(DbDataReader).GetTypeInfo().GetDeclaredMethod("GetFieldValue");

        private readonly MethodInfo _isDbNullMethod
            = typeof(DbDataReader).GetTypeInfo().GetDeclaredMethod("IsDBNull");

        private readonly ThreadSafeDictionaryCache<IEnumerable<Type>, Func<DbDataReader, int, object[]>> _cache
            = new ThreadSafeDictionaryCache<IEnumerable<Type>, Func<DbDataReader, int, object[]>>(
                new ReferenceEnumerableEqualityComparer<IEnumerable<Type>, Type>());

        public virtual IRelationalValueReaderFactory CreateValueReaderFactory(IEnumerable<Type> valueTypes, int offset)
            => new TypedValueReaderFactory(
                _cache.GetOrAdd(
                    Check.NotNull(valueTypes, nameof(valueTypes)),
                    CreateArrayInitializer),
                offset);

        private Func<DbDataReader, int, object[]> CreateArrayInitializer(IEnumerable<Type> valueTypes)
        {
            var dataReaderParam = Expression.Parameter(typeof(DbDataReader), "dataReader");
            var offsetParam = Expression.Parameter(typeof(int), "offset");

            return Expression.Lambda<Func<DbDataReader, int, object[]>>(
                Expression.NewArrayInit(
                    typeof(object),
                    valueTypes.Select((t, i) => CreateGetValueExpression(
                        dataReaderParam,
                        Expression.Add(offsetParam, Expression.Constant(i)),
                        t))),
                dataReaderParam,
                offsetParam).Compile();
        }

        private Expression CreateGetValueExpression(Expression dataReaderExpression, Expression indexExpression, Type type)
        {
            var underlyingTargetMemberType = type.UnwrapNullableType().UnwrapEnumType();

            Expression expression = Expression.Call(
                dataReaderExpression,
                _getFieldValueMethod.MakeGenericMethod(underlyingTargetMemberType),
                indexExpression);

            if (underlyingTargetMemberType != type)
            {
                expression = Expression.Convert(expression, type);
            }

            return Expression.Condition(
                Expression.Call(dataReaderExpression, _isDbNullMethod, indexExpression),
                Expression.Constant(null, typeof(object)),
                Expression.Convert(expression, typeof(object)));
        }
    }
}
