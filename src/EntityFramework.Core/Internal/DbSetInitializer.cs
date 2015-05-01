// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata.Internal;

namespace Microsoft.Data.Entity.Internal
{
    public class DbSetInitializer : IDbSetInitializer
    {
        private readonly IDbSetFinder _setFinder;
        private readonly IClrAccessorSource<IClrPropertySetter> _setSetters;
        private readonly IDbSetSource _setSource;

        public DbSetInitializer(
            [NotNull] IDbSetFinder setFinder,
            [NotNull] IClrAccessorSource<IClrPropertySetter> setSetters,
            [NotNull] IDbSetSource setSource)
        {
            _setFinder = setFinder;
            _setSetters = setSetters;
            _setSource = setSource;
        }

        public virtual void InitializeSets(DbContext context)
        {
            foreach (var setInfo in _setFinder.FindSets(context).Where(p => p.HasSetter))
            {
                _setSetters
                    .GetAccessor(setInfo.ContextType, setInfo.Name)
                    .SetClrValue(context, _setSource.Create(context, setInfo.EntityType));
            }
        }

        public virtual DbSet<TEntity> CreateSet<TEntity>(DbContext context) where TEntity : class
            => (DbSet<TEntity>)_setSource.Create(context, typeof(TEntity));
    }
}
