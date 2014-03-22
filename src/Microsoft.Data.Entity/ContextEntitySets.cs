// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity
{
    public class ContextEntitySets
    {
        private readonly EntitySetSource _source;
        private readonly EntitySetInitializer _setInitializer;
        private readonly Dictionary<Type, EntitySet> _sets = new Dictionary<Type, EntitySet>();

        /// <summary>
        ///     This constructor is intended only for use when creating test doubles that will override members
        ///     with mocked or faked behavior. Use of this constructor for other purposes may result in unexpected
        ///     behavior including but not limited to throwing <see cref="NullReferenceException" />.
        /// </summary>
        protected ContextEntitySets()
        {
        }

        public ContextEntitySets([NotNull] EntitySetSource source, [NotNull] EntitySetInitializer setInitializer)
        {
            Check.NotNull(source, "source");
            Check.NotNull(setInitializer, "setInitializer");

            _source = source;
            _setInitializer = setInitializer;
        }

        public virtual void InitializeSets([NotNull] EntityContext context)
        {
            Check.NotNull(context, "context");

            _setInitializer.InitializeSets(context);
        }

        public virtual EntitySet GetEntitySet([NotNull] EntityContext context, [NotNull] Type entityType)
        {
            Check.NotNull(context, "context");
            Check.NotNull(entityType, "entityType");

            EntitySet entitySet;
            if (!_sets.TryGetValue(entityType, out entitySet))
            {
                entitySet = _source.Create(context, entityType);
                _sets.Add(entityType, entitySet);
            }
            return entitySet;
        }

        public virtual EntitySet<TEntity> GetEntitySet<TEntity>([NotNull] EntityContext context) where TEntity : class
        {
            Check.NotNull(context, "context");

            EntitySet entitySet;
            if (!_sets.TryGetValue(typeof(TEntity), out entitySet))
            {
                entitySet = _source.Create(context, typeof(TEntity));
                _sets.Add(typeof(TEntity), entitySet);
            }
            return (EntitySet<TEntity>)entitySet;
        }
    }
}
