// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Scaffolding.Metadata.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class SqlServerDatabaseModelAnnotations
    {
        private readonly DatabaseModel _databaseModel;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public SqlServerDatabaseModelAnnotations([NotNull] DatabaseModel databaseModel)
        {
            Check.NotNull(databaseModel, nameof(databaseModel));

            _databaseModel = databaseModel;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual Dictionary<string, string> TypeAliases
        {
            get { return _databaseModel[SqlServerDatabaseModelAnnotationNames.TypeAliases] as Dictionary<string, string>; }
            [param: NotNull] set { _databaseModel[SqlServerDatabaseModelAnnotationNames.TypeAliases] = value; }
        }
    }
}
