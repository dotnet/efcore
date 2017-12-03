// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.EntityFrameworkCore.Query.ExpressionTranslators.Internal
{
    internal static class SqliteDateTimeHelper
    {
        public const string SqliteCalcDay = "60 * 60 * 24";
        public const string SqliteCalcHour = "60 * 60";
        public const string SqliteCalcMonth = "60 * 60 * 24 * 366/12";
        public const string SqliteCalcMinute = "60";
        public const string SqliteCalcSecond = "1";
        public const string SqliteCalcYear = "60 * 60 * 24 * 366";
        public const string SqliteDateAdd = "'{0} {1}'";
        public const string SqliteFormatDate = "'%Y-%m-%d %H:%M:%S'";
        public const string SqliteFormatDateFractional = "'%Y-%m-%d %H:%M:%S:%f'";
        public const string SqliteFunctionDateFormat = "strftime"; 
        public const string SqliteFractionalSeconds = "'%f'";
        public const string SqliteLocalTime = "'localtime'";
        public const string SqliteNow = "'now'";
        public const string SqliteStartOfDay = "'start of day'";
    }
}
