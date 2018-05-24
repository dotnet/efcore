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
    [AttributeUsage(AttributeTargets.Assembly)]
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
        {
            Check.NotEmpty(typeName, nameof(typeName));

            TypeName = typeName;
        }

        /// <summary>
        ///     Gets the assembly-qualified name of the type that can be used to add additional design time services to a
        ///     <see cref="ServiceCollection" />.
        ///     This type should implement <see cref="IDesignTimeServices" />.
        /// </summary>
        public string TypeName { get; }
    }
}
