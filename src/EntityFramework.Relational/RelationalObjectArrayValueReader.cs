// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Data.Common;
using System.Diagnostics;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Storage;

namespace Microsoft.Data.Entity.Relational
{
    public class RelationalObjectArrayValueReader : IValueReader
    {
        private readonly object[] _values;

        public RelationalObjectArrayValueReader([NotNull] DbDataReader dataReader)
        {
            Debug.Assert(dataReader != null); // hot path

            _values = new object[dataReader.FieldCount];

            dataReader.GetValues(_values);
        }

        public virtual bool IsNull(int index)
        {
            Debug.Assert(index >= 0 && index < Count);

            return ReferenceEquals(_values[index], DBNull.Value);
        }

        public virtual T ReadValue<T>(int index)
        {
            Debug.Assert(index >= 0 && index < Count);

            return (T)_values[index];
        }

        public virtual int Count => _values.Length;
    }
}
