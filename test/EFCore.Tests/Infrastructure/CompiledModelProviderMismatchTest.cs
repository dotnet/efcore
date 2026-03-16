// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Diagnostics.Internal;
using Microsoft.EntityFrameworkCore.InMemory.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.TestUtilities;

[assembly: DbContextModel(
    typeof(Microsoft.EntityFrameworkCore.Infrastructure.CompiledModelProviderMismatchTest.MismatchedProviderContext),
    typeof(Microsoft.EntityFrameworkCore.Infrastructure.CompiledModelProviderMismatchTest.MismatchedProviderTestModel),
    ProviderName = "Microsoft.EntityFrameworkCore.SqlServer")]

[assembly: DbContextModel(
    typeof(Microsoft.EntityFrameworkCore.Infrastructure.CompiledModelProviderMismatchTest.MultiProviderContext),
    typeof(Microsoft.EntityFrameworkCore.Infrastructure.CompiledModelProviderMismatchTest.WrongProviderTestModel),
    ProviderName = "Microsoft.EntityFrameworkCore.SqlServer")]
[assembly: DbContextModel(
    typeof(Microsoft.EntityFrameworkCore.Infrastructure.CompiledModelProviderMismatchTest.MultiProviderContext),
    typeof(Microsoft.EntityFrameworkCore.Infrastructure.CompiledModelProviderMismatchTest.CorrectProviderTestModel),
    ProviderName = "Microsoft.EntityFrameworkCore.InMemory")]

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.Infrastructure;

public class CompiledModelProviderMismatchTest
{
    [ConditionalFact]
    public void Compiled_model_with_mismatched_provider_is_skipped_and_warning_is_logged()
    {
        var serviceProvider = new ServiceCollection()
            .AddEntityFrameworkInMemoryDatabase().BuildServiceProvider(validateScopes: true);

        var context = new MismatchedProviderContext(serviceProvider);
        var warning = CoreStrings.WarningAsErrorTemplate(
            CoreEventId.CompiledModelProviderMismatchWarning,
            CoreResources.LogCompiledModelProviderMismatch(
                new TestLogger<TestLoggingDefinitions>()).GenerateMessage(
                "Microsoft.EntityFrameworkCore.SqlServer", "Microsoft.EntityFrameworkCore.InMemory"),
            "CoreEventId.CompiledModelProviderMismatchWarning");

        Assert.Equal(
            warning,
            Assert.Throws<InvalidOperationException>(() => context.Model).Message);
    }

    [ConditionalFact]
    public void Compiled_model_with_matching_provider_is_used_when_multiple_attributes_exist()
    {
        var serviceProvider = new ServiceCollection()
            .AddEntityFrameworkInMemoryDatabase().BuildServiceProvider(validateScopes: true);

        using var context = new MultiProviderContext(serviceProvider);
        var model = context.Model;

        Assert.NotNull(model);
        Assert.Same(CorrectProviderTestModel.Instance, model);
    }

    public class MismatchedProviderContext(IServiceProvider serviceProvider) : DbContext
    {
        private readonly IServiceProvider _serviceProvider = serviceProvider;

        protected internal override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder
                .UseInternalServiceProvider(_serviceProvider)
                .UseInMemoryDatabase(nameof(MismatchedProviderContext))
                .ConfigureWarnings(w => w.Default(WarningBehavior.Throw));
    }

    public class MultiProviderContext(IServiceProvider serviceProvider) : DbContext
    {
        private readonly IServiceProvider _serviceProvider = serviceProvider;

        protected internal override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder
                .UseInternalServiceProvider(_serviceProvider)
                .UseInMemoryDatabase(nameof(MultiProviderContext))
                .ConfigureWarnings(w => w.Default(WarningBehavior.Throw));
    }

    public class MismatchedProviderTestModel
    {
        private static readonly RuntimeModel _instance = CreateModel();

        public static IModel Instance
            => _instance;

        private static RuntimeModel CreateModel()
        {
#pragma warning disable CS0618 // Type or member is obsolete
            var model = new RuntimeModel();
#pragma warning restore CS0618
            model.AddAnnotation(CoreAnnotationNames.ProductVersion, ProductInfo.GetVersion());
            return model;
        }
    }

    public class WrongProviderTestModel
    {
        private static readonly RuntimeModel _instance = CreateModel();

        public static IModel Instance
            => _instance;

        private static RuntimeModel CreateModel()
        {
#pragma warning disable CS0618 // Type or member is obsolete
            var model = new RuntimeModel();
#pragma warning restore CS0618
            model.AddAnnotation(CoreAnnotationNames.ProductVersion, ProductInfo.GetVersion());
            return model;
        }
    }

    public class CorrectProviderTestModel
    {
        private static readonly RuntimeModel _instance = CreateModel();

        public static IModel Instance
            => _instance;

        private static RuntimeModel CreateModel()
        {
#pragma warning disable CS0618 // Type or member is obsolete
            var model = new RuntimeModel();
#pragma warning restore CS0618
            model.AddAnnotation(CoreAnnotationNames.ProductVersion, ProductInfo.GetVersion());
            return model;
        }
    }
}
