// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Scaffolding.Metadata
{
    public static class SqlServerDatabaseModelAnnotationNames
    {
        public const string Prefix = "SqlServerDatabaseModel:";
        public const string IsIdentity = Prefix + "IsIdentity";
        public const string IsClustered = Prefix + "IsClustered";
        public const string DateTimePrecision = Prefix + "DateTimePrecision";
    }
}
