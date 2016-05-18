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

        protected new virtual RelationalAnnotationsBuilder Annotations => (RelationalAnnotationsBuilder)base.Annotations;
        private InternalPropertyBuilder PropertyBuilder => ((Property)Property).Builder;
        protected override bool ShouldThrowOnConflict => false;

        public virtual bool HasColumnName([CanBeNull] string value) => SetColumnName(value);

        public virtual bool HasColumnType([CanBeNull] string value) => SetColumnType(value);

        public virtual bool HasDefaultValueSql([CanBeNull] string value)
        {
            PropertyBuilder.ValueGenerated(ValueGenerated.OnAdd, ConfigurationSource.Convention);

            return SetDefaultValueSql(value);
        }

        public virtual bool HasComputedColumnSql([CanBeNull] string value)
        {
            PropertyBuilder.ValueGenerated(ValueGenerated.OnAddOrUpdate, ConfigurationSource.Convention);

            return SetComputedColumnSql(value);
        }

        public virtual bool HasDefaultValue([CanBeNull] object value)
        {
            PropertyBuilder.ValueGenerated(ValueGenerated.OnAdd, ConfigurationSource.Convention);

            return SetDefaultValue(value);
        }
    }
}
