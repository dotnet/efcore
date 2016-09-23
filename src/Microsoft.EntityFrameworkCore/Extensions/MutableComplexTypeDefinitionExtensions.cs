// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore
{
    /// <summary>
    ///     Extension methods for <see cref="IMutableComplexTypeDefinition" />.
    /// </summary>
    public static class MutableComplexTypeDefinitionExtensions
    {
        /// <summary>
        ///     TODO: ComplexType docs
        /// </summary>
        public static IMutableComplexPropertyDefinition AddPropertyDefinition(
            [NotNull] this IMutableComplexTypeDefinition typeDefinition, [NotNull] PropertyInfo propertyInfo)
        {
            Check.NotNull(typeDefinition, nameof(typeDefinition));
            Check.NotNull(propertyInfo, nameof(propertyInfo));

            return typeDefinition.AsComplexTypeDefinition().AddPropertyDefinition(propertyInfo);
        }
    }
}
