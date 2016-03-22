// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.EntityFrameworkCore.Metadata.Internal
{
    public class SqlServerFullAnnotationNames : RelationalFullAnnotationNames
    {
        protected SqlServerFullAnnotationNames(string prefix)
            : base(prefix)
        {
            Clustered = prefix + SqlServerAnnotationNames.Clustered;
            ValueGenerationStrategy = prefix + SqlServerAnnotationNames.ValueGenerationStrategy;
            HiLoSequenceName = prefix + SqlServerAnnotationNames.HiLoSequenceName;
            HiLoSequenceSchema = prefix + SqlServerAnnotationNames.HiLoSequenceSchema;
        }

        public new static SqlServerFullAnnotationNames Instance { get; } = new SqlServerFullAnnotationNames(SqlServerAnnotationNames.Prefix);

        public readonly string Clustered;
        public readonly string ValueGenerationStrategy;
        public readonly string HiLoSequenceName;
        public readonly string HiLoSequenceSchema;
    }
}
