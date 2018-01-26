// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Query.Internal;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class DocumentDbPrinter : ExpressionPrinter
    {
        public DocumentDbPrinter()
            : base(
                new List<ConstantPrinterBase>
                {
                    new CommandBuilderPrinter()
                })
        {
        }

        protected override Expression VisitExtension(Expression extensionExpression)
        {
            if (extensionExpression is ShapedQueryExpression shapedQueryExpression)
            {

            }



            return base.VisitExtension(extensionExpression);
        }

        private class CommandBuilderPrinter : ConstantPrinterBase
        {
            public override bool TryPrintConstant(
                ConstantExpression constantExpression,
                IndentedStringBuilder stringBuilder,
                bool removeFormatting)
            {
                if (constantExpression.Value is DocumentCommandContext documentCommandContext)
                {
                    var appendAction = !removeFormatting ? AppendLine : Append;

                    appendAction(stringBuilder, "SelectExpression: ");
                    stringBuilder.IncrementIndent();

                    var querySqlGenerator = documentCommandContext.GetSqlGenerator();
                    var sql = querySqlGenerator.GenerateSql();

                    var lines = sql.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
                    for (var i = 0; i < lines.Length; i++)
                    {
                        if (i == lines.Length - 1)
                        {
                            appendAction = Append;
                        }

                        appendAction(stringBuilder, removeFormatting ? " " + lines[i].TrimStart(' ') : lines[i]);
                    }

                    stringBuilder.DecrementIndent();

                    return true;
                }

                return false;
            }
        }
    }
}
