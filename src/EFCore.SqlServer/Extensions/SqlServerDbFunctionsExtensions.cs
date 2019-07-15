// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.SqlServer.Internal;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore
{
    /// <summary>
    ///     Provides CLR methods that get translated to database functions when used in LINQ to Entities queries.
    ///     The methods on this class are accessed via <see cref="EF.Functions" />.
    /// </summary>
    public static class SqlServerDbFunctionsExtensions
    {
        /// <summary>
        ///     <para>
        ///         A DbFunction method stub that can be used in LINQ queries to target the SQL Server FREETEXT store function.
        ///     </para>
        /// </summary>
        /// <remarks>
        ///     This DbFunction method has no in-memory implementation and will throw if the query switches to client-evaluation.
        ///     This can happen if the query contains one or more expressions that could not be translated to the store.
        /// </remarks>
        /// <param name="_">DbFunctions instance</param>
        /// <param name="propertyReference">The property on which the search will be performed.</param>
        /// <param name="freeText">The text that will be searched for in the property.</param>
        /// <param name="languageTerm">A Language ID from the sys.syslanguages table.</param>
        public static bool FreeText(
            [CanBeNull] this DbFunctions _,
            [NotNull] string propertyReference,
            [NotNull] string freeText,
            int languageTerm)
            => FreeTextCore(propertyReference, freeText, languageTerm);

        /// <summary>
        ///     <para>
        ///         A DbFunction method stub that can be used in LINQ queries to target the SQL Server FREETEXT store function.
        ///     </para>
        /// </summary>
        /// <remarks>
        ///     This DbFunction method has no in-memory implementation and will throw if the query switches to client-evaluation.
        ///     This can happen if the query contains one or more expressions that could not be translated to the store.
        /// </remarks>
        /// <param name="_">DbFunctions instance</param>
        /// <param name="propertyReference">The property on which the search will be performed.</param>
        /// <param name="freeText">The text that will be searched for in the property.</param>
        public static bool FreeText(
            [CanBeNull] this DbFunctions _,
            [NotNull] string propertyReference,
            [NotNull] string freeText)
            => FreeTextCore(propertyReference, freeText, null);

        private static bool FreeTextCore(string propertyName, string freeText, int? languageTerm)
        {
            throw new InvalidOperationException(SqlServerStrings.FunctionOnClient(nameof(FreeText)));
        }

        /// <summary>
        ///     <para>
        ///         A DbFunction method stub that can be used in LINQ queries to target the SQL Server CONTAINS store function.
        ///     </para>
        /// </summary>
        /// <remarks>
        ///     This DbFunction method has no in-memory implementation and will throw if the query switches to client-evaluation.
        ///     This can happen if the query contains one or more expressions that could not be translated to the store.
        /// </remarks>
        /// <param name="_">DbFunctions instance</param>
        /// <param name="propertyReference">The property on which the search will be performed.</param>
        /// <param name="searchCondition">The text that will be searched for in the property and the condition for a match.</param>
        /// <param name="languageTerm">A Language ID from the sys.syslanguages table.</param>
        public static bool Contains(
            [CanBeNull] this DbFunctions _,
            [NotNull] string propertyReference,
            [NotNull] string searchCondition,
            int languageTerm)
            => ContainsCore(propertyReference, searchCondition, languageTerm);

        /// <summary>
        ///     <para>
        ///         A DbFunction method stub that can be used in LINQ queries to target the SQL Server CONTAINS store function.
        ///     </para>
        /// </summary>
        /// <remarks>
        ///     This DbFunction method has no in-memory implementation and will throw if the query switches to client-evaluation.
        ///     This can happen if the query contains one or more expressions that could not be translated to the store.
        /// </remarks>
        /// <param name="_">DbFunctions instance</param>
        /// <param name="propertyReference">The property on which the search will be performed.</param>
        /// <param name="searchCondition">The text that will be searched for in the property and the condition for a match.</param>
        public static bool Contains(
            [CanBeNull] this DbFunctions _,
            [NotNull] string propertyReference,
            [NotNull] string searchCondition)
            => ContainsCore(propertyReference, searchCondition, null);

        private static bool ContainsCore(string propertyName, string searchCondition, int? languageTerm)
        {
            throw new InvalidOperationException(SqlServerStrings.FunctionOnClient(nameof(Contains)));
        }

        /// <summary>
        ///     Counts the number of year boundaries crossed between the startDate and endDate.
        ///     Corresponds to SQL Server's DATEDIFF(YEAR,startDate,endDate).
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="startDate">Starting date for the calculation.</param>
        /// <param name="endDate">Ending date for the calculation.</param>
        /// <returns>Number of year boundaries crossed between the dates.</returns>
        public static int DateDiffYear(
            [CanBeNull] this DbFunctions _,
            DateTime startDate,
            DateTime endDate)
            => endDate.Year - startDate.Year;

        /// <summary>
        ///     Counts the number of year boundaries crossed between the startDate and endDate.
        ///     Corresponds to SQL Server's DATEDIFF(YEAR,startDate,endDate).
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="startDate">Starting date for the calculation.</param>
        /// <param name="endDate">Ending date for the calculation.</param>
        /// <returns>Number of year boundaries crossed between the dates.</returns>
        public static int? DateDiffYear(
            [CanBeNull] this DbFunctions _,
            DateTime? startDate,
            DateTime? endDate)
            => (startDate.HasValue && endDate.HasValue)
                ? (int?)DateDiffYear(_, startDate.Value, endDate.Value)
                : null;

        /// <summary>
        ///     Counts the number of year boundaries crossed between the startDate and endDate.
        ///     Corresponds to SQL Server's DATEDIFF(YEAR,startDate,endDate).
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="startDate">Starting date for the calculation.</param>
        /// <param name="endDate">Ending date for the calculation.</param>
        /// <returns>Number of year boundaries crossed between the dates.</returns>
        public static int DateDiffYear(
            [CanBeNull] this DbFunctions _,
            DateTimeOffset startDate,
            DateTimeOffset endDate)
            => DateDiffYear(_, startDate.UtcDateTime, endDate.UtcDateTime);

        /// <summary>
        ///     Counts the number of year boundaries crossed between the startDate and endDate.
        ///     Corresponds to SQL Server's DATEDIFF(YEAR,startDate,endDate).
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="startDate">Starting date for the calculation.</param>
        /// <param name="endDate">Ending date for the calculation.</param>
        /// <returns>Number of year boundaries crossed between the dates.</returns>
        public static int? DateDiffYear(
            [CanBeNull] this DbFunctions _,
            DateTimeOffset? startDate,
            DateTimeOffset? endDate)
            => (startDate.HasValue && endDate.HasValue)
                ? (int?)DateDiffYear(_, startDate.Value, endDate.Value)
                : null;

        /// <summary>
        ///     Counts the number of month boundaries crossed between the startDate and endDate.
        ///     Corresponds to SQL Server's DATEDIFF(MONTH,startDate,endDate).
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="startDate">Starting date for the calculation.</param>
        /// <param name="endDate">Ending date for the calculation.</param>
        /// <returns>Number of month boundaries crossed between the dates.</returns>
        public static int DateDiffMonth(
            [CanBeNull] this DbFunctions _,
            DateTime startDate,
            DateTime endDate)
            => 12 * (endDate.Year - startDate.Year) + endDate.Month - startDate.Month;

        /// <summary>
        ///     Counts the number of month boundaries crossed between the startDate and endDate.
        ///     Corresponds to SQL Server's DATEDIFF(MONTH,startDate,endDate).
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="startDate">Starting date for the calculation.</param>
        /// <param name="endDate">Ending date for the calculation.</param>
        /// <returns>Number of month boundaries crossed between the dates.</returns>
        public static int? DateDiffMonth(
            [CanBeNull] this DbFunctions _,
            DateTime? startDate,
            DateTime? endDate)
            => (startDate.HasValue && endDate.HasValue)
                ? (int?)DateDiffMonth(_, startDate.Value, endDate.Value)
                : null;

        /// <summary>
        ///     Counts the number of month boundaries crossed between the startDate and endDate.
        ///     Corresponds to SQL Server's DATEDIFF(MONTH,startDate,endDate).
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="startDate">Starting date for the calculation.</param>
        /// <param name="endDate">Ending date for the calculation.</param>
        /// <returns>Number of month boundaries crossed between the dates.</returns>
        public static int DateDiffMonth(
            [CanBeNull] this DbFunctions _,
            DateTimeOffset startDate,
            DateTimeOffset endDate)
            => DateDiffMonth(_, startDate.UtcDateTime, endDate.UtcDateTime);

        /// <summary>
        ///     Counts the number of month boundaries crossed between the startDate and endDate.
        ///     Corresponds to SQL Server's DATEDIFF(MONTH,startDate,endDate).
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="startDate">Starting date for the calculation.</param>
        /// <param name="endDate">Ending date for the calculation.</param>
        /// <returns>Number of month boundaries crossed between the dates.</returns>
        public static int? DateDiffMonth(
            [CanBeNull] this DbFunctions _,
            DateTimeOffset? startDate,
            DateTimeOffset? endDate)
            => (startDate.HasValue && endDate.HasValue)
                ? (int?)DateDiffMonth(_, startDate.Value, endDate.Value)
                : null;

        /// <summary>
        ///     Counts the number of day boundaries crossed between the startDate and endDate.
        ///     Corresponds to SQL Server's DATEDIFF(DAY,startDate,endDate).
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="startDate">Starting date for the calculation.</param>
        /// <param name="endDate">Ending date for the calculation.</param>
        /// <returns>Number of day boundaries crossed between the dates.</returns>
        public static int DateDiffDay(
            [CanBeNull] this DbFunctions _,
            DateTime startDate,
            DateTime endDate)
            => (endDate.Date - startDate.Date).Days;

        /// <summary>
        ///     Counts the number of day boundaries crossed between the startDate and endDate.
        ///     Corresponds to SQL Server's DATEDIFF(DAY,startDate,endDate).
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="startDate">Starting date for the calculation.</param>
        /// <param name="endDate">Ending date for the calculation.</param>
        /// <returns>Number of day boundaries crossed between the dates.</returns>
        public static int? DateDiffDay(
            [CanBeNull] this DbFunctions _,
            DateTime? startDate,
            DateTime? endDate)
            => (startDate.HasValue && endDate.HasValue)
                ? (int?)DateDiffDay(_, startDate.Value, endDate.Value)
                : null;

        /// <summary>
        ///     Counts the number of day boundaries crossed between the startDate and endDate.
        ///     Corresponds to SQL Server's DATEDIFF(DAY,startDate,endDate).
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="startDate">Starting date for the calculation.</param>
        /// <param name="endDate">Ending date for the calculation.</param>
        /// <returns>Number of day boundaries crossed between the dates.</returns>
        public static int DateDiffDay(
            [CanBeNull] this DbFunctions _,
            DateTimeOffset startDate,
            DateTimeOffset endDate)
            => DateDiffDay(_, startDate.UtcDateTime, endDate.UtcDateTime);

        /// <summary>
        ///     Counts the number of day boundaries crossed between the startDate and endDate.
        ///     Corresponds to SQL Server's DATEDIFF(DAY,startDate,endDate).
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="startDate">Starting date for the calculation.</param>
        /// <param name="endDate">Ending date for the calculation.</param>
        /// <returns>Number of day boundaries crossed between the dates.</returns>
        public static int? DateDiffDay(
            [CanBeNull] this DbFunctions _,
            DateTimeOffset? startDate,
            DateTimeOffset? endDate)
            => (startDate.HasValue && endDate.HasValue)
                ? (int?)DateDiffDay(_, startDate.Value, endDate.Value)
                : null;

        /// <summary>
        ///     Counts the number of hour boundaries crossed between the startDate and endDate.
        ///     Corresponds to SQL Server's DATEDIFF(HOUR,startDate,endDate).
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="startDate">Starting date for the calculation.</param>
        /// <param name="endDate">Ending date for the calculation.</param>
        /// <returns>Number of hour boundaries crossed between the dates.</returns>
        public static int DateDiffHour(
            [CanBeNull] this DbFunctions _,
            DateTime startDate,
            DateTime endDate)
        {
            checked
            {
                return DateDiffDay(_, startDate, endDate) * 24 + endDate.Hour - startDate.Hour;
            }
        }

        /// <summary>
        ///     Counts the number of hour boundaries crossed between the startDate and endDate.
        ///     Corresponds to SQL Server's DATEDIFF(HOUR,startDate,endDate).
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="startDate">Starting date for the calculation.</param>
        /// <param name="endDate">Ending date for the calculation.</param>
        /// <returns>Number of hour boundaries crossed between the dates.</returns>
        public static int? DateDiffHour(
            [CanBeNull] this DbFunctions _,
            DateTime? startDate,
            DateTime? endDate)
            => (startDate.HasValue && endDate.HasValue)
                ? (int?)DateDiffHour(_, startDate.Value, endDate.Value)
                : null;

        /// <summary>
        ///     Counts the number of hour boundaries crossed between the startDate and endDate.
        ///     Corresponds to SQL Server's DATEDIFF(HOUR,startDate,endDate).
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="startDate">Starting date for the calculation.</param>
        /// <param name="endDate">Ending date for the calculation.</param>
        /// <returns>Number of hour boundaries crossed between the dates.</returns>
        public static int DateDiffHour(
            [CanBeNull] this DbFunctions _,
            DateTimeOffset startDate,
            DateTimeOffset endDate)
            => DateDiffHour(_, startDate.UtcDateTime, endDate.UtcDateTime);

        /// <summary>
        ///     Counts the number of hour boundaries crossed between the startDate and endDate.
        ///     Corresponds to SQL Server's DATEDIFF(HOUR,startDate,endDate).
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="startDate">Starting date for the calculation.</param>
        /// <param name="endDate">Ending date for the calculation.</param>
        /// <returns>Number of hour boundaries crossed between the dates.</returns>
        public static int? DateDiffHour(
            [CanBeNull] this DbFunctions _,
            DateTimeOffset? startDate,
            DateTimeOffset? endDate)
            => (startDate.HasValue && endDate.HasValue)
                ? (int?)DateDiffHour(_, startDate.Value, endDate.Value)
                : null;

        /// <summary>
        ///     Counts the number of hour boundaries crossed between the startTimeSpan and endTimeSpan.
        ///     Corresponds to SQL Server's DATEDIFF(HOUR,startDate,endDate).
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="startTimeSpan">Starting timespan for the calculation.</param>
        /// <param name="endTimeSpan">Ending timespan for the calculation.</param>
        /// <returns>Number of hour boundaries crossed between the timespans.</returns>
        public static int DateDiffHour(
            [CanBeNull] this DbFunctions _,
            TimeSpan startTimeSpan,
            TimeSpan endTimeSpan)
        {
            checked
            {
                return endTimeSpan.Hours - startTimeSpan.Hours;
            }
        }

        /// <summary>
        ///     Counts the number of hour boundaries crossed between the startTimeSpan and endTimeSpan.
        ///     Corresponds to SQL Server's DATEDIFF(HOUR,startDate,endDate).
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="startTimeSpan">Starting timespan for the calculation.</param>
        /// <param name="endTimeSpan">Ending timespan for the calculation.</param>
        /// <returns>Number of hour boundaries crossed between the timespans.</returns>
        public static int? DateDiffHour(
            [CanBeNull] this DbFunctions _,
            TimeSpan? startTimeSpan,
            TimeSpan? endTimeSpan)
            => (startTimeSpan.HasValue && endTimeSpan.HasValue)
                ? (int?)DateDiffHour(_, startTimeSpan.Value, endTimeSpan.Value)
                : null;

        /// <summary>
        ///     Counts the number of minute boundaries crossed between the startDate and endDate.
        ///     Corresponds to SQL Server's DATEDIFF(MINUTE,startDate,endDate).
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="startDate">Starting date for the calculation.</param>
        /// <param name="endDate">Ending date for the calculation.</param>
        /// <returns>Number of minute boundaries crossed between the dates.</returns>
        public static int DateDiffMinute(
            [CanBeNull] this DbFunctions _,
            DateTime startDate,
            DateTime endDate)
        {
            checked
            {
                return DateDiffHour(_, startDate, endDate) * 60 + endDate.Minute - startDate.Minute;
            }
        }

        /// <summary>
        ///     Counts the number of minute boundaries crossed between the startDate and endDate.
        ///     Corresponds to SQL Server's DATEDIFF(MINUTE,startDate,endDate).
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="startDate">Starting date for the calculation.</param>
        /// <param name="endDate">Ending date for the calculation.</param>
        /// <returns>Number of minute boundaries crossed between the dates.</returns>
        public static int? DateDiffMinute(
            [CanBeNull] this DbFunctions _,
            DateTime? startDate,
            DateTime? endDate)
            => (startDate.HasValue && endDate.HasValue)
                ? (int?)DateDiffMinute(_, startDate.Value, endDate.Value)
                : null;

        /// <summary>
        ///     Counts the number of minute boundaries crossed between the startDate and endDate.
        ///     Corresponds to SQL Server's DATEDIFF(MINUTE,startDate,endDate).
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="startDate">Starting date for the calculation.</param>
        /// <param name="endDate">Ending date for the calculation.</param>
        /// <returns>Number of minute boundaries crossed between the dates.</returns>
        public static int DateDiffMinute(
            [CanBeNull] this DbFunctions _,
            DateTimeOffset startDate,
            DateTimeOffset endDate)
            => DateDiffMinute(_, startDate.UtcDateTime, endDate.UtcDateTime);

        /// <summary>
        ///     Counts the number of minute boundaries crossed between the startDate and endDate.
        ///     Corresponds to SQL Server's DATEDIFF(MINUTE,startDate,endDate).
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="startDate">Starting date for the calculation.</param>
        /// <param name="endDate">Ending date for the calculation.</param>
        /// <returns>Number of minute boundaries crossed between the dates.</returns>
        public static int? DateDiffMinute(
            [CanBeNull] this DbFunctions _,
            DateTimeOffset? startDate,
            DateTimeOffset? endDate)
            => (startDate.HasValue && endDate.HasValue)
                ? (int?)DateDiffMinute(_, startDate.Value, endDate.Value)
                : null;

        /// <summary>
        ///     Counts the number of minute boundaries crossed between the startTimeSpan and endTimeSpan.
        ///     Corresponds to SQL Server's DATEDIFF(MINUTE,startDate,endDate).
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="startTimeSpan">Starting timespan for the calculation.</param>
        /// <param name="endTimeSpan">Ending timespan for the calculation.</param>
        /// <returns>Number of minute boundaries crossed between the timespans.</returns>
        public static int DateDiffMinute(
            [CanBeNull] this DbFunctions _,
            TimeSpan startTimeSpan,
            TimeSpan endTimeSpan)
        {
            checked
            {
                return DateDiffHour(_, startTimeSpan, endTimeSpan) * 60 + endTimeSpan.Minutes - startTimeSpan.Minutes;
            }
        }

        /// <summary>
        ///     Counts the number of minute boundaries crossed between the startTimeSpan and endTimeSpan.
        ///     Corresponds to SQL Server's DATEDIFF(MINUTE,startDate,endDate).
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="startTimeSpan">Starting timespan for the calculation.</param>
        /// <param name="endTimeSpan">Ending timespan for the calculation.</param>
        /// <returns>Number of minute boundaries crossed between the timespans.</returns>
        public static int? DateDiffMinute(
            [CanBeNull] this DbFunctions _,
            TimeSpan? startTimeSpan,
            TimeSpan? endTimeSpan)
            => (startTimeSpan.HasValue && endTimeSpan.HasValue)
                ? (int?)DateDiffMinute(_, startTimeSpan.Value, endTimeSpan.Value)
                : null;

        /// <summary>
        ///     Counts the number of second boundaries crossed between the startDate and endDate.
        ///     Corresponds to SQL Server's DATEDIFF(SECOND,startDate,endDate).
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="startDate">Starting date for the calculation.</param>
        /// <param name="endDate">Ending date for the calculation.</param>
        /// <returns>Number of second boundaries crossed between the dates.</returns>
        public static int DateDiffSecond(
            [CanBeNull] this DbFunctions _,
            DateTime startDate,
            DateTime endDate)
        {
            checked
            {
                return DateDiffMinute(_, startDate, endDate) * 60 + endDate.Second - startDate.Second;
            }
        }

        /// <summary>
        ///     Counts the number of second boundaries crossed between the startDate and endDate.
        ///     Corresponds to SQL Server's DATEDIFF(SECOND,startDate,endDate).
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="startDate">Starting date for the calculation.</param>
        /// <param name="endDate">Ending date for the calculation.</param>
        /// <returns>Number of second boundaries crossed between the dates.</returns>
        public static int? DateDiffSecond(
            [CanBeNull] this DbFunctions _,
            DateTime? startDate,
            DateTime? endDate)
            => (startDate.HasValue && endDate.HasValue)
                ? (int?)DateDiffSecond(_, startDate.Value, endDate.Value)
                : null;

        /// <summary>
        ///     Counts the number of second boundaries crossed between the startDate and endDate.
        ///     Corresponds to SQL Server's DATEDIFF(SECOND,startDate,endDate).
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="startDate">Starting date for the calculation.</param>
        /// <param name="endDate">Ending date for the calculation.</param>
        /// <returns>Number of second boundaries crossed between the dates.</returns>
        public static int DateDiffSecond(
            [CanBeNull] this DbFunctions _,
            DateTimeOffset startDate,
            DateTimeOffset endDate)
            => DateDiffSecond(_, startDate.UtcDateTime, endDate.UtcDateTime);

        /// <summary>
        ///     Counts the number of second boundaries crossed between the startDate and endDate.
        ///     Corresponds to SQL Server's DATEDIFF(SECOND,startDate,endDate).
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="startDate">Starting date for the calculation.</param>
        /// <param name="endDate">Ending date for the calculation.</param>
        /// <returns>Number of second boundaries crossed between the dates.</returns>
        public static int? DateDiffSecond(
            [CanBeNull] this DbFunctions _,
            DateTimeOffset? startDate,
            DateTimeOffset? endDate)
            => (startDate.HasValue && endDate.HasValue)
                ? (int?)DateDiffSecond(_, startDate.Value, endDate.Value)
                : null;

        /// <summary>
        ///     Counts the number of second boundaries crossed between the startTimeSpan and endTimeSpan.
        ///     Corresponds to SQL Server's DATEDIFF(SECOND,startDate,endDate).
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="startTimeSpan">Starting timespan for the calculation.</param>
        /// <param name="endTimeSpan">Ending timespan for the calculation.</param>
        /// <returns>Number of second boundaries crossed between the timespans.</returns>
        public static int DateDiffSecond(
            [CanBeNull] this DbFunctions _,
            TimeSpan startTimeSpan,
            TimeSpan endTimeSpan)
        {
            checked
            {
                return DateDiffMinute(_, startTimeSpan, endTimeSpan) * 60 + endTimeSpan.Seconds - startTimeSpan.Seconds;
            }
        }

        /// <summary>
        ///     Counts the number of second boundaries crossed between the startTimeSpan and endTimeSpan.
        ///     Corresponds to SQL Server's DATEDIFF(SECOND,startDate,endDate).
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="startTimeSpan">Starting timespan for the calculation.</param>
        /// <param name="endTimeSpan">Ending timespan for the calculation.</param>
        /// <returns>Number of second boundaries crossed between the timespans.</returns>
        public static int? DateDiffSecond(
            [CanBeNull] this DbFunctions _,
            TimeSpan? startTimeSpan,
            TimeSpan? endTimeSpan)
            => (startTimeSpan.HasValue && endTimeSpan.HasValue)
                ? (int?)DateDiffSecond(_, startTimeSpan.Value, endTimeSpan.Value)
                : null;

        /// <summary>
        ///     Counts the number of millisecond boundaries crossed between the startDate and endDate.
        ///     Corresponds to SQL Server's DATEDIFF(MILLISECOND,startDate,endDate).
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="startDate">Starting date for the calculation.</param>
        /// <param name="endDate">Ending date for the calculation.</param>
        /// <returns>Number of millisecond boundaries crossed between the dates.</returns>
        public static int DateDiffMillisecond(
            [CanBeNull] this DbFunctions _,
            DateTime startDate,
            DateTime endDate)
        {
            checked
            {
                return DateDiffSecond(_, startDate, endDate) * 1000 + endDate.Millisecond - startDate.Millisecond;
            }
        }

        /// <summary>
        ///     Counts the number of millisecond boundaries crossed between the startDate and endDate.
        ///     Corresponds to SQL Server's DATEDIFF(MILLISECOND,startDate,endDate).
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="startDate">Starting date for the calculation.</param>
        /// <param name="endDate">Ending date for the calculation.</param>
        /// <returns>Number of millisecond boundaries crossed between the dates.</returns>
        public static int? DateDiffMillisecond(
            [CanBeNull] this DbFunctions _,
            DateTime? startDate,
            DateTime? endDate)
            => (startDate.HasValue && endDate.HasValue)
                ? (int?)DateDiffMillisecond(_, startDate.Value, endDate.Value)
                : null;

        /// <summary>
        ///     Counts the number of millisecond boundaries crossed between the startDate and endDate.
        ///     Corresponds to SQL Server's DATEDIFF(MILLISECOND,startDate,endDate).
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="startDate">Starting date for the calculation.</param>
        /// <param name="endDate">Ending date for the calculation.</param>
        /// <returns>Number of millisecond boundaries crossed between the dates.</returns>
        public static int DateDiffMillisecond(
            [CanBeNull] this DbFunctions _,
            DateTimeOffset startDate,
            DateTimeOffset endDate)
            => DateDiffMillisecond(_, startDate.UtcDateTime, endDate.UtcDateTime);

        /// <summary>
        ///     Counts the number of millisecond boundaries crossed between the startDate and endDate.
        ///     Corresponds to SQL Server's DATEDIFF(MILLISECOND,startDate,endDate).
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="startDate">Starting date for the calculation.</param>
        /// <param name="endDate">Ending date for the calculation.</param>
        /// <returns>Number of millisecond boundaries crossed between the dates.</returns>
        public static int? DateDiffMillisecond(
            [CanBeNull] this DbFunctions _,
            DateTimeOffset? startDate,
            DateTimeOffset? endDate)
            => (startDate.HasValue && endDate.HasValue)
                ? (int?)DateDiffMillisecond(_, startDate.Value, endDate.Value)
                : null;

        /// <summary>
        ///     Counts the number of millisecond boundaries crossed between the startTimeSpan and endTimeSpan.
        ///     Corresponds to SQL Server's DATEDIFF(MILLISECOND,startDate,endDate).
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="startTimeSpan">Starting timespan for the calculation.</param>
        /// <param name="endTimeSpan">Ending timespan for the calculation.</param>
        /// <returns>Number of millisecond boundaries crossed between the timespans.</returns>
        public static int DateDiffMillisecond(
            [CanBeNull] this DbFunctions _,
            TimeSpan startTimeSpan,
            TimeSpan endTimeSpan)
        {
            checked
            {
                return DateDiffSecond(_, startTimeSpan, endTimeSpan) * 1000 + endTimeSpan.Milliseconds - startTimeSpan.Milliseconds;
            }
        }

        /// <summary>
        ///     Counts the number of millisecond boundaries crossed between the startTimeSpan and endTimeSpan.
        ///     Corresponds to SQL Server's DATEDIFF(MILLISECOND,startDate,endDate).
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="startTimeSpan">Starting timespan for the calculation.</param>
        /// <param name="endTimeSpan">Ending timespan for the calculation.</param>
        /// <returns>Number of millisecond boundaries crossed between the timespans.</returns>
        public static int? DateDiffMillisecond(
            [CanBeNull] this DbFunctions _,
            TimeSpan? startTimeSpan,
            TimeSpan? endTimeSpan)
            => (startTimeSpan.HasValue && endTimeSpan.HasValue)
                ? (int?)DateDiffMillisecond(_, startTimeSpan.Value, endTimeSpan.Value)
                : null;

        /// <summary>
        ///     Counts the number of microsecond boundaries crossed between the startDate and endDate.
        ///     Corresponds to SQL Server's DATEDIFF(MICROSECOND,startDate,endDate).
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="startDate">Starting date for the calculation.</param>
        /// <param name="endDate">Ending date for the calculation.</param>
        /// <returns>Number of microsecond boundaries crossed between the dates.</returns>
        public static int DateDiffMicrosecond(
            [CanBeNull] this DbFunctions _,
            DateTime startDate,
            DateTime endDate)
        {
            checked
            {
                return (int)((endDate.Ticks - startDate.Ticks) / 10);
            }
        }

        /// <summary>
        ///     Counts the number of microsecond boundaries crossed between the startDate and endDate.
        ///     Corresponds to SQL Server's DATEDIFF(MICROSECOND,startDate,endDate).
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="startDate">Starting date for the calculation.</param>
        /// <param name="endDate">Ending date for the calculation.</param>
        /// <returns>Number of microsecond boundaries crossed between the dates.</returns>
        public static int? DateDiffMicrosecond(
            [CanBeNull] this DbFunctions _,
            DateTime? startDate,
            DateTime? endDate)
            => (startDate.HasValue && endDate.HasValue)
                ? (int?)DateDiffMicrosecond(_, startDate.Value, endDate.Value)
                : null;

        /// <summary>
        ///     Counts the number of microsecond boundaries crossed between the startDate and endDate.
        ///     Corresponds to SQL Server's DATEDIFF(MICROSECOND,startDate,endDate).
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="startDate">Starting date for the calculation.</param>
        /// <param name="endDate">Ending date for the calculation.</param>
        /// <returns>Number of microsecond boundaries crossed between the dates.</returns>
        public static int DateDiffMicrosecond(
            [CanBeNull] this DbFunctions _,
            DateTimeOffset startDate,
            DateTimeOffset endDate)
            => DateDiffMicrosecond(_, startDate.UtcDateTime, endDate.UtcDateTime);

        /// <summary>
        ///     Counts the number of microsecond boundaries crossed between the startDate and endDate.
        ///     Corresponds to SQL Server's DATEDIFF(MICROSECOND,startDate,endDate).
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="startDate">Starting date for the calculation.</param>
        /// <param name="endDate">Ending date for the calculation.</param>
        /// <returns>Number of microsecond boundaries crossed between the dates.</returns>
        public static int? DateDiffMicrosecond(
            [CanBeNull] this DbFunctions _,
            DateTimeOffset? startDate,
            DateTimeOffset? endDate)
            => (startDate.HasValue && endDate.HasValue)
                ? (int?)DateDiffMicrosecond(_, startDate.Value, endDate.Value)
                : null;

        /// <summary>
        ///     Counts the number of microsecond boundaries crossed between the startTimeSpan and endTimeSpan.
        ///     Corresponds to SQL Server's DATEDIFF(MICROSECOND,startDate,endDate).
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="startTimeSpan">Starting timespan for the calculation.</param>
        /// <param name="endTimeSpan">Ending timespan for the calculation.</param>
        /// <returns>Number of microsecond boundaries crossed between the timespans.</returns>
        public static int DateDiffMicrosecond(
            [CanBeNull] this DbFunctions _,
            TimeSpan startTimeSpan,
            TimeSpan endTimeSpan)
        {
            checked
            {
                return (int)((endTimeSpan.Ticks - startTimeSpan.Ticks) / 10);
            }
        }

        /// <summary>
        ///     Counts the number of microsecond boundaries crossed between the startTimeSpan and endTimeSpan.
        ///     Corresponds to SQL Server's DATEDIFF(MICROSECOND,startDate,endDate).
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="startTimeSpan">Starting timespan for the calculation.</param>
        /// <param name="endTimeSpan">Ending timespan for the calculation.</param>
        /// <returns>Number of microsecond boundaries crossed between the timespans.</returns>
        public static int? DateDiffMicrosecond(
            [CanBeNull] this DbFunctions _,
            TimeSpan? startTimeSpan,
            TimeSpan? endTimeSpan)
            => (startTimeSpan.HasValue && endTimeSpan.HasValue)
                ? (int?)DateDiffMicrosecond(_, startTimeSpan.Value, endTimeSpan.Value)
                : null;

        /// <summary>
        ///     Counts the number of nanosecond boundaries crossed between the startDate and endDate.
        ///     Corresponds to SQL Server's DATEDIFF(NANOSECOND,startDate,endDate).
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="startDate">Starting date for the calculation.</param>
        /// <param name="endDate">Ending date for the calculation.</param>
        /// <returns>Number of nanosecond boundaries crossed between the dates.</returns>
        public static int DateDiffNanosecond(
            [CanBeNull] this DbFunctions _,
            DateTime startDate,
            DateTime endDate)
        {
            checked
            {
                return (int)((endDate.Ticks - startDate.Ticks) * 100);
            }
        }

        /// <summary>
        ///     Counts the number of nanosecond boundaries crossed between the startDate and endDate.
        ///     Corresponds to SQL Server's DATEDIFF(NANOSECOND,startDate,endDate).
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="startDate">Starting date for the calculation.</param>
        /// <param name="endDate">Ending date for the calculation.</param>
        /// <returns>Number of nanosecond boundaries crossed between the dates.</returns>
        public static int? DateDiffNanosecond(
            [CanBeNull] this DbFunctions _,
            DateTime? startDate,
            DateTime? endDate)
            => (startDate.HasValue && endDate.HasValue)
                ? (int?)DateDiffNanosecond(_, startDate.Value, endDate.Value)
                : null;

        /// <summary>
        ///     Counts the number of nanosecond boundaries crossed between the startDate and endDate.
        ///     Corresponds to SQL Server's DATEDIFF(NANOSECOND,startDate,endDate).
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="startDate">Starting date for the calculation.</param>
        /// <param name="endDate">Ending date for the calculation.</param>
        /// <returns>Number of nanosecond boundaries crossed between the dates.</returns>
        public static int DateDiffNanosecond(
            [CanBeNull] this DbFunctions _,
            DateTimeOffset startDate,
            DateTimeOffset endDate)
            => DateDiffNanosecond(_, startDate.UtcDateTime, endDate.UtcDateTime);

        /// <summary>
        ///     Counts the number of nanosecond boundaries crossed between the startDate and endDate.
        ///     Corresponds to SQL Server's DATEDIFF(NANOSECOND,startDate,endDate).
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="startDate">Starting date for the calculation.</param>
        /// <param name="endDate">Ending date for the calculation.</param>
        /// <returns>Number of nanosecond boundaries crossed between the dates.</returns>
        public static int? DateDiffNanosecond(
            [CanBeNull] this DbFunctions _,
            DateTimeOffset? startDate,
            DateTimeOffset? endDate)
            => (startDate.HasValue && endDate.HasValue)
                ? (int?)DateDiffNanosecond(_, startDate.Value, endDate.Value)
                : null;

        /// <summary>
        ///     Counts the number of nanosecond boundaries crossed between the startTimeSpan and endTimeSpan.
        ///     Corresponds to SQL Server's DATEDIFF(NANOSECOND,startDate,endDate).
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="startTimeSpan">Starting timespan for the calculation.</param>
        /// <param name="endTimeSpan">Ending timespan for the calculation.</param>
        /// <returns>Number of nanosecond boundaries crossed between the dates.</returns>
        public static int DateDiffNanosecond(
            [CanBeNull] this DbFunctions _,
            TimeSpan startTimeSpan,
            TimeSpan endTimeSpan)
        {
            checked
            {
                return (int)((endTimeSpan.Ticks - startTimeSpan.Ticks) * 100);
            }
        }

        /// <summary>
        ///     Counts the number of nanosecond boundaries crossed between the startTimeSpan and endTimeSpan.
        ///     Corresponds to SQL Server's DATEDIFF(NANOSECOND,startDate,endDate).
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="startTimeSpan">Starting timespan for the calculation.</param>
        /// <param name="endTimeSpan">Ending timespan for the calculation.</param>
        /// <returns>Number of nanosecond boundaries crossed between the dates.</returns>
        public static int? DateDiffNanosecond(
            [CanBeNull] this DbFunctions _,
            TimeSpan? startTimeSpan,
            TimeSpan? endTimeSpan)
            => (startTimeSpan.HasValue && endTimeSpan.HasValue)
                ? (int?)DateDiffNanosecond(_, startTimeSpan.Value, endTimeSpan.Value)
                : null;

        /// <summary>
        ///     Validate if the given string is a valid date.
        ///     Corresponds to the SQL Server's ISDATE('date').
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="expression">Expression to validate</param>
        /// <returns>true for valid date and false otherwise.</returns>
        public static bool IsDate(
            [CanBeNull] this DbFunctions _,
            [NotNull] string expression)
            => throw new InvalidOperationException(SqlServerStrings.FunctionOnClient(nameof(IsDate)));
    }
}
