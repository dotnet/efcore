// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Metadata.ModelConventions;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Relational.Migrations.Infrastructure
{
    public class MigrationModelFactory : IMigrationModelFactory
    {
        public virtual IModel CreateModel(Action<ModelBuilder> onModelCreating)
        {
            Check.NotNull(onModelCreating, nameof(onModelCreating));

            var model = new Model();
            var conventions = new ConventionSet();
            var modelBuilder = new ModelBuilder(conventions, model);

            onModelCreating(modelBuilder);

            return model;
        }
    }
}
