// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Query.ExpressionTranslators;

namespace Microsoft.EntityFrameworkCore.Sqlite.Query.ExpressionTranslators.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class SqliteNetTopologySuiteMemberTranslatorPlugin : IMemberTranslatorPlugin
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IEnumerable<IMemberTranslator> Translators { get; }
            = new IMemberTranslator[]
            {
                new SqliteCurveMemberTranslator(),
                new SqliteGeometryMemberTranslator(),
                new SqliteGeometryCollectionMemberTranslator(),
                new SqliteLineStringMemberTranslator(),
                new SqliteMultiCurveMemberTranslator(),
                new SqlitePointMemberTranslator(),
                new SqlitePolygonMemberTranslator()
            };
    }
}
