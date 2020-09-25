// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.ChangeTracking.Internal
{
    /// <inheritdoc />
    public class ShadowValuesFactoryFactory : SnapshotFactoryFactory<ValueBuffer>
    {
        /// <inheritdoc />
        protected override int GetPropertyIndex(IPropertyBase propertyBase)
            // Navigations are not included in the supplied value buffer
            => (propertyBase as IProperty)?.GetShadowIndex() ?? -1;

        /// <inheritdoc />
        protected override int GetPropertyCount(IEntityType entityType)
            => entityType.ShadowPropertyCount();

        /// <inheritdoc />
        protected override ValueComparer GetValueComparer(IProperty property)
            => null;

        /// <inheritdoc />
        protected override bool UseEntityVariable
            => false;

        /// <inheritdoc />
        protected override Expression CreateReadShadowValueExpression(
            ParameterExpression parameter,
            IPropertyBase property)
            => Expression.Convert(
                Expression.Call(
                    parameter,
                    ValueBuffer.GetValueMethod,
                    Expression.Constant(property.GetShadowIndex())),
                property.ClrType);

        /// <inheritdoc />
        protected override Expression CreateReadValueExpression(
            ParameterExpression parameter,
            IPropertyBase property)
            => CreateReadShadowValueExpression(parameter, property);
    }
}
