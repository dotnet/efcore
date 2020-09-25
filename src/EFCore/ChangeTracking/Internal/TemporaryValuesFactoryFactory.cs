// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Microsoft.EntityFrameworkCore.ChangeTracking.Internal
{
    /// <inheritdoc />
    public class TemporaryValuesFactoryFactory : SidecarValuesFactoryFactory
    {
        /// <inheritdoc />
        protected override Expression CreateSnapshotExpression(
            Type entityType,
            ParameterExpression parameter,
            Type[] types,
            IList<IPropertyBase> propertyBases)
        {
            var constructorExpression = Expression.Convert(
                Expression.New(
                    Snapshot.CreateSnapshotType(types).GetDeclaredConstructor(new Type[0])),
                typeof(ISnapshot));

            return constructorExpression;
        }
    }
}
