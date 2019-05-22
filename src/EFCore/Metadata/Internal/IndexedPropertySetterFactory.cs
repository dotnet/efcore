// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public class IndexedPropertySetterFactory : ClrAccessorFactory<IClrPropertySetter>
    {
        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override IClrPropertySetter CreateGeneric<TEntity, TValue, TNonNullableEnumValue>(
            PropertyInfo propertyInfo, IPropertyBase propertyBase)
        {
            Debug.Assert(propertyInfo != null);
            Debug.Assert(propertyBase != null);

            if (!propertyInfo.IsEFIndexerProperty())
            {
                throw new InvalidOperationException(
                    CoreStrings.NoIndexer(propertyBase.DeclaringType.DisplayName()));
            }

            var entityParameter = Expression.Parameter(typeof(TEntity), "entity");
            var valueParameter = Expression.Parameter(typeof(TValue), "value");
            var indexerParameterList = new List<Expression>
            {
                Expression.Constant(propertyBase.Name)
            };

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
