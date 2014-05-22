// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;
using Microsoft.WindowsAzure.Storage.Table;

namespace Microsoft.Data.Entity.AzureTableStorage.Query
{
    public enum FilterComparisonOperator
    {
        Equal,
        GreaterThan,
        GreaterThanOrEqual,
        LessThan,
        LessThanOrEqual,
        NotEqual
    }

    internal static class FilterComparison
    {
        public static FilterComparisonOperator FromNodeType(ExpressionType type)
        {
            switch (type)
            {
                case ExpressionType.Equal:
                    return FilterComparisonOperator.Equal;
                case ExpressionType.GreaterThan:
                    return FilterComparisonOperator.GreaterThan;
                case ExpressionType.GreaterThanOrEqual:
                    return FilterComparisonOperator.GreaterThanOrEqual;
                case ExpressionType.LessThan:
                    return FilterComparisonOperator.LessThan;
                case ExpressionType.LessThanOrEqual:
                    return FilterComparisonOperator.LessThanOrEqual;
                case ExpressionType.NotEqual:
                    return FilterComparisonOperator.NotEqual;
                default:
                    throw new ArgumentOutOfRangeException("type", "Cannot match expression expression type");
            }
        }

        public static string ToString(FilterComparisonOperator type)
        {
            switch (type)
            {
                case FilterComparisonOperator.Equal:
                    return QueryComparisons.Equal;
                case FilterComparisonOperator.GreaterThan:
                    return QueryComparisons.GreaterThan;
                case FilterComparisonOperator.GreaterThanOrEqual:
                    return QueryComparisons.GreaterThanOrEqual;
                case FilterComparisonOperator.LessThan:
                    return QueryComparisons.LessThan;
                case FilterComparisonOperator.LessThanOrEqual:
                    return QueryComparisons.LessThanOrEqual;
                case FilterComparisonOperator.NotEqual:
                    return QueryComparisons.NotEqual;
                default:
                    throw new ArgumentOutOfRangeException("type");
            }
        }

        public static void FlipInequalities(ref FilterComparisonOperator type)
        {
            switch (type)
            {
                case FilterComparisonOperator.GreaterThan:
                    type = FilterComparisonOperator.LessThan;
                    break;
                case FilterComparisonOperator.GreaterThanOrEqual:
                    type = FilterComparisonOperator.LessThanOrEqual;
                    break;
                case FilterComparisonOperator.LessThan:
                    type = FilterComparisonOperator.GreaterThan;
                    break;
                case FilterComparisonOperator.LessThanOrEqual:
                    type = FilterComparisonOperator.GreaterThanOrEqual;
                    break;
            }
        }
    }
}
