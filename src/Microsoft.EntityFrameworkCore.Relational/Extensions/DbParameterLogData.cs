// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Data;
using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Storage
{
    public class DbParameterLogData
    {
        public DbParameterLogData(
            [NotNull] string name,
            [CanBeNull] object value,
            bool hasValue,
            ParameterDirection direction,
            DbType dbType,
            bool nullable,
            int size,
            byte precision,
            byte scale)
        {
            Name = name;
            Value = value;
            HasValue = hasValue;
            Direction = direction;
            DbType = dbType;
            IsNullable = nullable;
            Size = size;
            Precision = precision;
            Scale = scale;
        }

        public virtual string Name { get; }
        public virtual object Value { get; }
        public virtual bool HasValue { get; set; }
        public virtual ParameterDirection Direction { get; set; }
        public virtual DbType DbType { get; set; }
        public virtual bool IsNullable { get; }
        public virtual int Size { get; }
        public virtual byte Precision { get; }
        public virtual byte Scale { get; }
    }
}
