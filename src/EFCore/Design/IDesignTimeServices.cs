// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

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
        void ConfigureDesignTimeServices(IServiceCollection serviceCollection);
    }
}
