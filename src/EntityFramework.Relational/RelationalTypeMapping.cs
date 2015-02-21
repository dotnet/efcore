// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Data;
using System.Data.Common;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Relational.Update;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Relational
{
    public class RelationalTypeMapping
    {
        private readonly DbType _storeType;

        /// <summary>
        ///     This constructor is intended only for use when creating test doubles that will override members
        ///     with mocked or faked behavior. Use of this constructor for other purposes may result in unexpected
        ///     behavior including but not limited to throwing <see cref="NullReferenceException" />.
        /// </summary>
        protected RelationalTypeMapping()
        {
        }

        public RelationalTypeMapping([NotNull] string storeTypeName, DbType storeType)
        {
            Check.NotEmpty(storeTypeName, nameof(storeTypeName));

            StoreTypeName = storeTypeName;
            _storeType = storeType;
        }

        public virtual string StoreTypeName { get; }

        public virtual DbType StoreType
        {
            get { return _storeType; }
        }

        public virtual DbParameter CreateParameter([NotNull] DbCommand command, [NotNull] ColumnModification columnModification, bool useOriginalValue)
        {
            Check.NotNull(command, nameof(command));
            Check.NotNull(columnModification, nameof(columnModification));

            var parameter = command.CreateParameter();
            parameter.Direction = ParameterDirection.Input;
            if (useOriginalValue)
            {
                Check.NotNull(columnModification.OriginalParameterName, "columnModification", "OriginalParameterName");
                parameter.ParameterName = columnModification.OriginalParameterName;
                parameter.Value = columnModification.OriginalValue ?? DBNull.Value;
            }
            else
            {
                Check.NotNull(columnModification.ParameterName, "columnModification", "ParameterName");
                parameter.ParameterName = columnModification.ParameterName;
                parameter.Value = columnModification.Value ?? DBNull.Value;
            }

            ConfigureParameter(parameter, columnModification);

            return parameter;
        }

        protected virtual void ConfigureParameter([NotNull] DbParameter parameter, [NotNull] ColumnModification columnModification)
        {
            Check.NotNull(parameter, nameof(parameter));
            Check.NotNull(columnModification, nameof(columnModification));

            parameter.DbType = _storeType;
            parameter.IsNullable = columnModification.Property.IsNullable;
        }
    }
}
