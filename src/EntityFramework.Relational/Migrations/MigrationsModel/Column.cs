// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Relational.Migrations.MigrationsModel
{
    // TODO: Consider adding more validation.
    // Issue #767
    // has to be duplicated in the relational model
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class Column
    {
        private bool _isNullable = true;

        public Column([CanBeNull] string name, [NotNull] Type clrType)
        {
            Check.NotNull(clrType, "clrType");

            Name = name;
            ClrType = clrType;
        }

        public virtual string Name { get; [param: NotNull] set; }

        public virtual Type ClrType { get; [param: NotNull] set; }

        public virtual string DataType { get; [param: CanBeNull] set; }

        public virtual bool IsNullable
        {
            get { return _isNullable; }
            set { _isNullable = value; }
        }

        public virtual object DefaultValue { get; [param: CanBeNull] set; }

        public virtual string DefaultSql { get; [param: CanBeNull] set; }

        public virtual bool IsIdentity { get; set; }

        public virtual bool IsComputed { get; set; }
        
        public virtual bool IsTimestamp { get; set; }

        // TODO: Consider adding a DataType abstraction.

        public virtual int? MaxLength { get; set; }

        public virtual byte? Precision { get; set; }

        public virtual byte? Scale { get; set; }

        public virtual bool? IsFixedLength { get; set; }

        public virtual bool? IsUnicode { get; set; }

        [UsedImplicitly]
        private string DebuggerDisplay
        {
            get { return string.Format("{0}", Name); }
        }
    }
}
