// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Linq;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.XuGu.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.XuGu.Internal
{
    public static class XGValueGenerationStrategyCompatibility
    {
        public static XGValueGenerationStrategy? GetValueGenerationStrategy(IAnnotation[] annotations)
        {
            var valueGenerationStrategy = ObjectToEnumConverter.GetEnumValue<XGValueGenerationStrategy>(
                annotations.FirstOrDefault(a => a.Name == XGAnnotationNames.ValueGenerationStrategy)?.Value);

            if (!valueGenerationStrategy.HasValue ||
                valueGenerationStrategy == XGValueGenerationStrategy.None)
            {
                var generatedOnAddAnnotation = annotations.FirstOrDefault(a => a.Name == XGAnnotationNames.LegacyValueGeneratedOnAdd)?.Value;
                if (generatedOnAddAnnotation != null && (bool)generatedOnAddAnnotation)
                {
                    valueGenerationStrategy = XGValueGenerationStrategy.IdentityColumn;
                }

                var generatedOnAddOrUpdateAnnotation = annotations.FirstOrDefault(a => a.Name == XGAnnotationNames.LegacyValueGeneratedOnAddOrUpdate)?.Value;
                if (generatedOnAddOrUpdateAnnotation != null && (bool)generatedOnAddOrUpdateAnnotation)
                {
                    valueGenerationStrategy = XGValueGenerationStrategy.ComputedColumn;
                }
            }

            return valueGenerationStrategy;
        }
    }
}
