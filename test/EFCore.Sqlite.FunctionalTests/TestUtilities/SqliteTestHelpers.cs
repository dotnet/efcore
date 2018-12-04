// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Sqlite.Storage.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.Internal;
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

        public override IModelValidator CreateModelValidator(
            DiagnosticsLogger<DbLoggerCategory.Model> modelLogger,
            DiagnosticsLogger<DbLoggerCategory.Model.Validation> validationLogger)
            => new SqliteModelValidator(
                new ModelValidatorDependencies(validationLogger, modelLogger),
                new RelationalModelValidatorDependencies(
#pragma warning disable 618
                    TestServiceFactory.Instance.Create<ObsoleteRelationalTypeMapper>(),
#pragma warning restore 618
                new SqliteTypeMappingSource(
                                TestServiceFactory.Instance.Create<TypeMappingSourceDependencies>(),
                                TestServiceFactory.Instance.Create<RelationalTypeMappingSourceDependencies>())));
    }
}
