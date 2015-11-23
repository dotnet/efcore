// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Metadata.Internal;
using Microsoft.Data.Entity.Storage;

namespace Microsoft.Data.Entity.ChangeTracking.Internal
{
    public class ShadowValuesFactoryFactory : SnapshotFactoryFactory<ValueBuffer>
    {
        protected override int GetPropertyIndex(IPropertyBase propertyBase)
            => (propertyBase as IProperty)?.GetShadowIndex() ?? -1;

        protected override int GetPropertyCount(IEntityType entityType)
            => entityType.ShadowPropertyCount();

        protected override bool UseEntityVariable => false;

        protected override Expression CreateReadShadowValueExpression(ParameterExpression parameter, IProperty property)
            => Expression.Convert(
                Expression.Call(
                    parameter,
                    ValueBuffer.GetValueMethod,
                    Expression.Constant(property.GetIndex())),
                property.ClrType);
    }
}
