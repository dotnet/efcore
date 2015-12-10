// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Scaffolding.Metadata
{
    public class SqlServerColumnModelAnnotations
    {
        private readonly ColumnModel _column;

        public SqlServerColumnModelAnnotations([NotNull] ColumnModel column)
        {
            Check.NotNull(column, nameof(column));

            _column = column;
        }

        public virtual int? DateTimePrecision
        {
            get
            {
                return _column[SqlServerDatabaseModelAnnotationNames.DateTimePrecision] as int?;
            }
            [param: CanBeNull]
            set
            {
                _column[SqlServerDatabaseModelAnnotationNames.DateTimePrecision] = value;
            }
        }

        public virtual bool IsIdentity
        {
            get
            {
                var value = _column[SqlServerDatabaseModelAnnotationNames.IsIdentity];
                return value is bool ? (bool)value : false;
            }
            [param: NotNull]
            set
            {
                _column[SqlServerDatabaseModelAnnotationNames.IsIdentity] = value;
            }
        }
    }
}