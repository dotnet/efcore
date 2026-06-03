// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using ColumnInfo = Microsoft.EntityFrameworkCore.SqlServer.Query.Internal.SqlServerOpenJsonExpression.ColumnInfo;

namespace Microsoft.EntityFrameworkCore.SqlServer.Query.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class SqlServerSqlTreePruner : SqlTreePruner
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression VisitExtension(Expression node)
    {
        switch (node)
        {
            case SqlServerOpenJsonExpression { ColumnInfos: IReadOnlyList<ColumnInfo> columnInfos } openJson:
                var visitedJson = (SqlExpression)Visit(openJson.JsonExpression);

                if (ReferencedColumnMap.TryGetValue(openJson.Alias, out var referencedAliases))
                {
                    List<ColumnInfo>? newColumnInfos = null;

                    for (var i = 0; i < columnInfos.Count; i++)
                    {
                        if (referencedAliases.Contains(columnInfos[i].Name))
                        {
                            newColumnInfos?.Add(columnInfos[i]);
                        }
                        else if (newColumnInfos is null)
                        {
                            newColumnInfos = [];
                            for (var j = 0; j < i; j++)
                            {
                                newColumnInfos.Add(columnInfos[j]);
                            }
                        }
                    }

                    // Not that if we pruned everything, the WITH clause gets removed entirely
                    return openJson.Update(visitedJson, openJson.Path, newColumnInfos ?? openJson.ColumnInfos);
                }

                // There are no references to the OPENJSON expression; remove the WITH clause entirely
                return openJson.Update(visitedJson, openJson.Path);

            default:
                return base.VisitExtension(node);
        }
    }
}
