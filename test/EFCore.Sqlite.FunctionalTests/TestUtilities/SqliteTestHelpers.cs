// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Sqlite.Diagnostics.Internal;
using Microsoft.EntityFrameworkCore.Sqlite.Internal;
using Microsoft.EntityFrameworkCore.Sqlite.Storage.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.TestUtilities
{
    public class SqliteTestHelpers : TestHelpers
    {
        protected SqliteTestHelpers()
        {
        }

        public static SqliteTestHelpers Instance { get; } = new SqliteTestHelpers();

        public override IServiceCollection AddProviderServices(IServiceCollection services)
            => services.AddEntityFrameworkSqlite();

        protected override void UseProviderOptions(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder.UseSqlite(new SqliteConnection("Data Source=:memory:"));

        public override IModelValidator CreateModelValidator()
        {
            var typeMappingSource = new SqliteTypeMappingSource(
                TestServiceFactory.Instance.Create<TypeMappingSourceDependencies>(),
                TestServiceFactory.Instance.Create<RelationalTypeMappingSourceDependencies>());

            return new SqliteModelValidator(
                new ModelValidatorDependencies(
                    typeMappingSource,
                    new MemberClassifier(
                        typeMappingSource,
                        TestServiceFactory.Instance.Create<IParameterBindingFactories>())),
                new RelationalModelValidatorDependencies(typeMappingSource));
        }

        public override LoggingDefinitions LoggingDefinitions { get; } = new SqliteLoggingDefinitions();
    }
}
