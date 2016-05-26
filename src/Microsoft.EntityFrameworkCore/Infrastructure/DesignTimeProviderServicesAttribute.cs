// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.Infrastructure
{
    /// <summary>
    ///     <para>
    ///         Identifies where to find the design time services for a given database provider. This attribute should
    ///         be present in the primary assembly of the database provider.
    ///     </para>
    ///     <para>
    ///         This attribute is typically used by database providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly)]
    public sealed class DesignTimeProviderServicesAttribute : Attribute
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="DesignTimeProviderServicesAttribute"/> class.
        /// </summary>
        /// <param name="typeName"> 
        ///     The name of the type that can be used to add the database providers design time services to a <see cref="ServiceCollection"/>. 
        ///     This type should contain a method with the following signature 
        ///     <code>public IServiceCollection ConfigureDesignTimeServices(IServiceCollection serviceCollection)</code>.
        /// </param>
        /// <param name="assemblyName">
        ///     The name of the assembly that contains the design time services.
        /// </param>
        /// <param name="packageName">
        ///     The NuGet package name that contains the design time services.
        /// </param>
        public DesignTimeProviderServicesAttribute(
            [NotNull] string typeName, [NotNull] string assemblyName, [NotNull] string packageName)
        {
            Check.NotEmpty(typeName, nameof(typeName));
            Check.NotEmpty(assemblyName, nameof(assemblyName));
            Check.NotEmpty(packageName, nameof(packageName));

            TypeName = typeName;
            AssemblyName = assemblyName;
            PackageName = packageName;
        }

        /// <summary>
        ///     Gets the name of the type that can be used to add the database providers design time services to a <see cref="ServiceCollection"/>. 
        ///     This type should contain a method with the following signature 
        ///     <code>public IServiceCollection ConfigureDesignTimeServices(IServiceCollection serviceCollection)</code>.
        /// </summary>
        public string TypeName { get; }

        /// <summary>
        ///     Gets the name of the assembly that contains the design time services.
        /// </summary>
        public string AssemblyName { get; }

        /// <summary>
        ///     Gets the NuGet package name that contains the design time services.
        /// </summary>
        public string PackageName { get; }
    }
}
