// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.EntityFrameworkCore.ValueGeneration;

namespace Microsoft.EntityFrameworkCore
{
    /// <summary>
    ///     Extension methods for <see cref="IComplexPropertyDefinition" />.
    /// </summary>
    public static class ComplexPropertyDefinitionExtensions
    {
        /// <summary>
        ///     TODO: ComplexType docs
        /// </summary>
        public static Func<IProperty, IEntityType, ValueGenerator> GetValueGeneratorFactoryDefault([NotNull] this IComplexPropertyDefinition property)
        {
            Check.NotNull(property, nameof(property));

            return (Func<IProperty, IEntityType, ValueGenerator>)property[CoreAnnotationNames.ValueGeneratorFactoryAnnotation];
        }

        /// <summary>
        ///     TODO: ComplexType docs
        /// </summary>
        public static int? GetMaxLengthDefault([NotNull] this IComplexPropertyDefinition property)
        {
            Check.NotNull(property, nameof(property));

            return (int?)property[CoreAnnotationNames.MaxLengthAnnotation];
        }

        /// <summary>
        ///     TODO: ComplexType docs
        /// </summary>
        public static bool? IsUnicodeDefault([NotNull] this IComplexPropertyDefinition property)
        {
            Check.NotNull(property, nameof(property));

            return (bool?)property[CoreAnnotationNames.UnicodeAnnotation];
        }
    }
}
