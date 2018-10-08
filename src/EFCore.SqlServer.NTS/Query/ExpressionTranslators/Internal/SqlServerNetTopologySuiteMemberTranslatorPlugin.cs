// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Query.ExpressionTranslators;

namespace Microsoft.EntityFrameworkCore.SqlServer.Query.ExpressionTranslators.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class SqlServerNetTopologySuiteMemberTranslatorPlugin : IMemberTranslatorPlugin
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IEnumerable<IMemberTranslator> Translators { get; }
            = new IMemberTranslator[]
            {
                new SqlServerCurveMemberTranslator(),
                new SqlServerGeometryMemberTranslator(),
                new SqlServerGeometryCollectionMemberTranslator(),
                new SqlServerLineStringMemberTranslator(),
                new SqlServerMultiCurveMemberTranslator(),
                new SqlServerPointMemberTranslator(),
                new SqlServerPolygonMemberTranslator()
            };
    }
}
