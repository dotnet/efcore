// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Data.Common;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Relational
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
