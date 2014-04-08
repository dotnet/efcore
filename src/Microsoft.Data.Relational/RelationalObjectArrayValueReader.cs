// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Data.Common;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Relational.Utilities;

namespace Microsoft.Data.Relational
{
    public class RelationalObjectArrayValueReader : ObjectArrayValueReader
    {
        public RelationalObjectArrayValueReader([NotNull] DbDataReader dataReader)
            : base(Check.NotNull(CreateBuffer(dataReader), "dataReader"))
        {
        }

        private static object[] CreateBuffer(DbDataReader dataReader)
        {
            var values = new object[dataReader.FieldCount];

            dataReader.GetValues(values);

            for (var i = 0; i < values.Length; i++)
            {
                if (ReferenceEquals(values[i], DBNull.Value))
                {
                    values[i] = null;
                }
            }

            return values;
        }
    }
}
