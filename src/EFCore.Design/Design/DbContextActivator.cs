// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Design.Internal;

namespace Microsoft.EntityFrameworkCore.Design;

/// <summary>
///     Used to instantiate <see cref="DbContext" /> types at design time.
/// </summary>
public static class DbContextActivator
{
    /// <summary>
    ///     Creates an instance of the specified <see cref="DbContext" /> type using the standard design-time
    ///     mechanisms. When available, this will use any <see cref="IDesignTimeDbContextFactory{TContext}" />
    ///     implementations or the application's service provider.
    /// </summary>
    /// <param name="contextType">The <see cref="DbContext" /> type to instantiate.</param>
    /// <param name="startupAssembly">The application's startup assembly.</param>
    /// <param name="reportHandler">The design-time report handler.</param>
    /// <returns>The newly created object.</returns>
    public static DbContext CreateInstance(
        Type contextType,
        Assembly? startupAssembly = null,
        IOperationReportHandler? reportHandler = null)
        => CreateInstance(contextType, startupAssembly, reportHandler, null);

    /// <summary>
    ///     Creates an instance of the specified <see cref="DbContext" /> type using the standard design-time
    ///     mechanisms. When available, this will use any <see cref="IDesignTimeDbContextFactory{TContext}" />
    ///     implementations or the application's service provider.
    /// </summary>
    /// <param name="contextType">The <see cref="DbContext" /> type to instantiate.</param>
    /// <param name="startupAssembly">The application's startup assembly.</param>
    /// <param name="reportHandler">The design-time report handler.</param>
    /// <param name="args">Arguments passed to the application.</param>
    /// <returns>The newly created object.</returns>
    public static DbContext CreateInstance(
        Type contextType,
        Assembly? startupAssembly,
        IOperationReportHandler? reportHandler,
        string[]? args)
    {
        Check.NotNull(contextType, nameof(contextType));

        EF.IsDesignTime = true;

        return new DbContextOperations(
                new OperationReporter(reportHandler),
                contextType.Assembly,
                startupAssembly ?? contextType.Assembly,
                projectDir: "",
                rootNamespace: null,
                language: "C#",
                nullable: false,
                args: args ?? [])
            .CreateContext(contextType.FullName!);
    }
}
