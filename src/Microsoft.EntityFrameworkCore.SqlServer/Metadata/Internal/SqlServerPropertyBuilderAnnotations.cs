// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal
{
    public class SqlServerPropertyBuilderAnnotations : SqlServerPropertyAnnotations
    {
        public SqlServerPropertyBuilderAnnotations(
            [NotNull] InternalPropertyBuilder internalBuilder,
            ConfigurationSource configurationSource)
            : base(new RelationalAnnotationsBuilder(internalBuilder, configurationSource))
        {
        }

        private InternalPropertyBuilder PropertyBuilder => ((Property)Property).Builder;
        protected override bool ShouldThrowOnConflict => false;

#pragma warning disable 109
        public new virtual bool ColumnName([CanBeNull] string value) => SetColumnName(value);

        public new virtual bool ColumnType([CanBeNull] string value) => SetColumnType(value);

        public new virtual bool DefaultValueSql([CanBeNull] string value)
        {
            PropertyBuilder.ValueGenerated(ValueGenerated.OnAdd, ConfigurationSource.Convention);
            return SetDefaultValueSql(value);
        }

        public new virtual bool ComputedColumnSql([CanBeNull] string value)
        {
            PropertyBuilder.ValueGenerated(ValueGenerated.OnAddOrUpdate, ConfigurationSource.Convention);
            return SetComputedColumnSql(value);
        }

        public new virtual bool DefaultValue([CanBeNull] object value)
        {
            PropertyBuilder.ValueGenerated(ValueGenerated.OnAdd, ConfigurationSource.Convention);
            return SetDefaultValue(value);
        }

        public new virtual bool HiLoSequenceName([CanBeNull] string value) => SetHiLoSequenceName(value);

        public new virtual bool HiLoSequenceSchema([CanBeNull] string value) => SetHiLoSequenceSchema(value);

        public new virtual bool ValueGenerationStrategy(SqlServerValueGenerationStrategy? value) => SetValueGenerationStrategy(value);
#pragma warning restore 109
    }
}
