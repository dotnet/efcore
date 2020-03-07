// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.Design
{
    /// <summary>
    ///     <para>
    ///         Identifies where to find additional design time services.
    ///     </para>
    ///     <para>
    ///         This attribute is typically used by design-time extensions. It is generally not used in application code.
    ///     </para>
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    public sealed class DesignTimeServicesReferenceAttribute : Attribute
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="DesignTimeServicesReferenceAttribute" /> class.
        /// </summary>
        /// <param name="typeName">
        ///     The assembly-qualified name of the type that can be used to add additional design time services to a <see cref="ServiceCollection" />.
        ///     This type should implement <see cref="IDesignTimeServices" />.
        /// </param>
        public DesignTimeServicesReferenceAttribute([NotNull] string typeName)
            : this(typeName, forProvider: null)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="DesignTimeServicesReferenceAttribute" /> class.
        /// </summary>
        /// <param name="typeName">
        ///     The assembly-qualified name of the type that can be used to add additional design time services to a <see cref="ServiceCollection" />.
        ///     This type should implement <see cref="IDesignTimeServices" />.
        /// </param>
        /// <param name="forProvider">
        ///     The name of the provider for which these services should be added. If null, the services will be added
        ///     for all providers.
        /// </param>
        public DesignTimeServicesReferenceAttribute([NotNull] string typeName, [CanBeNull] string forProvider)
        {
            Check.NotEmpty(typeName, nameof(typeName));

            TypeName = typeName;
            ForProvider = forProvider;
        }

        /// <summary>
        ///     Gets the assembly-qualified name of the type that can be used to add additional design time services to a
        ///     <see cref="ServiceCollection" />.
        ///     This type should implement <see cref="IDesignTimeServices" />.
        /// </summary>
        public string TypeName { get; }

        /// <summary>
        ///     Gets the name of the provider for which these services should be added. If null, the services will be
        ///     added for all providers.
        /// </summary>
        public string ForProvider { get; }
    }
}
