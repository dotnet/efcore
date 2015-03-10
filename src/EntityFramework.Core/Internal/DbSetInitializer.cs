// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;

namespace Microsoft.Data.Entity.Internal
{
    public class DbSetInitializer
    {
        private readonly DbSetFinder _setFinder;
        private readonly ClrPropertySetterSource _setSetters;
        private readonly DbSetSource _setSource;

        public DbSetInitializer(
            [NotNull] DbSetFinder setFinder,
            [NotNull] ClrPropertySetterSource setSetters,
            [NotNull] DbSetSource setSource)
        {
            _setFinder = setFinder;
            _setSetters = setSetters;
            _setSource = setSource;
        }

        public virtual void InitializeSets([NotNull] DbContext context)
        {
            foreach (var setInfo in _setFinder.FindSets(context).Where(p => p.HasSetter))
            {
                _setSetters
                    .GetAccessor(setInfo.ContextType, setInfo.Name)
                    .SetClrValue(context, _setSource.Create(context, setInfo.EntityType));
            }
        }

        public virtual DbSet<TEntity> CreateSet<TEntity>([NotNull] DbContext context) where TEntity : class
            => (DbSet<TEntity>)_setSource.Create(context, typeof(TEntity));
    }
}
