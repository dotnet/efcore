// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Oracle.Metadata.Internal
{
    public class OraclePropertyBuilderAnnotations : OraclePropertyAnnotations
    {
        public OraclePropertyBuilderAnnotations(
            [NotNull] InternalPropertyBuilder internalBuilder,
            ConfigurationSource configurationSource)
            : base(new RelationalAnnotationsBuilder(internalBuilder, configurationSource))
        {
        }

        protected new virtual RelationalAnnotationsBuilder Annotations => (RelationalAnnotationsBuilder)base.Annotations;

        protected override bool ShouldThrowOnConflict => false;

        protected override bool ShouldThrowOnInvalidConfiguration => Annotations.ConfigurationSource == ConfigurationSource.Explicit;

#pragma warning disable 109

        public new virtual bool ColumnName([CanBeNull] string value) => SetColumnName(value);

        public new virtual bool ColumnType([CanBeNull] string value) => SetColumnType(value);

        public new virtual bool DefaultValueSql([CanBeNull] string value) => SetDefaultValueSql(value);

        public new virtual bool ComputedColumnSql([CanBeNull] string value) => SetComputedColumnSql(value);

        public new virtual bool DefaultValue([CanBeNull] object value) => SetDefaultValue(value);

        public new virtual bool HiLoSequenceName([CanBeNull] string value) => SetHiLoSequenceName(value);

        public new virtual bool ValueGenerationStrategy(OracleValueGenerationStrategy? value)
        {
            if (!SetValueGenerationStrategy(value))
            {
                return false;
            }

            if (value == null)
            {
                HiLoSequenceName(null);
            }
            else if (value.Value == OracleValueGenerationStrategy.IdentityColumn)
            {
                HiLoSequenceName(null);
            }

            return true;
        }
#pragma warning restore 109
    }
}
