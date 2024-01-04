// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.SqlServer.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.SqlServer.Storage.Internal;

namespace Microsoft.EntityFrameworkCore;

public class SqlServerOptionsExtensionTest
{
    [ConditionalFact]
    public void Compiled_model_is_thread_safe()
    {
        var tasks = new Task[Environment.ProcessorCount];
        for (var i = 0; i < tasks.Length; i++)
        {
            tasks[i] = Task.Run(
                () =>
                {
                    using var ctx = new EmptyContext();
                    Assert.NotNull(ctx.Model.GetRelationalDependencies());
                });
        }

        Task.WaitAll(tasks);
    }

    private class EmptyContext : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer().UseModel(EmptyContextModel.Instance);
            }
        }
    }

    [DbContext(typeof(EmptyContext))]
    private class EmptyContextModel(bool skipDetectChanges, Guid modelId, int entityTypeCount, int typeConfigurationCount) : RuntimeModel(skipDetectChanges, modelId, entityTypeCount, typeConfigurationCount)
    {
        static EmptyContextModel()
        {
            var model = new EmptyContextModel(false, Guid.NewGuid(), 0, 0);
            _instance = model;
        }

        private static readonly EmptyContextModel _instance;

        public static IModel Instance
            => _instance;
    }

    [ConditionalFact]
    public void ApplyServices_adds_SQL_server_services()
    {
        var services = new ServiceCollection();

        new SqlServerOptionsExtension().ApplyServices(services);

        Assert.Contains(services, sd => sd.ServiceType == typeof(ISqlServerConnection));
    }

    private class ChangedRowNumberContext(bool setInternalServiceProvider) : DbContext
    {
        private static readonly IServiceProvider _serviceProvider
            = new ServiceCollection()
                .AddEntityFrameworkSqlServer()
                .BuildServiceProvider(validateScopes: true);

        private readonly bool _setInternalServiceProvider = setInternalServiceProvider;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (_setInternalServiceProvider)
            {
                optionsBuilder.UseInternalServiceProvider(_serviceProvider);
            }

            optionsBuilder.UseSqlServer("Database=Maltesers");
        }
    }
}
