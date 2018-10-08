// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Microsoft.EntityFrameworkCore.InMemory.Storage.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class InMemoryTypeMapping : CoreTypeMapping
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public InMemoryTypeMapping(
            [NotNull] Type clrType,
            [CanBeNull] ValueComparer comparer = null,
            [CanBeNull] ValueComparer keyComparer = null,
            [CanBeNull] ValueComparer structuralComparer = null)
            : base(new CoreTypeMappingParameters(
                clrType,
                converter: null,
                comparer,
                keyComparer,
                structuralComparer,
                valueGeneratorFactory: null))
        {
        }

        private InMemoryTypeMapping(CoreTypeMappingParameters parameters)
            : base(parameters)
        {
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override CoreTypeMapping Clone(ValueConverter converter)
            => new InMemoryTypeMapping(Parameters.WithComposedConverter(converter));
    }
}
