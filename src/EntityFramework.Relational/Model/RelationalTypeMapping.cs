// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Data;
using System.Data.Common;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Relational.Update;
using Microsoft.Data.Entity.Relational.Utilities;

namespace Microsoft.Data.Entity.Relational.Model
{
    public class RelationalTypeMapping
    {
        private readonly string _storeTypeName;
        private readonly DbType _storeType;

        public RelationalTypeMapping([NotNull] string storeTypeName, DbType storeType)
        {
            Check.NotEmpty(storeTypeName, "storeTypeName");

            _storeTypeName = storeTypeName;
            _storeType = storeType;
        }

        public virtual string StoreTypeName
        {
            get { return _storeTypeName; }
        }

        public virtual DbType StoreType
        {
            get { return _storeType; }
        }

        public virtual DbParameter CreateParameter([NotNull] DbCommand command, [NotNull] ColumnModification columnModification)
        {
            Check.NotNull(command, "command");
            Check.NotNull(columnModification, "columnModification");

            var parameter = command.CreateParameter();
            parameter.Direction = ParameterDirection.Input;
            parameter.ParameterName = columnModification.ParameterName;

            ConfigureParameter(parameter, columnModification);

            return parameter;
        }

        protected virtual void ConfigureParameter([NotNull] DbParameter parameter, [NotNull] ColumnModification columnModification)
        {
            Check.NotNull(parameter, "parameter");
            Check.NotNull(columnModification, "columnModification");

            parameter.DbType = _storeType;
            parameter.IsNullable = columnModification.Property.IsNullable;
            parameter.Value = columnModification.StateEntry[columnModification.Property] ?? DBNull.Value;
        }
    }
}
