// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using Microsoft.Data.Entity.Storage;

namespace Microsoft.Data.Entity.Relational
{
    public class NonTypedValueReaderFactory : IRelationalValueReaderFactory
    {
        public virtual IValueReader CreateValueReader(DbDataReader dataReader, IEnumerable<Type> valueTypes, int offset)
        {
            Debug.Assert(dataReader != null); // hot path

            var values = new object[dataReader.FieldCount];

            dataReader.GetValues(values);

            return new RelationalObjectArrayValueReader(values, offset);
        }
    }
}
