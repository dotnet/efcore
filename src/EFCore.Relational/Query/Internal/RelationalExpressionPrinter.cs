// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Microsoft.EntityFrameworkCore.Query.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class RelationalExpressionPrinter : ExpressionPrinter
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public RelationalExpressionPrinter()
            : base(new List<ConstantPrinterBase>
            {
                new CommandBuilderPrinter(),
                new EntityTrackingInfoListPrinter(),
                new MetadataPropertyCollectionPrinter()
            })
        {
        }

        private class CommandBuilderPrinter : ConstantPrinterBase
        {
            public override bool TryPrintConstant(
                ConstantExpression constantExpression,
                IndentedStringBuilder stringBuilder,
                bool removeFormatting)
            {
                if (constantExpression.Value is ShaperCommandContext shaperCommandContext)
                {
                    var appendAction = !removeFormatting ? AppendLine : Append;

                    appendAction(stringBuilder, "SelectExpression: ");
                    stringBuilder.IncrementIndent();

                    var querySqlGenerator = shaperCommandContext.QuerySqlGeneratorFactory();
                    var sql = querySqlGenerator.GenerateSql(new Dictionary<string, object>()).CommandText;

                    var lines = sql.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
                    foreach (var line in lines)
                    {
                        appendAction(stringBuilder, removeFormatting ? " " + line.TrimStart(' ') : line);
                    }

                    stringBuilder.DecrementIndent();

                    return true;
                }

                return false;
            }
        }

        private class EntityTrackingInfoListPrinter : ConstantPrinterBase
        {
            public override bool TryPrintConstant(
                ConstantExpression constantExpression,
                IndentedStringBuilder stringBuilder,
                bool removeFormatting)
            {
                if (constantExpression.Value is List<EntityTrackingInfo> trackingInfoList)
                {
                    var appendAction = trackingInfoList.Count > 2 && !removeFormatting ? AppendLine : Append;

                    appendAction(stringBuilder, "{ ");
                    stringBuilder.IncrementIndent();

                    for (var i = 0; i < trackingInfoList.Count; i++)
                    {
                        var entityTrackingInfo = trackingInfoList[i];
                        var separator = i == trackingInfoList.Count - 1 ? " " : ", ";
                        stringBuilder.Append("itemType: " + entityTrackingInfo.QuerySource.ItemType.Name);
                        appendAction(stringBuilder, separator);
                    }

                    stringBuilder.DecrementIndent();
                    appendAction(stringBuilder, "}");

                    return true;
                }

                return false;
            }
        }

        private class MetadataPropertyCollectionPrinter : ConstantPrinterBase
        {
            public override bool TryPrintConstant(
                ConstantExpression constantExpression,
                IndentedStringBuilder stringBuilder,
                bool removeFormatting)
            {
                if (constantExpression.Value is IEnumerable<IPropertyBase> properties)
                {
                    var propertiesList = properties.ToList();
                    var appendAction = propertiesList.Count > 2 && !removeFormatting ? AppendLine : Append;

                    appendAction(stringBuilder, constantExpression.Value.GetType().ShortDisplayName() + " ");
                    appendAction(stringBuilder, "{ ");

                    stringBuilder.IncrementIndent();
                    foreach (var property in propertiesList)
                    {
                        appendAction(stringBuilder, property.DeclaringType.ClrType.Name + "." + property.Name + ", ");
                    }

                    stringBuilder.DecrementIndent();
                    stringBuilder.Append("}");

                    return true;
                }

                return false;
            }
        }
    }
}
