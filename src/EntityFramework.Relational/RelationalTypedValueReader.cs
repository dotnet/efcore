// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Data.Common;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Relational
{
    public class RelationalTypedValueReader : IValueReader
    {
        private readonly DbDataReader _dataReader;

        public RelationalTypedValueReader([NotNull] DbDataReader dataReader)
        {
            Check.NotNull(dataReader, nameof(dataReader));

            _dataReader = dataReader;
        }

        public virtual bool IsNull(int index) => _dataReader.IsDBNull(index);

        public virtual T ReadValue<T>(int index) => _dataReader.GetFieldValue<T>(index);

        public virtual int Count => _dataReader.FieldCount;
    }
}
