// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Internal;

namespace Microsoft.EntityFrameworkCore.TestUtilities;

public class TestModelSource : ModelSource
{
    private readonly Action<ModelConfigurationBuilder>? _configureConventions;
    private readonly Action<ModelBuilder, DbContext> _onModelCreating;

    private TestModelSource(
        Action<ModelConfigurationBuilder>? configureConventions,
        Action<ModelBuilder, DbContext> onModelCreating,
        ModelSourceDependencies dependencies)
        : base(dependencies)
    {
        _configureConventions = configureConventions;
        _onModelCreating = onModelCreating;
    }

    protected override IModel CreateModel(
        DbContext context,
        IConventionSetBuilder conventionSetBuilder,
        ModelDependencies modelDependencies)
    {
        var modelConfigurationBuilder = new ModelConfigurationBuilder(
            conventionSetBuilder.CreateConventionSet(),
            context.GetInfrastructure());
        context.ConfigureConventions(modelConfigurationBuilder);
        _configureConventions?.Invoke(modelConfigurationBuilder);
        var modelBuilder = modelConfigurationBuilder.CreateModelBuilder(modelDependencies);

        Dependencies.ModelCustomizer.Customize(modelBuilder, context);

        _onModelCreating(modelBuilder, context);

        return modelBuilder.FinalizeModel();
    }

    public static Func<IServiceProvider, IModelSource> GetFactory(
        Action<ModelBuilder> onModelCreating,
        Action<ModelConfigurationBuilder>? configureConventions = null)
        => p => new TestModelSource(
            configureConventions,
            (mb, c) => onModelCreating(mb),
            p.GetRequiredService<ModelSourceDependencies>());

    public static Func<IServiceProvider, IModelSource> GetFactory(
        Action<ModelBuilder, DbContext> onModelCreating,
        Action<ModelConfigurationBuilder>? configureConventions = null)
        => p => new TestModelSource(
            configureConventions,
            onModelCreating,
            p.GetRequiredService<ModelSourceDependencies>());
}
