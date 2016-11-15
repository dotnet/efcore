// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal
{
    public class SqlServerValueGeneratorConvention : ValueGeneratorConvention
    {
        public override Annotation Apply(InternalPropertyBuilder propertyBuilder, string name, Annotation annotation, Annotation oldAnnotation)
        {
            if (name == SqlServerFullAnnotationNames.Instance.ValueGenerationStrategy)
            {
                SetRequiresValueGenerator(propertyBuilder, propertyBuilder.Metadata.GetValueGeneratorFactory() != null);
            }

            return base.Apply(propertyBuilder, name, annotation, oldAnnotation);
        }

        protected override void SetRequiresValueGenerator(InternalPropertyBuilder propertyBuilder, bool valueGeneratorFactorySet)
            => propertyBuilder.RequiresValueGenerator(
                valueGeneratorFactorySet
                || propertyBuilder.Metadata.SqlServer().ValueGenerationStrategy == SqlServerValueGenerationStrategy.SequenceHiLo
                    ? true
                    : (bool?)null,
                ConfigurationSource.Convention);
    }
}
