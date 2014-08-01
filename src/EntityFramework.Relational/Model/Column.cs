// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics.Contracts;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Relational.Utilities;

namespace Microsoft.Data.Entity.Relational.Model
{
    public class Column
    {
        private Table _table;
        private bool _isNullable = true;

        public Column([NotNull] string name, [NotNull] string dataType)
            : this(name, null, Check.NotEmpty(dataType, "dataType"))
        {
        }

        public Column([CanBeNull] string name, [CanBeNull] Type clrType)
            : this(name, Check.NotNull(clrType, "clrType"), null)
        {
        }

        public Column([CanBeNull] string name, [CanBeNull] Type clrType, [CanBeNull] string dataType)
        {
            // TODO: Replace assert with exception.
            Contract.Assert((clrType != null) || !string.IsNullOrEmpty(dataType));

            Name = name;
            ClrType = clrType;
            DataType = dataType;
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

        public virtual string Name { get; [param: CanBeNull] set; }

        public virtual Type ClrType { get; [param: CanBeNull] set; }

        public virtual string DataType { get; [param: CanBeNull] set; }

        public virtual bool IsNullable
        {
            get { return _isNullable; }
            set { _isNullable = value; }
        }

        public virtual object DefaultValue { get; [param: CanBeNull] set; }

        public virtual string DefaultSql { get; [param: CanBeNull] set; }
        public virtual ValueGenerationOnSave ValueGenerationStrategy { get; set; }

        public virtual bool HasDefault
        {
            get { return DefaultValue != null || DefaultSql != null; }
        }

        public virtual bool IsTimestamp { get; set; }

        // TODO: Consider adding a DataType abstraction.

        public virtual int? MaxLength { get; set; }

        public virtual byte? Precision { get; set; }

        public virtual byte? Scale { get; set; }

        public virtual bool? IsFixedLength { get; set; }

        public virtual bool? IsUnicode { get; set; }
    }
}
