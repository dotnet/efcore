// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Design.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Design
{
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
        /// <param name="contextType"> The <see cref="DbContext" /> type to instantiate. </param>
        /// <param name="startupAssembly"> The application's startup assembly. </param>
        /// <param name="reportHandler"> The design-time report handler. </param>
        /// <returns> The newly created object. </returns>
        public static DbContext CreateInstance(
            [NotNull] Type contextType,
            [CanBeNull] Assembly startupAssembly = null,
            [CanBeNull] IOperationReportHandler reportHandler = null)
        {
            Check.NotNull(contextType, nameof(contextType));

            return new DbContextOperations(
                    new OperationReporter(reportHandler),
                    contextType.Assembly,
                    startupAssembly ?? contextType.Assembly,
                    args: Array.Empty<string>()) // TODO: Issue #8332
                .CreateContext(contextType.FullName);
        }
    }
}
