// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Metadata;

namespace Microsoft.Data.Entity.Query.Internal
{
    public class RelationalExpressionPrinter : ExpressionPrinter
    {
        protected override List<IConstantPrinter> GetConstantPrinters()
        {
            var relationalPrinters = new List<IConstantPrinter>
            {
                new CommandBuilderPrinter(),
                new EntityTrackingInfoListPrinter(),
                new MetadataPropertyCollectionPrinter()
            };

            return relationalPrinters.Concat(base.GetConstantPrinters()).ToList();
        }

        private class CommandBuilderPrinter : IConstantPrinter
        {
            public bool TryPrintConstant(object value, IndentedStringBuilder stringBuilder)
            {
                var commandBuilder = value as CommandBuilder;
                if (commandBuilder != null)
                {
                    stringBuilder.AppendLine("SelectExpression: ");
                    stringBuilder.IncrementIndent();

                    var commandGenerator = commandBuilder.SqlGeneratorFunc();
                    var sql = commandGenerator.GenerateSql(new Dictionary<string, object>()).CommandText;

                    var lines = sql.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
                    foreach (var line in lines)
                    {
                        stringBuilder.AppendLine(line);
                    }

                    stringBuilder.DecrementIndent();

                    return true;
                }

                return false;
            }
        }

        private class EntityTrackingInfoListPrinter : IConstantPrinter
        {
            public bool TryPrintConstant(object value, IndentedStringBuilder stringBuilder)
            {
                var trackingInfoList = value as List<EntityTrackingInfo>;
                if (trackingInfoList != null)
                {
                    var appendAction = trackingInfoList.Count() > 2 ? AppendLine : Append;

                    appendAction(stringBuilder, "{ ");
                    stringBuilder.IncrementIndent();

                    for (int i = 0; i < trackingInfoList.Count; i++)
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

        private class MetadataPropertyCollectionPrinter : IConstantPrinter
        {
            public bool TryPrintConstant(object value, IndentedStringBuilder stringBuilder)
            {
                var properties = value as IEnumerable<IPropertyBase>;
                if (properties != null)
                {
                    var appendAction = properties.Count() > 2 ? AppendLine : Append;

                    appendAction(stringBuilder, value.GetType().DisplayName(fullName: false) + " ");
                    appendAction(stringBuilder, "{ ");

                    stringBuilder.IncrementIndent();
                    foreach (var property in properties)
                    {
                        appendAction(stringBuilder, property.DeclaringEntityType.ClrType.Name + "." + property.Name + ", ");
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
