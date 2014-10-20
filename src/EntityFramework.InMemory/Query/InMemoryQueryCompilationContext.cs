// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.ChangeTracking;
using Microsoft.Data.Entity.InMemory.Utilities;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Query;
using Microsoft.Framework.Logging;

namespace Microsoft.Data.Entity.InMemory.Query
{
    public class InMemoryQueryCompilationContext : QueryCompilationContext
    {
        private readonly InMemoryDatabase _database;
        private readonly EntityKeyFactorySource _entityKeyFactorySource;

        public InMemoryQueryCompilationContext(
            [NotNull] IModel model,
            [NotNull] ILogger logger,
            [NotNull] EntityKeyFactorySource entityKeyFactorySource,
            [NotNull] InMemoryDatabase database)
            : base(
                Check.NotNull(model, "model"),
                Check.NotNull(logger, "logger"),
                new LinqOperatorProvider(),
                new ResultOperatorHandler())
        {
            Check.NotNull(entityKeyFactorySource, "entityKeyFactorySource");
            Check.NotNull(database, "database");

            _entityKeyFactorySource = entityKeyFactorySource;
            _database = database;
        }

        public override EntityQueryModelVisitor CreateQueryModelVisitor(
            EntityQueryModelVisitor parentEntityQueryModelVisitor)
        {
            return new InMemoryQueryModelVisitor(this);
        }

        public virtual EntityKeyFactorySource EntityKeyFactorySource
        {
            get { return _entityKeyFactorySource; }
        }

        public virtual InMemoryDatabase Database
        {
            get { return _database; }
        }
    }
}
