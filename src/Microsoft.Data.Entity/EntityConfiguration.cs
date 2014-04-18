// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.AspNet.DependencyInjection;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity
{
    public class EntityConfiguration
    {
        private readonly IServiceProvider _services;
        private readonly ServiceCollection _serviceCollection;
        private readonly ConfigurationAnnotations _annotations;
        private readonly IModel _model;

        public EntityConfiguration(
            [CanBeNull] IServiceProvider serviceProvider,
            [CanBeNull] ServiceCollection serviceCollection,
            [NotNull] ConfigurationAnnotations annotations,
            [CanBeNull] IModel model)
        {
            Check.NotNull(annotations, "annotations");

            _services = serviceProvider;
            _serviceCollection = serviceCollection;
            _annotations = annotations;
            _model = model;
        }

        [CanBeNull]
        public virtual IModel Model
        {
            get { return _model; }
        }

        [CanBeNull]
        public virtual IServiceProvider Services
        {
            get { return _services; }
        }

        [CanBeNull]
        public virtual ServiceCollection ServiceCollection
        {
            get { return _serviceCollection; }
        }

        public virtual ConfigurationAnnotations Annotations
        {
            get { return _annotations; }
        }
    }
}
