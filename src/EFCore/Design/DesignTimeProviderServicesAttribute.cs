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
        ///     Initializes a new instance of the <see cref="DesignTimeProviderServicesAttribute" /> class.
        /// </summary>
        /// <param name="typeName">
        ///     The name of the type that can be used to add the database providers design time services to a <see cref="ServiceCollection" />.
        ///     This type should implement <see cref="IDesignTimeServices" />.
        /// </param>
        public DesignTimeProviderServicesAttribute(
            [NotNull] string typeName)
        {
            Check.NotEmpty(typeName, nameof(typeName));

            TypeName = typeName;
        }

        /// <summary>
        ///     Gets the name of the type that can be used to add the database providers design time services to a <see cref="ServiceCollection" />.
        ///     This type should implement <see cref="IDesignTimeServices" />.
        /// </summary>
        public string TypeName { get; }
    }
}
