// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class IndexedPropertyGetterFactory : ClrAccessorFactory<IClrPropertyGetter>
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override IClrPropertyGetter CreateGeneric<TEntity, TValue, TNonNullableEnumValue>(
            PropertyInfo propertyInfo, IPropertyBase propertyBase)
        {
            Debug.Assert(propertyInfo != null);
            Debug.Assert(propertyBase != null);

            if (!propertyInfo.IsEFIndexerProperty())
            {
                throw new InvalidOperationException(
                    CoreStrings.NoIndexer(propertyBase.Name, propertyBase.DeclaringType.DisplayName()));
            }

            var entityParameter = Expression.Parameter(typeof(TEntity), "entity");
            var indexerParameterList = new List<Expression>() { Expression.Constant(propertyBase.Name) };
            Expression readExpression = Expression.MakeIndex(
                entityParameter, propertyInfo, indexerParameterList);

            if (readExpression.Type != typeof(TValue))
            {
                readExpression = Expression.Convert(readExpression, typeof(TValue));
            }

            var property = propertyBase as IProperty;
            var comparer = typeof(TValue).IsNullableType()
                ? null
                : property?.GetValueComparer()
                  ?? property?.FindMapping()?.Comparer
                  ?? (ValueComparer)Activator.CreateInstance(
                      typeof(ValueComparer<>).MakeGenericType(typeof(TValue)),
                      new object[] { false });

            var hasDefaultValueExpression = comparer == null
                ? Expression.Equal(readExpression, Expression.Default(typeof(TValue)))
                : comparer.ExtractEqualsBody(readExpression, Expression.Default(typeof(TValue)));

            return new ClrPropertyGetter<TEntity, TValue>(
                Expression.Lambda<Func<TEntity, TValue>>(readExpression, entityParameter).Compile(),
                Expression.Lambda<Func<TEntity, bool>>(hasDefaultValueExpression, entityParameter).Compile());
        }
    }
}
