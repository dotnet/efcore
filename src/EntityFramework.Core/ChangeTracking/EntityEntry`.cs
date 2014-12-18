// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.ChangeTracking
{
    public class EntityEntry<TEntity> : EntityEntry where TEntity : class
    {
        public EntityEntry([NotNull] DbContext context, [NotNull] StateEntry stateEntry)
            : base(context, stateEntry)
        {
        }

        public new virtual TEntity Entity => (TEntity)base.Entity;

        public virtual PropertyEntry<TEntity, TProperty> Property<TProperty>(
            [NotNull] Expression<Func<TEntity, TProperty>> propertyExpression)
        {
            Check.NotNull(propertyExpression, "propertyExpression");

            var propertyInfo = propertyExpression.GetPropertyAccess();

            return new PropertyEntry<TEntity, TProperty>(StateEntry, propertyInfo.Name);
        }
    }
}
