// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics.Contracts;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Relational;
using Microsoft.Data.Entity.Relational.Model;
using Microsoft.Data.Entity.SqlServer.Utilities;

namespace Microsoft.Data.Entity.SqlServer
{
    public class SqlServerDatabaseBuilder : DatabaseBuilder
    {
        protected override Sequence BuildSequence(IProperty property)
        {
            Check.NotNull(property, "property");

            var sequence = property.SqlServer().TryGetSequence();

            return sequence == null
                ? null
                : new Sequence(
                    new SchemaQualifiedName(sequence.Name, sequence.Schema),
                    GetSqlDataType(sequence.Type),
                    sequence.StartValue,
                    sequence.IncrementBy);
        }

        private static string GetSqlDataType(Type sequenceType)
        {
            Contract.Assert(sequenceType == typeof(long)
                            || sequenceType == typeof(int)
                            || sequenceType == typeof(short)
                            || sequenceType == typeof(byte));

            return sequenceType == typeof(long)
                ? "bigint"
                : sequenceType == typeof(int)
                    ? "int"
                    : sequenceType == typeof(short)
                        ? "smallint"
                        : "tinyint";
        }
    }
}
