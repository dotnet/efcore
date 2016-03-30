// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal
{
    public class RelationalForeignKeyBuilderAnnotations : RelationalForeignKeyAnnotations
    {
        public RelationalForeignKeyBuilderAnnotations(
            [NotNull] InternalRelationshipBuilder internalBuilder,
            ConfigurationSource configurationSource,
            [CanBeNull] RelationalFullAnnotationNames providerFullAnnotationNames)
            : base(new RelationalAnnotationsBuilder(internalBuilder, configurationSource), providerFullAnnotationNames)
        {
        }

        public virtual bool HasConstraintName([CanBeNull] string value) => SetName(value);
    }
}
