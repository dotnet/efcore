// Copyright (c) Microsoft Open Technologies, Inc.
// All Rights Reserved
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.0
// 
// THIS CODE IS PROVIDED *AS IS* BASIS, WITHOUT WARRANTIES OR
// CONDITIONS OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING
// WITHOUT LIMITATION ANY IMPLIED WARRANTIES OR CONDITIONS OF
// TITLE, FITNESS FOR A PARTICULAR PURPOSE, MERCHANTABLITY OR
// NON-INFRINGEMENT.
// See the Apache 2 License for the specific language governing
// permissions and limitations under the License.

using System;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.ChangeTracking
{
    public class EntityEntry<TEntity> : EntityEntry
    {
        public EntityEntry([NotNull] StateEntry stateEntry)
            : base(stateEntry)
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

            return new PropertyEntry<TEntity, TProperty>(StateEntry, propertyInfo.Name);
        }
    }
}
