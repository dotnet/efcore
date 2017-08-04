// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore
{
    public class SharedCrossStoreFixture : CrossStoreFixture
    {
        protected override string StoreName { get; } = "SharedCrossStoreTest";

        private IModel _model;

        private IModel Model
        {
            get
            {
                if (_model == null)
                {
                    var conventionSet = new CoreConventionSetBuilder(new CoreConventionSetBuilderDependencies(
                        new CoreTypeMapper(new CoreTypeMapperDependencies())))
                        .CreateConventionSet();
                    var modelBuilder = new ModelBuilder(conventionSet);

                    OnModelCreating(modelBuilder, null);
                    _model = modelBuilder.Model;
                }
                return _model;
            }
        }

        public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
            => base.AddOptions(builder).UseModel(Model);
    }
}
