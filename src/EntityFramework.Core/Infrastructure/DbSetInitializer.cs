// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Infrastructure
{
    public class DbSetInitializer
    {
        private readonly DbSetFinder _setFinder;
        private readonly ClrPropertySetterSource _setSetters;
        private readonly DbSetSource _setSource;

        /// <summary>
        ///     This constructor is intended only for use when creating test doubles that will override members
        ///     with mocked or faked behavior. Use of this constructor for other purposes may result in unexpected
        ///     behavior including but not limited to throwing <see cref="NullReferenceException" />.
        /// </summary>
        protected DbSetInitializer()
        {
        }

        public DbSetInitializer(
            [NotNull] DbSetFinder setFinder,
            [NotNull] ClrPropertySetterSource setSetters,
            [NotNull] DbSetSource setSource)
        {
            Check.NotNull(setFinder, "setFinder");
            Check.NotNull(setSetters, "setSetters");
            Check.NotNull(setSource, "setSource");

            _setFinder = setFinder;
            _setSetters = setSetters;
            _setSource = setSource;
        }

        public virtual void InitializeSets([NotNull] DbContext context)
        {
            Check.NotNull(context, "context");

            foreach (var setInfo in _setFinder.FindSets(context).Where(p => p.HasSetter))
            {
                _setSetters
                    .GetAccessor(setInfo.ContextType, setInfo.Name)
                    .SetClrValue(context, _setSource.Create(context, setInfo.EntityType));
            }
        }

        public virtual DbSet<TEntity> CreateSet<TEntity>([NotNull] DbContext context)
            where TEntity : class
        {
            Check.NotNull(context, "context");

            return (DbSet<TEntity>)_setSource.Create(context, typeof(TEntity));
        }
    }
}
