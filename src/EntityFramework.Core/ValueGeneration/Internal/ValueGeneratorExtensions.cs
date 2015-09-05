// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;

namespace Microsoft.Data.Entity.ValueGeneration.Internal
{
    public static class ValueGeneratorExtensions
    {
        public static object NextSkippingSentinel([NotNull] this ValueGenerator valueGenerator, [NotNull] IProperty property)
        {
            var value = valueGenerator.Next();

            if (property.IsSentinelValue(value))
            {
                value = valueGenerator.Next();
            }

            return value;
        }
    }
}
