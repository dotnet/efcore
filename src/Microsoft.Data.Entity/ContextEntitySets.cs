// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity
{
    public class ContextEntitySets
    {
        private readonly EntityContext _context;
        private readonly EntitySetSource _source;
        private readonly Dictionary<Type, EntitySet> _sets = new Dictionary<Type, EntitySet>();

        /// <summary>
        ///     This constructor is intended only for use when creating test doubles that will override members
        ///     with mocked or faked behavior. Use of this constructor for other purposes may result in unexpected
        ///     behavior including but not limited to throwing <see cref="NullReferenceException" />.
        /// </summary>
        protected ContextEntitySets()
        {
        }

        public ContextEntitySets([NotNull] EntityContext context, [NotNull] EntitySetSource source)
        {
            Check.NotNull(context, "context");
            Check.NotNull(source, "source");

            _context = context;
            _source = source;
        }

        public virtual EntitySet GetEntitySet([NotNull] Type entityType)
        {
            Check.NotNull(entityType, "entityType");

            EntitySet entitySet;
            if (!_sets.TryGetValue(entityType, out entitySet))
            {
                entitySet = _source.Create(_context, entityType);
                _sets.Add(entityType, entitySet);
            }
            return entitySet;
        }

        public virtual EntitySet<TEntity> GetEntitySet<TEntity>() where TEntity : class
        {
            EntitySet entitySet;
            if (!_sets.TryGetValue(typeof(TEntity), out entitySet))
            {
                entitySet = _source.Create(_context, typeof(TEntity));
                _sets.Add(typeof(TEntity), entitySet);
            }
            return (EntitySet<TEntity>)entitySet;
        }
    }
}
