// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;

namespace Microsoft.Data.Entity.Metadata.Internal
{
    public class RelationalPropertyBuilderAnnotations : RelationalPropertyAnnotations
    {
        public RelationalPropertyBuilderAnnotations(
            [NotNull] InternalPropertyBuilder internalBuilder,
            ConfigurationSource configurationSource,
            [CanBeNull] string providerPrefix)
            : base(new RelationalAnnotationsBuilder(internalBuilder, configurationSource, providerPrefix))
        {
        }
        
        public new virtual bool ColumnName([CanBeNull] string value) => SetColumnName(value);

        public new virtual bool ColumnType([CanBeNull] string value) => SetColumnType(value);

        public new virtual bool GeneratedValueSql([CanBeNull] string value) => SetGeneratedValueSql(value);

        public new virtual bool DefaultValue([CanBeNull] object value) => SetDefaultValue(value);
    }
}
