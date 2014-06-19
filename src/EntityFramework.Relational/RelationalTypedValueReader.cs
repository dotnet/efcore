// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Data.Common;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Relational.Utilities;

namespace Microsoft.Data.Entity.Relational
{
    public class RelationalTypedValueReader : IValueReader
    {
        private readonly DbDataReader _dataReader;

        public RelationalTypedValueReader([NotNull] DbDataReader dataReader)
        {
            Check.NotNull(dataReader, "dataReader");

            _dataReader = dataReader;
        }

        public virtual bool IsNull(int index)
        {
            return _dataReader.IsDBNull(index);
        }

        public virtual T ReadValue<T>(int index)
        {
            return _dataReader.GetFieldValue<T>(index);
        }

        public virtual int Count
        {
            get { return _dataReader.FieldCount; }
        }
    }
}
