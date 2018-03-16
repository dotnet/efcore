// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.EntityFrameworkCore.Oracle.Metadata.Internal
{
    public static class OracleAnnotationNames
    {
        public const string Prefix = "Oracle:";

        public const string ValueGenerationStrategy = Prefix + "ValueGenerationStrategy";

        public const string HiLoSequenceName = Prefix + "HiLoSequenceName";
    }
}
