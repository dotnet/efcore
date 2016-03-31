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

        public RelationalTypeMapping([NotNull] string defaultTypeName, [NotNull] Type clrType, [CanBeNull] DbType? storeType, bool unicode = true)
            : this(defaultTypeName, clrType, unicode)
        {
            StoreType = storeType;
        }

        public RelationalTypeMapping([NotNull] string defaultTypeName, [NotNull] Type clrType, bool unicode = true)
            : this(defaultTypeName)
        {
            Check.NotNull(clrType, nameof(clrType));

            ClrType = clrType;
            IsUnicode = unicode;
        }

        private RelationalTypeMapping([NotNull] string defaultTypeName)
        {
            Check.NotEmpty(defaultTypeName, nameof(defaultTypeName));

            DefaultTypeName = defaultTypeName;
        }

        public virtual string DefaultTypeName { get; }

        public virtual DbType? StoreType { get; }

        public virtual Type ClrType { get; }

        public virtual bool IsUnicode { get; }

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

            if (StoreType.HasValue)
            {
                parameter.DbType = StoreType.Value;
            }

            ConfigureParameter(parameter);

            return parameter;
        }

        protected virtual void ConfigureParameter([NotNull] DbParameter parameter)
        {
        }
    }
}
