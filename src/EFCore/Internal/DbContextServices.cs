// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore.Infrastructure.Internal;

namespace Microsoft.EntityFrameworkCore.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class DbContextServices : IDbContextServices
{
    private IServiceProvider? _scopedProvider;
    private DbContextOptions? _contextOptions;
    private ICurrentDbContext? _currentContext;
    private IModel? _model;
    private IModel? _designTimeModel;
    private bool _inOnModelCreating;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IDbContextServices Initialize(
        IServiceProvider scopedProvider,
        DbContextOptions contextOptions,
        DbContext context)
    {
        _scopedProvider = scopedProvider;
        _contextOptions = contextOptions;
        _currentContext = new CurrentDbContext(context);

        var providers = _scopedProvider.GetService<IEnumerable<IDatabaseProvider>>()?.ToList();
        var providerCount = providers?.Count ?? 0;

        if (providerCount > 1)
        {
            throw new InvalidOperationException(CoreStrings.MultipleProvidersConfigured(BuildDatabaseNamesString(providers!)));
        }

        if (providerCount == 0
            || !providers![0].IsConfigured(contextOptions))
        {
            throw new InvalidOperationException(CoreStrings.NoProviderConfigured);
        }

        return this;
    }

    private static string BuildDatabaseNamesString(IEnumerable<IDatabaseProvider> available)
        => string.Join(", ", available.Select(e => "'" + e.Name + "'"));

    private IModel CreateModel(bool designTime)
    {
        if (_inOnModelCreating)
        {
            throw new InvalidOperationException(CoreStrings.RecursiveOnModelCreating);
        }

        try
        {
            _inOnModelCreating = true;

            var dependencies = _scopedProvider!.GetRequiredService<ModelCreationDependencies>();
            var modelFromOptions = CoreOptions?.Model ?? FindCompiledModel(_currentContext!.Context.GetType());

            var modelVersion = modelFromOptions?.GetProductVersion();
            if (modelVersion != null)
            {
                var modelMinorVersion = modelVersion[..modelVersion.LastIndexOf('.')];
                var productVersion = ProductInfo.GetVersion();
                var productMinorVersion = productVersion[..productVersion.LastIndexOf('.')];

                if (modelMinorVersion != productMinorVersion)
                {
                    var logger = _scopedProvider!.GetRequiredService<IDiagnosticsLogger<DbLoggerCategory.Infrastructure>>();
                    logger.OldModelVersionWarning(_currentContext!.Context, _contextOptions!);
                }
            }

            return modelFromOptions == null
                || (designTime && modelFromOptions is not Metadata.Internal.Model)
                    ? RuntimeFeature.IsDynamicCodeSupported
                        ? dependencies.ModelSource.GetModel(_currentContext!.Context, dependencies, designTime)
                        : throw new InvalidOperationException(CoreStrings.NativeAotDesignTimeModel)
                    : dependencies.ModelRuntimeInitializer.Initialize(modelFromOptions, designTime, dependencies.ValidationLogger);
        }
        finally
        {
            _inOnModelCreating = false;
        }

        static IModel? FindCompiledModel(Type contextType)
        {
            var contextAssembly = contextType.Assembly;
            IModel? model = null;
            foreach (var modelAttribute in contextAssembly.GetCustomAttributes<DbContextModelAttribute>())
            {
                if (modelAttribute.ContextType != contextType)
                {
                    continue;
                }

                var modelType = modelAttribute.ModelType;

                var instanceProperty = modelType.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static);
                if (instanceProperty == null
                    || instanceProperty.PropertyType != typeof(IModel))
                {
                    throw new InvalidOperationException(CoreStrings.CompiledModelMissingInstance(modelType.DisplayName()));
                }

                if (model != null)
                {
                    throw new InvalidOperationException(CoreStrings.CompiledModelDuplicateAttribute(
                        contextAssembly.FullName, contextType.DisplayName()));
                }

                model = (IModel)instanceProperty.GetValue(null)!;
            }

            return model;
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual ICurrentDbContext CurrentContext
        => _currentContext!;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IModel Model
        => _model ??= CreateModel(designTime: false);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IModel DesignTimeModel
        => _designTimeModel ??= CreateModel(designTime: true);

    private CoreOptionsExtension? CoreOptions
        => _contextOptions?.FindExtension<CoreOptionsExtension>();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual DbContextOptions ContextOptions
        => _contextOptions!;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IServiceProvider InternalServiceProvider
        => _scopedProvider!;
}
