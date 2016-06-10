// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Infrastructure
{
    /// <summary>
    ///     Provides information about the environment an application is running in.
    /// </summary>
    public class DbContextFactoryOptions
    {
        /// <summary>
        ///     Gets or sets the directory containing the application.
        /// </summary>
        public virtual string ApplicationBasePath { get; [param: CanBeNull] set; }

        /// <summary>
        ///     Gets or sets the directory containing the application content files.
        /// </summary>
        public virtual string ContentRootPath { get; [param: CanBeNull] set; }

        /// <summary>
        ///     Gets or sets the name of the environment.
        /// </summary>
        public virtual string EnvironmentName { get; [param: CanBeNull] set; }
    }
}
