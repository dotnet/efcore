// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.EntityFrameworkCore.Metadata.Internal
{
    public class RelationalFullAnnotationNames
    {
        protected RelationalFullAnnotationNames(string prefix)
        {
            ColumnName = prefix + RelationalAnnotationNames.ColumnName;
            ColumnType = prefix + RelationalAnnotationNames.ColumnType;
            GeneratedValueSql = prefix + RelationalAnnotationNames.GeneratedValueSql;
            DefaultValue = prefix + RelationalAnnotationNames.DefaultValue;
            DatabaseName = prefix + RelationalAnnotationNames.DatabaseName;
            TableName = prefix + RelationalAnnotationNames.TableName;
            Schema = prefix + RelationalAnnotationNames.Schema;
            DefaultSchema = prefix + RelationalAnnotationNames.DefaultSchema;
            Name = prefix + RelationalAnnotationNames.Name;
            SequencePrefix = prefix + RelationalAnnotationNames.SequencePrefix;
            DiscriminatorProperty = prefix + RelationalAnnotationNames.DiscriminatorProperty;
            DiscriminatorValue = prefix + RelationalAnnotationNames.DiscriminatorValue;
        }

        public static RelationalFullAnnotationNames Instance { get; } = new RelationalFullAnnotationNames(RelationalAnnotationNames.Prefix);

        public readonly string ColumnName;
        public readonly string ColumnType;
        public readonly string GeneratedValueSql;
        public readonly string DefaultValue;
        public readonly string DatabaseName;
        public readonly string TableName;
        public readonly string Schema;
        public readonly string DefaultSchema;
        public readonly string Name;
        public readonly string SequencePrefix;
        public readonly string DiscriminatorProperty;
        public readonly string DiscriminatorValue;
    }
}
