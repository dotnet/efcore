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
    public class SqlServerIndexModelAnnotations
    {
        private readonly IndexModel _index;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public SqlServerIndexModelAnnotations([NotNull] IndexModel index)
        {
            Check.NotNull(index, nameof(index));

            _index = index;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual bool IsClustered
        {
            get
            {
                var value = _index[SqlServerDatabaseModelAnnotationNames.IsClustered];
                return value is bool && (bool)value;
            }
            [param: NotNull] set { _index[SqlServerDatabaseModelAnnotationNames.IsClustered] = value; }
        }
    }
}
