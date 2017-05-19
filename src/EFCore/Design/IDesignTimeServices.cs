// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.Design
{
    /// <summary>
    ///     Enables configuring design-time services. Tools will automatically discover implementations of this
    ///     interface that are in the startup assembly.
    /// </summary>
    public interface IDesignTimeServices
    {
        /// <summary>
        ///     Configures design-time services. Use this method to override the default design-time services with your
        ///     own implementations.
        /// </summary>
        /// <param name="serviceCollection"> The design-time service collection. </param>
        void ConfigureDesignTimeServices([NotNull] IServiceCollection serviceCollection);
    }
}
