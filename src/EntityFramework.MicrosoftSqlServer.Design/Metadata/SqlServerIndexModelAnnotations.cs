// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Scaffolding.Metadata
{
    public class SqlServerIndexModelAnnotations
    {
        private readonly IndexModel _index;

        public SqlServerIndexModelAnnotations([NotNull] IndexModel index)
        {
            Check.NotNull(index, nameof(index));

            _index = index;
        }

        public virtual bool IsClustered
        {
            get
            {
                var value = _index[SqlServerDatabaseModelAnnotationNames.IsClustered];
                return value is bool ? (bool)value : false;
            }
            [param: NotNull]
            set
            {
                _index[SqlServerDatabaseModelAnnotationNames.IsClustered] = value;
            }
        }
    }
}