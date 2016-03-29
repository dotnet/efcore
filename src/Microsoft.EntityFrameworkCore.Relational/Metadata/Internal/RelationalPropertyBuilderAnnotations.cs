// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal
{
    public class RelationalPropertyBuilderAnnotations : RelationalPropertyAnnotations
    {
        public RelationalPropertyBuilderAnnotations(
            [NotNull] InternalPropertyBuilder internalBuilder,
            ConfigurationSource configurationSource,
            [CanBeNull] RelationalFullAnnotationNames providerFullAnnotationNames)
            : base(new RelationalAnnotationsBuilder(internalBuilder, configurationSource), providerFullAnnotationNames)
        {
        }

        public virtual bool HasColumnName([CanBeNull] string value) => SetColumnName(value);

        public virtual bool HasColumnType([CanBeNull] string value) => SetColumnType(value);

        public virtual bool HasDefaultValueSql([CanBeNull] string value) => SetDefaultValueSql(value);

        public virtual bool HasComputedValueSql([CanBeNull] string value) => SetComputedValueSql(value);

        public virtual bool HasDefaultValue([CanBeNull] object value) => SetDefaultValue(value);
    }
}
