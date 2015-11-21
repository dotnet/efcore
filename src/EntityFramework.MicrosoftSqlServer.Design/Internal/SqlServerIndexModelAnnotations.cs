// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Scaffolding.Metadata;

namespace Microsoft.Data.Entity.Scaffolding.Internal
{
    public class SqlServerIndexModelAnnotations
    {
        private readonly IndexModel _indexModel;

        public SqlServerIndexModelAnnotations([NotNull] IndexModel indexModel)
        {
            _indexModel = indexModel;
        }

        public virtual bool IsClustered
        {
            get { return (bool)_indexModel[SqlServerDatabaseModelAnnotationNames.IndexIsClustered]; }
            set { _indexModel[SqlServerDatabaseModelAnnotationNames.IndexIsClustered] = value; }
        }
    }
}
