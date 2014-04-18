// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.AspNet.DependencyInjection;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity
{
    public class EntityConfigurationBuilder
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly EntityServicesBuilder _servicesBuilder;
        private IModel _model;
        private readonly ConfigurationAnnotations _annotations = new ConfigurationAnnotations();

        public EntityConfigurationBuilder()
        {
            _servicesBuilder = new EntityServicesBuilder(new ServiceCollection().AddEntityFramework());
        }

        public EntityConfigurationBuilder([NotNull] IServiceProvider serviceProvider)
        {
            Check.NotNull(serviceProvider, "serviceProvider");

            _serviceProvider = serviceProvider;
        }

        public virtual EntityConfiguration BuildConfiguration()
        {
            return new EntityConfiguration(
                _serviceProvider,
                _servicesBuilder == null ? null : _servicesBuilder.ServiceCollection,
                _annotations,
                _model);
        }

        public virtual EntityConfigurationBuilder WithServices([NotNull] Action<EntityServicesBuilder> servicesBuilder)
        {
            Check.NotNull(servicesBuilder, "servicesBuilder");

            if (_servicesBuilder == null)
            {
                // TODO: Proper messgae
                throw new InvalidOperationException("Services already configured.");
            }

            servicesBuilder(_servicesBuilder);

            return this;
        }

        public virtual ConfigurationAnnotations Annotations
        {
            get { return _annotations; }
        }

        public virtual EntityConfigurationBuilder UseModel([NotNull] IModel model)
        {
            Check.NotNull(model, "model");

            _model = model;

            return this;
        }
    }
}
