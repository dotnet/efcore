// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Design.Internal;

namespace Microsoft.EntityFrameworkCore.Scaffolding.Internal;

/// <summary>
///     Wraps a service provider so that scaffolding templates are handed an <see cref="ICSharpHelper" /> that emits LF
///     line endings, without changing the shared <see cref="ICSharpHelper" /> that migrations, snapshots and compiled
///     models use. A helper that has been replaced or subclassed by the application is left alone.
/// </summary>
internal sealed class LfCSharpHelperServiceProvider(IServiceProvider inner) : IServiceProvider
{
    private ICSharpHelper? _helper;

    public object? GetService(Type serviceType)
    {
        var service = inner.GetService(serviceType);

        if (serviceType != typeof(ICSharpHelper)
            || service?.GetType() != typeof(CSharpHelper))
        {
            return service;
        }

        return _helper ??= new ScaffoldingCSharpHelper(inner.GetRequiredService<ITypeMappingSource>());
    }
}
