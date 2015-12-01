// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Scaffolding.Metadata;

namespace Microsoft.Data.Entity.Scaffolding.Internal
{
    public class SqlServerColumnModelAnnotations
    {
        private readonly ColumnModel _columnModel;

        public SqlServerColumnModelAnnotations([NotNull] ColumnModel columnModel)
        {
            _columnModel = columnModel;
        }

        public virtual bool IsIdentity
        {
            get { return (bool)_columnModel[SqlServerDatabaseModelAnnotationNames.ColumnIsIdentity]; }
             set { _columnModel[SqlServerDatabaseModelAnnotationNames.ColumnIsIdentity] = value; }
        }

        public virtual int? DateTimePrecision
        {
            get { return _columnModel[SqlServerDatabaseModelAnnotationNames.ColumnDateTimePrecision] as int?; }
            [param: CanBeNull] set { _columnModel[SqlServerDatabaseModelAnnotationNames.ColumnDateTimePrecision] = value; }
        }
    }
}
