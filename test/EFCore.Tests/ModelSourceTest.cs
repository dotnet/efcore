// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

// ReSharper disable UnusedMember.Local
// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore
{
    public class ModelSourceTest
    {
        [ConditionalFact]
        public void OnModelCreating_is_only_called_once()
        {
            const int threadCount = 5;

            var models = new IModel[threadCount];

            Parallel.For(
                0, threadCount,
                i =>
                {
                    using var context = new SlowContext();
                    models[i] = context.Model;
                });

            Assert.NotNull(models[0]);

            foreach (var model in models)
            {
                Assert.Same(models[0], model);
            }

            Assert.Equal(1, SlowContext.CallCount);
        }

        private class SlowContext : DbContext
        {
            public static int CallCount { get; private set; }

            protected internal override void OnModelCreating(ModelBuilder modelBuilder)
            {
                CallCount++;
                Thread.Sleep(200);
            }

            protected internal override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
                => optionsBuilder
                    .UseInternalServiceProvider(InMemoryFixture.DefaultServiceProvider)
                    .UseInMemoryDatabase(nameof(SlowContext));
        }

        [ConditionalFact]
        public void Adds_all_entities_based_on_all_distinct_entity_types_found()
        {
            var serviceProvider = InMemoryTestHelpers.Instance.CreateContextServices();

            var model = serviceProvider.GetRequiredService<IModelSource>()
                .GetModel(
                    new Context1(),
                    serviceProvider.GetRequiredService<ModelCreationDependencies>()
                    with { ConventionSetBuilder = CreateRuntimeConventionSetBuilder(new FakeSetFinder(), serviceProvider) },
                    designTime: false);

            Assert.Equal(
                new[] { typeof(SetA).DisplayName(), typeof(SetB).DisplayName() },
                model.GetEntityTypes().Select(e => e.Name).ToArray());
        }

        private class FakeModelValidator : IModelValidator
        {
            public void Validate(IModel model, IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
            {
            }
        }

        private class FakeSetFinder : IDbSetFinder
        {
            public IReadOnlyList<DbSetProperty> FindSets(Type contextType)
                => new[]
                {
                    new DbSetProperty("One", typeof(SetA), setter: null),
                    new DbSetProperty("Two", typeof(SetB), setter: null),
                    new DbSetProperty("Three", typeof(SetA), setter: null)
                };
        }

        private class JustAClass
        {
            public DbSet<Random> One { get; set; }
            protected DbSet<object> Two { get; set; }
            private DbSet<string> Three { get; set; }
            private DbSet<string> Four { get; set; }
        }

        private class SetA
        {
            public int Id { get; set; }
        }

        private class SetB
        {
            public int Id { get; set; }
        }

        [ConditionalFact]
        public void Caches_model_by_context_type()
        {
            var serviceProvider = InMemoryTestHelpers.Instance.CreateContextServices();
            var modelSource = serviceProvider.GetRequiredService<IModelSource>();
            var testModelDependencies = serviceProvider.GetRequiredService<ModelCreationDependencies>();

            var model1 = modelSource.GetModel(new Context1(), testModelDependencies, designTime: false);
            var model2 = modelSource.GetModel(new Context2(), testModelDependencies, designTime: false);

            var designModel1 = modelSource.GetModel(new Context1(), testModelDependencies, designTime: true);
            var designModel2 = modelSource.GetModel(new Context2(), testModelDependencies, designTime: true);

            Assert.NotSame(model1, model2);
            Assert.Same(model1, modelSource.GetModel(new Context1(), testModelDependencies, designTime: false));
            Assert.Same(model2, modelSource.GetModel(new Context2(), testModelDependencies, designTime: false));

            Assert.NotSame(designModel1, designModel2);
            Assert.Same(designModel1, modelSource.GetModel(new Context1(), testModelDependencies, designTime: true));
            Assert.Same(designModel2, modelSource.GetModel(new Context2(), testModelDependencies, designTime: true));

            Assert.NotSame(model1, designModel1);
            Assert.NotSame(model2, designModel2);
        }

        [ConditionalFact]
        public void Model_from_options_is_preserved()
        {
            var serviceProvider = InMemoryTestHelpers.Instance.CreateContextServices();
            var modelSource = serviceProvider.GetRequiredService<IModelSource>();
            var testModelDependencies = serviceProvider.GetRequiredService<ModelCreationDependencies>();

            var model = modelSource.GetModel(new Context1(), testModelDependencies, designTime: false);
            var designTimeModel = modelSource.GetModel(new Context1(), testModelDependencies, designTime: true);

            Assert.NotSame(model, designTimeModel);

            var context = new ModelContext(model);

            Assert.NotSame(context.Model, context.GetService<IDesignTimeModel>().Model);
            Assert.Same(model, context.Model);
            Assert.NotSame(model, context.GetService<IDesignTimeModel>().Model);

            var designTimeContext = new ModelContext(designTimeModel);

            Assert.NotSame(context.Model, designTimeContext.GetService<IDesignTimeModel>().Model);
            Assert.NotSame(model, designTimeContext.Model);
            Assert.Same(designTimeModel, designTimeContext.GetService<IDesignTimeModel>().Model);
        }

        private class ModelContext : DbContext
        {
            private readonly IModel _model;

            public ModelContext(IModel model)
            {
                _model = model;
            }

            protected internal override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            {
                optionsBuilder = optionsBuilder
                    .UseInternalServiceProvider(InMemoryFixture.DefaultServiceProvider)
                    .UseInMemoryDatabase(nameof(ModelContext));

                if (_model != null)
                {
                    optionsBuilder.UseModel(_model);
                }
            }
        }

        [ConditionalFact]
        public void Stores_model_version_information_as_annotation_on_model()
        {
            var serviceProvider = InMemoryTestHelpers.Instance.CreateContextServices();
            var modelSource = serviceProvider.GetRequiredService<IModelSource>();
            var testModelDependencies = serviceProvider.GetRequiredService<ModelCreationDependencies>();

            var model = modelSource.GetModel(new Context1(), testModelDependencies, designTime: false);
            var packageVersion = typeof(Context1).Assembly.GetCustomAttributes<AssemblyMetadataAttribute>()
                .Single(m => m.Key == "PackageVersion").Value;

            var prereleaseIndex = packageVersion.IndexOf("-", StringComparison.Ordinal);
            if (prereleaseIndex != -1)
            {
                packageVersion = packageVersion.Substring(0, prereleaseIndex);
            }

            Assert.StartsWith(packageVersion, model.GetProductVersion(), StringComparison.OrdinalIgnoreCase);
        }

        private class Context1 : DbContext
        {
        }

        private class Context2 : DbContext
        {
        }

        private static RuntimeConventionSetBuilder CreateRuntimeConventionSetBuilder(
            IDbSetFinder setFinder,
            IServiceProvider serviceProvider)
            => new RuntimeConventionSetBuilder(
                new ProviderConventionSetBuilder(
                    serviceProvider.GetRequiredService<ProviderConventionSetBuilderDependencies>() with { SetFinder = setFinder }),
                    new List<IConventionSetPlugin>());
    }
}
