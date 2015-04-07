// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Data.Common;
using System.Diagnostics;
using Microsoft.Data.Entity.Storage;

namespace Microsoft.Data.Entity.Relational
{
    public class RelationalObjectArrayValueReaderFactory : RelationalValueReaderFactory
    {
        public override IValueReader Create(DbDataReader dataReader)
        {
            Debug.Assert(dataReader != null); // hot path

            return new RelationalObjectArrayValueReader(dataReader);
        }
    }
}
