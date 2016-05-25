// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Data;
using System.Data.Common;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Storage
{
    public class RelationalTypeMapping
    {
        public static readonly RelationalTypeMapping NullMapping = new RelationalTypeMapping("NULL");

        public RelationalTypeMapping(
            [NotNull] string storeType,
            [NotNull] Type clrType)
            : this(storeType, clrType, dbType: null)
        {
        }

        public RelationalTypeMapping(
            [NotNull] string storeType,
            [NotNull] Type clrType,
            [CanBeNull] DbType? dbType)
            : this(storeType, clrType, dbType, unicode: false, size: null)
        {
        }

        public RelationalTypeMapping(
            [NotNull] string storeType,
            [NotNull] Type clrType,
            [CanBeNull] DbType? dbType,
            bool unicode,
            int? size,
            bool hasNonDefaultUnicode = false,
            bool hasNonDefaultSize = false)
            : this(storeType)
        {
            Check.NotNull(clrType, nameof(clrType));

            ClrType = clrType;
            DbType = dbType;
            IsUnicode = unicode;
            Size = size;
            HasNonDefaultUnicode = hasNonDefaultUnicode;
            HasNonDefaultSize = hasNonDefaultSize;
        }

        private RelationalTypeMapping([NotNull] string storeType)
        {
            Check.NotEmpty(storeType, nameof(storeType));

            StoreType = storeType;
        }

        public virtual RelationalTypeMapping CreateCopy([NotNull] string storeType, int? size)
            => new RelationalTypeMapping(
                storeType, 
                ClrType, 
                DbType, 
                IsUnicode, 
                size, 
                HasNonDefaultUnicode, 
                hasNonDefaultSize: size != Size);

        public virtual string StoreType { get; }

        public virtual Type ClrType { get; }

        public virtual DbType? DbType { get; }

        public virtual bool IsUnicode { get; }

        public virtual int? Size { get; }

        public virtual bool HasNonDefaultUnicode { get; }

        public virtual bool HasNonDefaultSize { get; }

        public virtual DbParameter CreateParameter(
            [NotNull] DbCommand command,
            [NotNull] string name,
            [CanBeNull] object value,
            bool? nullable = null)
        {
            Check.NotNull(command, nameof(command));

            var parameter = command.CreateParameter();
            parameter.Direction = ParameterDirection.Input;
            parameter.ParameterName = name;
            parameter.Value = value ?? DBNull.Value;

            if (nullable.HasValue)
            {
                parameter.IsNullable = nullable.Value;
            }

            if (DbType.HasValue)
            {
                parameter.DbType = DbType.Value;
            }

            if (Size.HasValue)
            {
                parameter.Size = Size.Value;
            }

            ConfigureParameter(parameter);

            return parameter;
        }

        protected virtual void ConfigureParameter([NotNull] DbParameter parameter)
        {
        }
    }
}
