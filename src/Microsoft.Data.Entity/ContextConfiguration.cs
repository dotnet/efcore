// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.AspNet.DependencyInjection;
using Microsoft.Data.Entity.ChangeTracking;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity
{
    public class ContextConfiguration : EntityConfiguration
    {
        private EntityContext _context;
        private LazyRef<IModel> _model;

        public virtual ContextConfiguration Initialize(
            [NotNull] IServiceProvider scopedProvider, [NotNull] EntityContext context)
        {
            Check.NotNull(scopedProvider, "scopedProvider");
            Check.NotNull(context, "context");

            Initialize(scopedProvider);

            _context = context;
            _model = new LazyRef<IModel>(() => ServiceProvider.GetRequiredService<IModelSource>().GetModel(_context));

            return this;
        }

        public virtual EntityContext Context
        {
            get { return _context; }
        }

        public virtual IModel Model
        {
            get { return _model.Value; }
        }

        public virtual StateManager StateManager
        {
            get { return ServiceProvider.GetRequiredService<StateManager>(); }
        }

        public virtual ContextEntitySets ContextEntitySets
        {
            get { return ServiceProvider.GetRequiredService<ContextEntitySets>(); }
        }

        public virtual StateEntryNotifier StateEntryNotifier
        {
            get { return ServiceProvider.GetRequiredService<StateEntryNotifier>(); }
        }

        public virtual StateEntryFactory StateEntryFactory
        {
            get { return ServiceProvider.GetRequiredService<StateEntryFactory>(); }
        }

        public virtual IEnumerable<IEntityStateListener> EntityStateListeners
        {
            get
            {
                return ServiceProvider.GetService<IEnumerable<IEntityStateListener>>()
                       ?? Enumerable.Empty<IEntityStateListener>();
            }
        }
    }
}
