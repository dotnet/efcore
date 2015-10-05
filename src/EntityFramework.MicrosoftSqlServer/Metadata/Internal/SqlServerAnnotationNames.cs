// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Metadata.Internal
{
    public static class SqlServerAnnotationNames
    {
        public const string Prefix = "SqlServer:";
        public const string Clustered = "Clustered";
        public const string ValueGenerationStrategy = "ValueGenerationStrategy";
        public const string HiLoSequenceName = "HiLoSequenceName";
        public const string HiLoSequenceSchema = "HiLoSequenceSchema";

        public const string DefaultHiLoSequenceName = "EntityFrameworkHiLoSequence";
    }
}
