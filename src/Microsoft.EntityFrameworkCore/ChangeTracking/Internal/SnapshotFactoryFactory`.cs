// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Microsoft.EntityFrameworkCore.ChangeTracking.Internal
{
    public abstract class SnapshotFactoryFactory<TInput> : SnapshotFactoryFactory
    {
        public virtual Func<TInput, ISnapshot> Create([NotNull] IEntityType entityType)
        {
            if (GetPropertyCount(entityType) == 0)
            {
                return e => Snapshot.Empty;
            }

            var parameter = Expression.Parameter(typeof(TInput), "source");

            return Expression.Lambda<Func<TInput, ISnapshot>>(
                CreateConstructorExpression(entityType, parameter),
                parameter)
                .Compile();
        }
    }
}
