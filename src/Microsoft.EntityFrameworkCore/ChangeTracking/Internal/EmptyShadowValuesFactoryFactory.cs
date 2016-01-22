// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.ChangeTracking.Internal
{
    public class EmptyShadowValuesFactoryFactory : SnapshotFactoryFactory
    {
        protected override int GetPropertyIndex(IPropertyBase propertyBase)
            => (propertyBase as IProperty)?.GetShadowIndex() ?? -1;

        protected override int GetPropertyCount(IEntityType entityType)
            => entityType.ShadowPropertyCount();

        protected override bool UseEntityVariable => false;

        protected override Expression CreateReadShadowValueExpression(ParameterExpression parameter, IProperty property)
            => Expression.Default(property.ClrType);
    }
}
