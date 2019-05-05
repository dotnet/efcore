// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public class PropertyParameterBinding : ParameterBinding
    {
        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public PropertyParameterBinding([NotNull] IProperty consumedProperty)
            : base(consumedProperty.ClrType, consumedProperty)
        {
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
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
