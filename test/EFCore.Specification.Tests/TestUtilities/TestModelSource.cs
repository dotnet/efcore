// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Internal;

namespace Microsoft.EntityFrameworkCore.TestUtilities;

public class TestModelSource : ModelSource
{
    private readonly Action<ModelConfigurationBuilder>? _configureConventions;
    private readonly Action<ModelBuilder, DbContext> _onModelCreating;
    private readonly bool _skipValidation;

    private TestModelSource(
        Action<ModelConfigurationBuilder>? configureConventions,
        Action<ModelBuilder, DbContext> onModelCreating,
        ModelSourceDependencies dependencies,
        bool skipValidation = false)
        : base(dependencies)
    {
        _configureConventions = configureConventions;
        _onModelCreating = onModelCreating;
        _skipValidation = skipValidation;
    }

    public override IModel CreateModel(
        DbContext context,
        ModelCreationDependencies modelCreationDependencies,
        bool designTime)
    {
        var model = CreateModel(context, modelCreationDependencies.ConventionSetBuilder, modelCreationDependencies.ModelDependencies);
        return modelCreationDependencies.ModelRuntimeInitializer.Initialize(
            model, designTime, _skipValidation ? null : modelCreationDependencies.ValidationLogger);
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
        Action<ModelConfigurationBuilder>? configureConventions = null,
        bool skipValidation = false)
        => p => new TestModelSource(
            configureConventions,
            (mb, c) => onModelCreating(mb),
            p.GetRequiredService<ModelSourceDependencies>(),
            skipValidation);

    public static Func<IServiceProvider, IModelSource> GetFactory(
        Action<ModelBuilder, DbContext> onModelCreating,
        Action<ModelConfigurationBuilder>? configureConventions = null,
        bool skipValidation = false)
        => p => new TestModelSource(
            configureConventions,
            onModelCreating,
            p.GetRequiredService<ModelSourceDependencies>(),
            skipValidation);
}
