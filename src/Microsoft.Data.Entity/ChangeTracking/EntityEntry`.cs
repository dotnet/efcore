// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.ChangeTracking
{
    public class EntityEntry<TEntity> : EntityEntry
    {
        internal EntityEntry(EntityEntryImpl impl)
            : base(impl)
        {
        }

        public EntityEntry([NotNull] ChangeTracker changeTracker, [NotNull] TEntity entity)
            : base(changeTracker, entity)
        {
        }

        public new virtual TEntity Entity
        {
            get { return (TEntity)base.Entity; }
        }

        public virtual PropertyEntry<TEntity, TProperty> Property<TProperty>(
            [NotNull] Expression<Func<TEntity, TProperty>> propertyExpression)
        {
            Check.NotNull(propertyExpression, "propertyExpression");

            var propertyInfo = propertyExpression.GetPropertyAccess();

            return new PropertyEntry<TEntity, TProperty>(this, propertyInfo.Name);
        }
    }
}
