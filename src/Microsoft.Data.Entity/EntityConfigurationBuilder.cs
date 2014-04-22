// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity
{
    public class EntityConfigurationBuilder
    {
        private IModel _model;

        private readonly IList<Action<IEntityConfigurationConstruction>> _buildActions
            = new List<Action<IEntityConfigurationConstruction>>();

        public virtual EntityConfiguration BuildConfiguration()
        {
            return BuildConfiguration(() => new EntityConfiguration());
        }

        public virtual TConfiguration BuildConfiguration<TConfiguration>([NotNull] Func<TConfiguration> factory)
            where TConfiguration : EntityConfiguration
        {
            Check.NotNull(factory, "factory");

            var configuration = (IEntityConfigurationConstruction)factory();
            configuration.Model = _model;

            foreach (var buildAction in _buildActions)
            {
                buildAction(configuration);
            }

            configuration.Lock();

            return (TConfiguration)configuration;
        }

        public virtual EntityConfigurationBuilder UseModel([NotNull] IModel model)
        {
            Check.NotNull(model, "model");

            _model = model;

            return this;
        }

        public virtual EntityConfigurationBuilder AddBuildAction([NotNull] Action<IEntityConfigurationConstruction> action)
        {
            Check.NotNull(action, "action");

            _buildActions.Add(action);

            return this;
        }
    }
}
