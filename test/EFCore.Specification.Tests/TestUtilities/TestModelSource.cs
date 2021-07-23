// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.TestUtilities
{
    public class TestModelSource : ModelSource
    {
        private readonly Action<ModelBuilder, DbContext> _onModelCreating;

        private TestModelSource(Action<ModelBuilder, DbContext> onModelCreating, ModelSourceDependencies dependencies)
            : base(dependencies)
        {
            _onModelCreating = onModelCreating;
        }

        protected override IModel CreateModel(
            DbContext context,
            IConventionSetBuilder conventionSetBuilder,
            ModelDependencies modelDependencies)
        {
            var modelBuilder = new ModelBuilder(conventionSetBuilder.CreateConventionSet(), modelDependencies);

            Dependencies.ModelCustomizer.Customize(modelBuilder, context);

            _onModelCreating(modelBuilder, context);

            return modelBuilder.FinalizeModel();
        }

        public static Func<IServiceProvider, IModelSource> GetFactory(Action<ModelBuilder> onModelCreating)
            => p => new TestModelSource(
                (mb, c) => onModelCreating(mb),
                p.GetRequiredService<ModelSourceDependencies>());

        public static Func<IServiceProvider, IModelSource> GetFactory(Action<ModelBuilder, DbContext> onModelCreating)
            => p => new TestModelSource(
                onModelCreating,
                p.GetRequiredService<ModelSourceDependencies>());
    }
}
