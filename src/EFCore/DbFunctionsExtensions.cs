// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore
{
    /// <summary>
    ///     Provides CLR methods that get translated to database functions when used in LINQ to Entities queries.
    ///     The methods on this class are accessed via <see cref="EF.Functions" />.
    /// </summary>
    // ReSharper disable once InconsistentNaming
    public static class DbFunctionsExtensions
    {
        /// <summary>
        ///    Counts the number of year boundaries crossed between the startDate and endDate.
        ///    Corresponds to SQL Server's DATEDIFF(YEAR,startDate,endDate).
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
        ///    Counts the number of year boundaries crossed between the startDate and endDate.
        ///    Corresponds to SQL Server's DATEDIFF(YEAR,startDate,endDate).
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
        ///    Counts the number of year boundaries crossed between the startDate and endDate.
        ///    Corresponds to SQL Server's DATEDIFF(YEAR,startDate,endDate).
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
        ///    Counts the number of year boundaries crossed between the startDate and endDate.
        ///    Corresponds to SQL Server's DATEDIFF(YEAR,startDate,endDate).
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
        ///    Counts the number of month boundaries crossed between the startDate and endDate.
        ///    Corresponds to SQL Server's DATEDIFF(MONTH,startDate,endDate).
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
        ///    Counts the number of month boundaries crossed between the startDate and endDate.
        ///    Corresponds to SQL Server's DATEDIFF(MONTH,startDate,endDate).
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
        ///    Counts the number of month boundaries crossed between the startDate and endDate.
        ///    Corresponds to SQL Server's DATEDIFF(MONTH,startDate,endDate).
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
        ///    Counts the number of month boundaries crossed between the startDate and endDate.
        ///    Corresponds to SQL Server's DATEDIFF(MONTH,startDate,endDate).
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
        ///    Counts the number of day boundaries crossed between the startDate and endDate.
        ///    Corresponds to SQL Server's DATEDIFF(DAY,startDate,endDate).
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
        ///    Counts the number of day boundaries crossed between the startDate and endDate.
        ///    Corresponds to SQL Server's DATEDIFF(DAY,startDate,endDate).
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
        ///    Counts the number of day boundaries crossed between the startDate and endDate.
        ///    Corresponds to SQL Server's DATEDIFF(DAY,startDate,endDate).
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
        ///    Counts the number of day boundaries crossed between the startDate and endDate.
        ///    Corresponds to SQL Server's DATEDIFF(DAY,startDate,endDate).
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
        ///    Counts the number of hour boundaries crossed between the startDate and endDate.
        ///    Corresponds to SQL Server's DATEDIFF(HOUR,startDate,endDate).
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
        ///    Counts the number of hour boundaries crossed between the startDate and endDate.
        ///    Corresponds to SQL Server's DATEDIFF(HOUR,startDate,endDate).
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
        ///    Counts the number of hour boundaries crossed between the startDate and endDate.
        ///    Corresponds to SQL Server's DATEDIFF(HOUR,startDate,endDate).
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
        ///    Counts the number of hour boundaries crossed between the startDate and endDate.
        ///    Corresponds to SQL Server's DATEDIFF(HOUR,startDate,endDate).
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
        ///    Counts the number of minute boundaries crossed between the startDate and endDate.
        ///    Corresponds to SQL Server's DATEDIFF(MINUTE,startDate,endDate).
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
        ///    Counts the number of minute boundaries crossed between the startDate and endDate.
        ///    Corresponds to SQL Server's DATEDIFF(MINUTE,startDate,endDate).
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
        ///    Counts the number of minute boundaries crossed between the startDate and endDate.
        ///    Corresponds to SQL Server's DATEDIFF(MINUTE,startDate,endDate).
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
        ///    Counts the number of minute boundaries crossed between the startDate and endDate.
        ///    Corresponds to SQL Server's DATEDIFF(MINUTE,startDate,endDate).
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
        ///    Counts the number of second boundaries crossed between the startDate and endDate.
        ///    Corresponds to SQL Server's DATEDIFF(SECOND,startDate,endDate).
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
        ///    Counts the number of second boundaries crossed between the startDate and endDate.
        ///    Corresponds to SQL Server's DATEDIFF(SECOND,startDate,endDate).
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
        ///    Counts the number of second boundaries crossed between the startDate and endDate.
        ///    Corresponds to SQL Server's DATEDIFF(SECOND,startDate,endDate).
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
        ///    Counts the number of second boundaries crossed between the startDate and endDate.
        ///    Corresponds to SQL Server's DATEDIFF(SECOND,startDate,endDate).
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
        ///    Counts the number of millisecond boundaries crossed between the startDate and endDate.
        ///    Corresponds to SQL Server's DATEDIFF(MILLISECOND,startDate,endDate).
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
        ///    Counts the number of millisecond boundaries crossed between the startDate and endDate.
        ///    Corresponds to SQL Server's DATEDIFF(MILLISECOND,startDate,endDate).
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
        ///    Counts the number of millisecond boundaries crossed between the startDate and endDate.
        ///    Corresponds to SQL Server's DATEDIFF(MILLISECOND,startDate,endDate).
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
        ///    Counts the number of millisecond boundaries crossed between the startDate and endDate.
        ///    Corresponds to SQL Server's DATEDIFF(MILLISECOND,startDate,endDate).
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
        ///    Counts the number of microsecond boundaries crossed between the startDate and endDate.
        ///    Corresponds to SQL Server's DATEDIFF(MICROSECOND,startDate,endDate).
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
        ///    Counts the number of microsecond boundaries crossed between the startDate and endDate.
        ///    Corresponds to SQL Server's DATEDIFF(MICROSECOND,startDate,endDate).
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
        ///    Counts the number of microsecond boundaries crossed between the startDate and endDate.
        ///    Corresponds to SQL Server's DATEDIFF(MICROSECOND,startDate,endDate).
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
        ///    Counts the number of microsecond boundaries crossed between the startDate and endDate.
        ///    Corresponds to SQL Server's DATEDIFF(MICROSECOND,startDate,endDate).
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
        ///    Counts the number of nanosecond boundaries crossed between the startDate and endDate.
        ///    Corresponds to SQL Server's DATEDIFF(NANOSECOND,startDate,endDate).
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
        ///    Counts the number of nanosecond boundaries crossed between the startDate and endDate.
        ///    Corresponds to SQL Server's DATEDIFF(NANOSECOND,startDate,endDate).
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
        ///    Counts the number of nanosecond boundaries crossed between the startDate and endDate.
        ///    Corresponds to SQL Server's DATEDIFF(NANOSECOND,startDate,endDate).
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
        ///    Counts the number of nanosecond boundaries crossed between the startDate and endDate.
        ///    Corresponds to SQL Server's DATEDIFF(NANOSECOND,startDate,endDate).
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
        ///     <para>
        ///         An implementation of the SQL LIKE operation. On relational databases this is usually directly
        ///         translated to SQL.
        ///     </para>
        ///     <para>
        ///         Note that if this function is translated into SQL, then the semantics of the comparison will
        ///         depend on the database configuration. In particular, it may be either case-sensitive or
        ///         case-insensitive. If this function is evaluated on the client, then it will always use
        ///         a case-insensitive comparison.
        ///     </para>
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="matchExpression">The string that is to be matched.</param>
        /// <param name="pattern">The pattern which may involve wildcards %,_,[,],^.</param>
        /// <returns>true if there is a match.</returns>
        public static bool Like(
            [CanBeNull] this DbFunctions _,
            [CanBeNull] string matchExpression,
            [CanBeNull] string pattern)
            => LikeCore(matchExpression, pattern, escapeCharacter: null);

        /// <summary>
        ///     <para>
        ///         An implementation of the SQL LIKE operation. On relational databases this is usually directly
        ///         translated to SQL.
        ///     </para>
        ///     <para>
        ///         Note that if this function is translated into SQL, then the semantics of the comparison will
        ///         depend on the database configuration. In particular, it may be either case-sensitive or
        ///         case-insensitive. If this function is evaluated on the client, then it will always use
        ///         a case-insensitive comparison.
        ///     </para>
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="matchExpression">The string that is to be matched.</param>
        /// <param name="pattern">The pattern which may involve wildcards %,_,[,],^.</param>
        /// <param name="escapeCharacter">
        ///     The escape character (as a single character string) to use in front of %,_,[,],^
        ///     if they are not used as wildcards.
        /// </param>
        /// <returns>true if there is a match.</returns>
        public static bool Like(
            [CanBeNull] this DbFunctions _,
            [CanBeNull] string matchExpression,
            [CanBeNull] string pattern,
            [CanBeNull] string escapeCharacter)
            => LikeCore(matchExpression, pattern, escapeCharacter);

        // Regex special chars defined here:
        // https://msdn.microsoft.com/en-us/library/4edbef7e(v=vs.110).aspx

        private static readonly char[] _regexSpecialChars
            = { '.', '$', '^', '{', '[', '(', '|', ')', '*', '+', '?', '\\' };

        private static readonly string _defaultEscapeRegexCharsPattern
            = BuildEscapeRegexCharsPattern(_regexSpecialChars);

        private static readonly TimeSpan _regexTimeout = TimeSpan.FromMilliseconds(value: 1000.0);

        private static string BuildEscapeRegexCharsPattern(IEnumerable<char> regexSpecialChars)
        {
            return string.Join("|", regexSpecialChars.Select(c => @"\" + c));
        }

        private static bool LikeCore(string matchExpression, string pattern, string escapeCharacter)
        {
            //TODO: this fixes https://github.com/aspnet/EntityFramework/issues/8656 by insisting that
            // the "escape character" is a string but just using the first character of that string,
            // but we may later want to allow the complete string as the "escape character"
            // in which case we need to change the way we construct the regex below.
            var singleEscapeCharacter =
                (escapeCharacter == null || escapeCharacter.Length == 0)
                    ? (char?)null
                    : escapeCharacter.First();

            if (matchExpression == null
                || pattern == null)
            {
                return false;
            }

            if (matchExpression.Equals(pattern, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            if (matchExpression.Length == 0
                || pattern.Length == 0)
            {
                return false;
            }

            var escapeRegexCharsPattern
                = singleEscapeCharacter == null
                    ? _defaultEscapeRegexCharsPattern
                    : BuildEscapeRegexCharsPattern(_regexSpecialChars.Where(c => c != singleEscapeCharacter));

            var regexPattern
                = Regex.Replace(
                    pattern,
                    escapeRegexCharsPattern,
                    c => @"\" + c,
                    default,
                    _regexTimeout);

            var stringBuilder = new StringBuilder();

            for (var i = 0; i < regexPattern.Length; i++)
            {
                var c = regexPattern[i];
                var escaped = i > 0 && regexPattern[i - 1] == singleEscapeCharacter;

                switch (c)
                {
                    case '_':
                        {
                            stringBuilder.Append(escaped ? '_' : '.');
                            break;
                        }
                    case '%':
                        {
                            stringBuilder.Append(escaped ? "%" : ".*");
                            break;
                        }
                    default:
                        {
                            if (c != singleEscapeCharacter)
                            {
                                stringBuilder.Append(c);
                            }

                            break;
                        }
                }
            }

            regexPattern = stringBuilder.ToString();

            return Regex.IsMatch(
                matchExpression,
                @"\A" + regexPattern + @"\s*\z",
                RegexOptions.IgnoreCase | RegexOptions.Singleline,
                _regexTimeout);
        }
    }
}
