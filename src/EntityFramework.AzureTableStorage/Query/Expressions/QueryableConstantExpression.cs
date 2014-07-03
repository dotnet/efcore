// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Globalization;
using System.Linq.Expressions;
using System.Text;
using JetBrains.Annotations;
using Microsoft.Data.Entity.AzureTableStorage.Utilities;
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.Parsing;

namespace Microsoft.Data.Entity.AzureTableStorage.Query.Expressions
{
    public class QueryableConstantExpression : ExtensionExpression
    {
        private readonly dynamic _constant;

        public QueryableConstantExpression([NotNull] object constant)
            : base(Check.NotNull(constant, "constant").GetType())
        {
            IsStringProperty = false;
            _constant = constant;
        }

        public override Expression Accept([NotNull] ExpressionTreeVisitor visitor)
        {
            Check.NotNull(visitor, "visitor");

            var specificVisitor = visitor as TableQueryGenerator;
            if (specificVisitor != null)
            {
                return specificVisitor.VisitQueryableConstant(this);
            }
            return base.Accept(visitor);
        }

        protected override Expression VisitChildren(ExpressionTreeVisitor visitor)
        {
            return this;
        }

        public virtual string Value
        {
            get
            {
                if (IsStringProperty)
                {
                    Escape(_constant.ToString());
                }
                return Escape(_constant);
            }
        }

        public virtual bool IsStringProperty { get; set; }

        internal static string Escape(dynamic givenValue)
        {
            return EscapeQueryValue(givenValue);
        }

        #region Escape Query Values

        //TODO remove when/if https://github.com/Azure/azure-storage-net/pull/64 is merged
        internal static string EscapeQueryValue(string givenValue)
        {
            return string.Format(CultureInfo.InvariantCulture, "'{0}'", givenValue.Replace("'", "''"));
        }

        internal static string EscapeQueryValue(bool givenValue)
        {
            return givenValue ? "true" : "false";
        }

        internal static string EscapeQueryValue(int givenValue)
        {
            return System.Convert.ToString(givenValue, CultureInfo.InvariantCulture);
        }

        internal static string EscapeQueryValue(double givenValue)
        {
            return System.Convert.ToString(givenValue, CultureInfo.InvariantCulture);
        }

        internal static string EscapeQueryValue(long givenValue)
        {
            return string.Format(CultureInfo.InvariantCulture, "{0}L", System.Convert.ToString(givenValue, CultureInfo.InvariantCulture));
        }

        internal static string EscapeQueryValue(Guid givenValue)
        {
            Check.NotNull(givenValue, "givenValue");
            return string.Format(CultureInfo.InvariantCulture, "guid'{0}'", givenValue);
        }

        internal static string EscapeQueryValue(DateTimeOffset givenValue)
        {
            Check.NotNull(givenValue, "givenValue");
            return string.Format(CultureInfo.InvariantCulture, "datetime'{0}'",
                givenValue.UtcDateTime.ToString("o", CultureInfo.InvariantCulture));
        }

        internal static string EscapeQueryValue(
            byte[] givenValue)
        {
            Check.NotNull(givenValue, "givenValue");
            var sb = new StringBuilder();

            foreach (var b in givenValue)
            {
                sb.AppendFormat("{0:x2}", b);
            }
            return string.Format(CultureInfo.InvariantCulture, "X'{0}'", sb.ToString());
        }

        #endregion
    }
}
