// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Data.Common;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Relational.Utilities;

namespace Microsoft.Data.Relational
{
    public class RelationalObjectArrayValueReaderFactory : RelationalValueReaderFactory
    {
        public override IValueReader Create(DbDataReader dataReader)
        {
            Check.NotNull(dataReader, "dataReader");

            return new RelationalObjectArrayValueReader(dataReader);
        }
    }
}
