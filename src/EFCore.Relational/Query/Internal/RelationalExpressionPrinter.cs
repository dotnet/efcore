// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Microsoft.EntityFrameworkCore.Query.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public class RelationalExpressionPrinter : ExpressionPrinter
    {
        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public RelationalExpressionPrinter()
        {
            ConstantPrinters.InsertRange(
                0,
                new List<ConstantPrinterBase>
                {
                    new MetadataPropertyCollectionPrinter(),
                });
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
