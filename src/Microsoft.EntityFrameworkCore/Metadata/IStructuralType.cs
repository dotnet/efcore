// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Microsoft.EntityFrameworkCore.Metadata
{
    /// <summary>
    ///     Represents a type in an <see cref="IModel" />.
    /// </summary>
    public interface IStructuralType : IAnnotatable
    {
        /// <summary>
        ///     Gets the model this type belongs to.
        /// </summary>
        IModel Model { get; }

        /// <summary>
        ///     Gets the name of the type.
        /// </summary>
        string Name { get; }

        /// <summary>
        ///     <para>
        ///         Gets the CLR class that is used to represent instances of this type. Returns null if the type does not have a
        ///         corresponding CLR class (known as a shadow type).
        ///     </para>
        ///     <para>
        ///         Shadow types are not currently supported in a model that is used at runtime with a <see cref="DbContext" />.
        ///         Therefore, shadow types will only exist in migration model snapshots, etc.
        ///     </para>
        /// </summary>
        Type ClrType { get; }

        /// <summary>
        ///     <para>
        ///         Gets the property with a given name. Returns null if no property with the given name is defined.
        ///     </para>
        ///     <para>
        ///         This API only finds scalar properties and does not find navigation properties. Use
        ///         <see cref="EntityTypeExtensions.FindNavigation(IEntityType, string)" /> to find a navigation property.
        ///     </para>
        /// </summary>
        /// <param name="name"> The name of the property. </param>
        /// <returns> The property, or null if none is found. </returns>
        IStructuralProperty FindProperty([NotNull] string name);

        /// <summary>
        ///     <para>
        ///         Gets the properties defined on this type.
        ///     </para>
        ///     <para>
        ///         This API only returns scalar properties and does not return navigation properties. Use
        ///         <see cref="EntityTypeExtensions.GetNavigations(IEntityType)" /> to get navigation properties.
        ///     </para>
        /// </summary>
        /// <returns> The properties defined on this type. </returns>
        IEnumerable<IStructuralProperty> GetProperties();

        /// <summary>
        ///     TODO: ComplexType docs
        /// </summary>
        IComplexTypeReference FindComplexTypeReference([NotNull] string name);

        /// <summary>
        ///     TODO: ComplexType docs
        /// </summary>
        IEnumerable<IComplexTypeReference> GetComplexTypeReferences();
    }
}
