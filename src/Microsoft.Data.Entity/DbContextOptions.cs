// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity
{
    public class DbContextOptions
    {
        private IModel _model;

        private readonly IList<Action<IDbContextOptionsConstruction>> _buildActions
            = new List<Action<IDbContextOptionsConstruction>>();

        public virtual ImmutableDbContextOptions BuildConfiguration()
        {
            return BuildConfiguration(() => new ImmutableDbContextOptions());
        }

        public virtual TConfiguration BuildConfiguration<TConfiguration>([NotNull] Func<TConfiguration> factory)
            where TConfiguration : ImmutableDbContextOptions
        {
            Check.NotNull(factory, "factory");

            var configuration = (IDbContextOptionsConstruction)factory();
            configuration.Model = _model;

            foreach (var buildAction in _buildActions)
            {
                buildAction(configuration);
            }

            configuration.Lock();

            return (TConfiguration)configuration;
        }

        public virtual DbContextOptions UseModel([NotNull] IModel model)
        {
            Check.NotNull(model, "model");

            _model = model;

            return this;
        }

        public virtual DbContextOptions AddBuildAction([NotNull] Action<IDbContextOptionsConstruction> action)
        {
            Check.NotNull(action, "action");

            _buildActions.Add(action);

            return this;
        }
    }
}
