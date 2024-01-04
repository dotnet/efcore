// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.TestUtilities.FakeProvider;

public class FakeRelationalOptionsExtension : RelationalOptionsExtension
{
    private DbContextOptionsExtensionInfo _info;

    public FakeRelationalOptionsExtension()
    {
    }

    protected FakeRelationalOptionsExtension(FakeRelationalOptionsExtension copyFrom)
        : base(copyFrom)
    {
    }

    public override DbContextOptionsExtensionInfo Info
        => _info ??= new ExtensionInfo(this);

    protected override RelationalOptionsExtension Clone()
        => new FakeRelationalOptionsExtension(this);

    public override void ApplyServices(IServiceCollection services)
        => AddEntityFrameworkRelationalDatabase(services);

    public static IServiceCollection AddEntityFrameworkRelationalDatabase(IServiceCollection serviceCollection)
    {
        var builder = new EntityFrameworkRelationalServicesBuilder(serviceCollection)
            .TryAdd<LoggingDefinitions, TestRelationalLoggingDefinitions>()
            .TryAdd<IDatabaseProvider, DatabaseProvider<FakeRelationalOptionsExtension>>()
            .TryAdd<ISqlGenerationHelper, RelationalSqlGenerationHelper>()
            .TryAdd<IRelationalTypeMappingSource, TestRelationalTypeMappingSource>()
            .TryAdd<IMigrationsSqlGenerator, TestRelationalMigrationSqlGenerator>()
            .TryAdd<IProviderConventionSetBuilder, TestRelationalConventionSetBuilder>()
            .TryAdd<IRelationalConnection, FakeRelationalConnection>()
            .TryAdd<IHistoryRepository>(_ => null)
            .TryAdd<IUpdateSqlGenerator, FakeSqlGenerator>()
            .TryAdd<IModificationCommandBatchFactory, TestModificationCommandBatchFactory>()
            .TryAdd<IRelationalDatabaseCreator, FakeRelationalDatabaseCreator>();

        builder.TryAddCoreServices();

        return serviceCollection;
    }

    private sealed class ExtensionInfo(IDbContextOptionsExtension extension) : RelationalExtensionInfo(extension)
    {
        public override void PopulateDebugInfo(IDictionary<string, string> debugInfo)
        {
        }
    }
}
