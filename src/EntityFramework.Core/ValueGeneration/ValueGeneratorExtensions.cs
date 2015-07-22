// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.ValueGeneration
{
    public static class ValueGeneratorExtensions
    {
        public static object NextSkippingSentinel([NotNull] this ValueGenerator valueGenerator, [NotNull] IProperty property)
        {
            Check.NotNull(valueGenerator, nameof(valueGenerator));
            Check.NotNull(property, nameof(property));

            var value = valueGenerator.Next();

            if (property.IsSentinelValue(value))
            {
                value = valueGenerator.Next();
            }

            return value;
        }
    }
}
