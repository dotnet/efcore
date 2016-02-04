// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query.ExpressionVisitors.Internal;

namespace Microsoft.EntityFrameworkCore.Query.Internal
{
    public class RelationalExpressionPrinter : ExpressionPrinter
    {
        protected override List<IConstantPrinter> GetConstantPrinters()
        {
            var relationalPrinters = new List<IConstantPrinter>
            {
                new CommandBuilderPrinter(),
                new ShaperPrinter(),
                new EntityTrackingInfoListPrinter(),
                new MetadataPropertyCollectionPrinter()
            };

            return relationalPrinters.Concat(base.GetConstantPrinters()).ToList();
        }

        private class CommandBuilderPrinter : IConstantPrinter
        {
            public bool TryPrintConstant(object value, IndentedStringBuilder stringBuilder)
            {
                var shaperCommandContext = value as ShaperCommandContext;
                if (shaperCommandContext != null)
                {
                    stringBuilder.AppendLine("SelectExpression: ");
                    stringBuilder.IncrementIndent();

                    var querySqlGenerator = shaperCommandContext.QuerySqlGeneratorFactory();
                    var sql = querySqlGenerator.GenerateSql(new Dictionary<string, object>()).CommandText;

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

        private class ShaperPrinter : IConstantPrinter
        {
            public bool TryPrintConstant(object value, IndentedStringBuilder stringBuilder)
            {
                if (value == null)
                {
                    return false;
                }

                var typeName = value.GetType().Name;
                if (typeName.StartsWith("CompositeShaper"))
                {
                    PrintCompositeShaper(value, stringBuilder);

                    return true;
                }
                else if (typeName.StartsWith("BufferedEntityShaper")
                    || typeName.StartsWith("BufferedOffsetEntityShaper")
                    || typeName == "EntityShaper")
                {
                    PrintEntityShaper(value, stringBuilder);

                    return true;
                }
                else if (typeName == "ValueBufferShaper")
                {
                    stringBuilder.AppendLine(typeName);

                    return true;
                }

                return false;
            }

            private void PrintCompositeShaper(object value, IndentedStringBuilder stringBuilder)
            {
                var shaperTypeInfo = value.GetType().GetTypeInfo();
                stringBuilder.AppendLine(value.GetType().DisplayName(fullName: false) + ": ");
                stringBuilder.IncrementIndent();

                var materializerStringFieldInfo = shaperTypeInfo.GetDeclaredField("_materializerString");
                var materializerString = (string)materializerStringFieldInfo.GetValue(value);

                var outerShaperFieldInfo = shaperTypeInfo.GetDeclaredField("_outerShaper");
                var outerShaper = outerShaperFieldInfo.GetValue(value);
                var innerShaperFieldInfo = shaperTypeInfo.GetDeclaredField("_innerShaper");
                var innerShaper = innerShaperFieldInfo.GetValue(value);

                stringBuilder.Append("outerShaper: ");
                TryPrintConstant(outerShaper, stringBuilder);

                stringBuilder.Append("innerShaper: ");
                TryPrintConstant(innerShaper, stringBuilder);

                stringBuilder.AppendLine("materializer: " + materializerString);

                stringBuilder.DecrementIndent();
            }

            private void PrintEntityShaper(object value, IndentedStringBuilder stringBuilder)
            {
                stringBuilder.AppendLine(value.GetType().DisplayName(fullName: false) + ": ");
                stringBuilder.IncrementIndent();

                var materializerStringPropertyInfo = typeof(EntityShaper).GetTypeInfo().GetDeclaredProperty("MaterializerString");
                var materializerString = (string)materializerStringPropertyInfo.GetValue(value);

                var valueBufferOffsetPropertyInfo = typeof(EntityShaper).GetTypeInfo().GetDeclaredProperty("ValueBufferOffset");
                var valueBufferOffset = (int)valueBufferOffsetPropertyInfo.GetValue(value);

                stringBuilder.AppendLine("materializer: ");
                stringBuilder.IncrementIndent();
                var lines = materializerString.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
                foreach (var line in lines)
                {
                    stringBuilder.AppendLine(line);
                }

                stringBuilder.DecrementIndent();

                if (valueBufferOffset > 0)
                {
                    stringBuilder.AppendLine("valueBufferOffset: " + valueBufferOffset);
                }

                stringBuilder.DecrementIndent();
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
