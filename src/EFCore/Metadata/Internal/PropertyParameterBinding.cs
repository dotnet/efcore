// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class PropertyParameterBinding : ParameterBinding
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public PropertyParameterBinding([NotNull] IProperty consumedProperty)
            : base(consumedProperty.ClrType, consumedProperty)
        {
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override Expression BindToParameter(ParameterBindingInfo bindingInfo)
        {
            var property = ConsumedProperties[0];

            return Expression.Call(
                EntityMaterializerSource.TryReadValueMethod.MakeGenericMethod(property.ClrType),
                Expression.Call(bindingInfo.MaterializationContextExpression, MaterializationContext.GetValueBufferMethod),
                Expression.Constant(bindingInfo.GetValueBufferIndex(property)),
                Expression.Constant(property, typeof(IPropertyBase)));
        }
    }
}
