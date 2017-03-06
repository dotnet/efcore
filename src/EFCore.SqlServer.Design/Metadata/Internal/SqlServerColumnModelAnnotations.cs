// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Scaffolding.Metadata.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class SqlServerColumnModelAnnotations
    {
        private readonly ColumnModel _column;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public SqlServerColumnModelAnnotations([NotNull] ColumnModel column)
        {
            Check.NotNull(column, nameof(column));

            _column = column;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual int? DateTimePrecision
        {
            get { return _column[SqlServerDatabaseModelAnnotationNames.DateTimePrecision] as int?; }
            [param: CanBeNull] set { _column[SqlServerDatabaseModelAnnotationNames.DateTimePrecision] = value; }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual bool IsIdentity
        {
            get
            {
                var value = _column[SqlServerDatabaseModelAnnotationNames.IsIdentity];
                return value is bool && (bool)value;
            }
            [param: NotNull] set { _column[SqlServerDatabaseModelAnnotationNames.IsIdentity] = value; }
        }
    }
}
