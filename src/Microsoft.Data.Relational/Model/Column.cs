// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Diagnostics.Contracts;
using JetBrains.Annotations;
using Microsoft.Data.Relational.Utilities;

namespace Microsoft.Data.Relational.Model
{
    public class Column
    {
        private Table _table;
        private readonly string _name;
        private readonly Type _clrType;
        private readonly string _dataType;

        public Column([NotNull] string name, [NotNull] string dataType)
            : this(name, null, Check.NotEmpty(dataType, "dataType"))
        {
        }

        public Column([NotNull] string name, [CanBeNull] Type clrType, [CanBeNull] string dataType)
        {
            Check.NotEmpty(name, "name");

            // TODO: Replace assert with exception.
            Contract.Assert((clrType != null) || !string.IsNullOrEmpty(dataType));

            _name = name;
            _clrType = clrType;
            _dataType = dataType;
        }

        public virtual Table Table
        {
            get { return _table; }

            [param: CanBeNull]
            internal set
            {
                Contract.Assert((value == null) != (_table == null));
                _table = value;
            }
        }

        public virtual string Name
        {
            get { return _name; }
        }

        public virtual Type ClrType
        {
            get { return _clrType; }
        }

        public virtual string DataType
        {
            get { return _dataType; }
        }

        public virtual bool IsNullable { get; set; }

        public virtual object DefaultValue { get; [param: CanBeNull] set; }

        public virtual string DefaultSql { get; [param: CanBeNull] set; }
    }
}
