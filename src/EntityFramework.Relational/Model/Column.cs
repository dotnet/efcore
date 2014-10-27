// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Relational.Utilities;

namespace Microsoft.Data.Entity.Relational.Model
{
    // TODO: Consider adding more validation.
    // TODO: Inheriting from MetadataBase to get annotations; it is unfortunate that all property information
    // Issue #767
    // has to be duplicated in the relational model
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class Column : MetadataBase
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
            Contract.Assert((clrType != null) || !string.IsNullOrEmpty(dataType));

            Name = name;
            ClrType = clrType;
            DataType = dataType;
        }

        public Column([NotNull] Column source)
        {
            Copy(source);
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

        public virtual bool GenerateValueOnAdd { get; set; }

        public virtual bool IsComputed { get; set; }
        
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

        public virtual void Copy([NotNull] Column source)
        {
            Check.NotNull(source, "source");

            Name = source.Name;
            ClrType = source.ClrType;
            DataType = source.DataType;
            IsNullable = source.IsNullable;
            DefaultValue = source.DefaultValue;
            DefaultSql = source.DefaultSql;
            GenerateValueOnAdd = source.GenerateValueOnAdd;
            IsComputed = source.IsComputed;
            IsTimestamp = source.IsTimestamp;
            MaxLength = source.MaxLength;
            Precision = source.Precision;
            Scale = source.Scale;
            IsFixedLength = source.IsFixedLength;
            IsUnicode = source.IsUnicode;

            foreach (var annotation in source.Annotations)
            {
                this[annotation.Name] = annotation.Value;
            }
        }

        public virtual Column Clone([NotNull] CloneContext cloneContext)
        {
            Check.NotNull(cloneContext, "cloneContext");

            return (Column)cloneContext.GetOrAdd(this, () => new Column(this));
        }

        [UsedImplicitly]
        private string DebuggerDisplay
        {
            get { return string.Format("{0}[{1}]", Table.Name, Name); }
        }
    }
}
