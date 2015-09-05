// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Data.Common;
using System.Diagnostics;
using JetBrains.Annotations;

namespace Microsoft.Data.Entity.Storage.Internal
{
    public class UntypedRelationalValueBufferFactory : IRelationalValueBufferFactory
    {
        private readonly Action<object[]> _processValuesAction;

        public UntypedRelationalValueBufferFactory([CanBeNull] Action<object[]> processValuesAction)
        {
            _processValuesAction = processValuesAction;
        }

        public virtual ValueBuffer Create(DbDataReader dataReader)
        {
            Debug.Assert(dataReader != null); // hot path

            var fieldCount = dataReader.FieldCount;

            if (fieldCount == 0)
            {
                return ValueBuffer.Empty;
            }

            var values = new object[fieldCount];

            dataReader.GetValues(values);

            _processValuesAction?.Invoke(values);

            for (var i = 0; i < fieldCount; i++)
            {
                if (ReferenceEquals(values[i], DBNull.Value))
                {
                    values[i] = null;
                }
            }

            return new ValueBuffer(values);
        }
    }
}
