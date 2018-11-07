// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class IndexedPropertySetterFactory : ClrAccessorFactory<IClrPropertySetter>
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override IClrPropertySetter CreateGeneric<TEntity, TValue, TNonNullableEnumValue>(
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
            var valueParameter = Expression.Parameter(typeof(TValue), "value");
            var indexerParameterList = new List<Expression>() { Expression.Constant(propertyBase.Name) };

            // the indexer expects the value to be an object, but the indexed property
            // can have been declared as a different type so cast it to that if necessary
            var propertyType = propertyBase.ClrType;
            var convertedParameter = propertyType == typeof(object)
                ? (Expression)valueParameter
                : Expression.TypeAs(valueParameter, typeof(object));

            Expression writeExpression = Expression.Assign(
                Expression.MakeIndex(entityParameter, propertyInfo, indexerParameterList),
                convertedParameter);

            var setter = Expression.Lambda<Action<TEntity, TValue>>(
                writeExpression,
                entityParameter,
                valueParameter).Compile();

            return propertyType.IsNullableType()
                   && propertyType.UnwrapNullableType().GetTypeInfo().IsEnum
                ? new NullableEnumClrPropertySetter<TEntity, TValue, TNonNullableEnumValue>(setter)
                : (IClrPropertySetter)new ClrPropertySetter<TEntity, TValue>(setter);
        }
    }
}
