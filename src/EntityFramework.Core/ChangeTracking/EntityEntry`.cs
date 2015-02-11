// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.Data.Entity.ChangeTracking.Internal;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.ChangeTracking
{
    public class EntityEntry<TEntity> : EntityEntry
        where TEntity : class
    {
        public EntityEntry([NotNull] DbContext context, [NotNull] InternalEntityEntry internalEntry)
            : base(context, internalEntry)
        {
        }

        public new virtual TEntity Entity => (TEntity)base.Entity;

        public virtual PropertyEntry<TEntity, TProperty> Property<TProperty>(
            [NotNull] Expression<Func<TEntity, TProperty>> propertyExpression)
        {
            Check.NotNull(propertyExpression, nameof(propertyExpression));

            var propertyInfo = propertyExpression.GetPropertyAccess();

            return new PropertyEntry<TEntity, TProperty>(InternalEntry, propertyInfo.Name);
        }
    }
}
