// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace Microsoft.Data.Entity.Migrations.Infrastructure
{
    public class MigrationMetadataComparer : IComparer<IMigrationMetadata>
    {
        public readonly static MigrationMetadataComparer Instance = new MigrationMetadataComparer();

        private MigrationMetadataComparer()
        {
        }

        public int Compare(IMigrationMetadata x, IMigrationMetadata y)
        {
            var result = string.CompareOrdinal(x.Timestamp, y.Timestamp);

            if (result == 0)
            {
                result = string.CompareOrdinal(x.Name, y.Name);
            }

            return result;
        }
    }
}
