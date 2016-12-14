// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Relational.Specification.Tests.TestUtilities;
using Microsoft.EntityFrameworkCore.Specification.Tests.TestUtilities;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.EntityFrameworkCore.Relational.Tests.TestUtilities.FakeProvider
{
    public class FakeRelationalOptionsExtension : RelationalOptionsExtension
    {
        public FakeRelationalOptionsExtension()
        {
        }

        public FakeRelationalOptionsExtension(FakeRelationalOptionsExtension copyFrom)
            : base(copyFrom)
        {
        }

        public override void ApplyServices(IServiceCollection services)
            => AddEntityFrameworkRelationalDatabase(services);

        public static IServiceCollection AddEntityFrameworkRelationalDatabase(IServiceCollection services)
        {
            services.AddRelational();

            services.TryAddEnumerable(ServiceDescriptor
                .Singleton<IDatabaseProvider, DatabaseProvider<TestRelationalDatabaseProviderServices, FakeRelationalOptionsExtension>>());

            services.TryAdd(new ServiceCollection()
                .AddSingleton<TestRelationalModelSource>()
                .AddSingleton<TestRelationalValueGeneratorCache>()
                .AddSingleton<RelationalSqlGenerationHelper>()
                .AddSingleton<TestRelationalTypeMapper>()
                .AddSingleton<TestAnnotationProvider>()
                .AddScoped<TestRelationalDatabaseProviderServices>()
                .AddScoped<TestRelationalConventionSetBuilder>()
                .AddScoped<TestRelationalCompositeMemberTranslator>()
                .AddScoped<TestRelationalCompositeMethodCallTranslator>()
                .AddScoped<TestQuerySqlGeneratorFactory>()
                .AddScoped<FakeRelationalConnection>());

            return services;
        }
    }
}
