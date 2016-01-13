// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Scaffolding.Metadata
{
    public class SqlServerDatabaseModelAnnotations
    {
        private readonly DatabaseModel _databaseModel;

        public SqlServerDatabaseModelAnnotations([NotNull] DatabaseModel databaseModel)
        {
            Check.NotNull(databaseModel, nameof(databaseModel));

            _databaseModel = databaseModel;
        }

        public virtual Dictionary<string, string> TypeAliases
        {
            get
            {
                return _databaseModel[SqlServerDatabaseModelAnnotationNames.TypeAliases] as Dictionary<string, string>;
            }
            [param: NotNull]
            set { _databaseModel[SqlServerDatabaseModelAnnotationNames.TypeAliases] = value; }
        }
    }
}
