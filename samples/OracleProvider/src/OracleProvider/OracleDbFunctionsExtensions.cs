// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore
{
    /// <summary>
    ///     Provides CLR methods that get translated to database functions when used in LINQ to Entities queries.
    ///     The methods on this class are accessed via <see cref="EF.Functions" />.
    /// </summary>
    public static class OracleDbFunctionsExtensions
    {
        /// <summary>
        ///    Counts the number of year boundaries crossed between the startDate and endDate.
        ///    Corresponds to Oracle EXTRACT(YEAR FROM endDate) - EXTRACT(YEAR FROM startDate)
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
        ///    Corresponds to Oracle EXTRACT(YEAR FROM endDate) - EXTRACT(YEAR FROM startDate)
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
        ///    Corresponds to Oracle EXTRACT(YEAR FROM endDate) - EXTRACT(YEAR FROM startDate)
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
        ///    Corresponds to Oracle EXTRACT(YEAR FROM endDate) - EXTRACT(YEAR FROM startDate)
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
        ///    Corresponds to Oracle TRUNC(MONTHS_BETWEEN(endDate, startDate))
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
        ///    Corresponds to Oracle TRUNC(MONTHS_BETWEEN(endDate, startDate))
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
        ///    Corresponds to Oracle TRUNC(MONTHS_BETWEEN(endDate, startDate))
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
        ///    Corresponds to Oracle TRUNC(MONTHS_BETWEEN(endDate, startDate))
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
        ///    Corresponds to Oracle EXTRACT(DAY FROM endDate - startDate)
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
        ///    Corresponds to Oracle EXTRACT(DAY FROM endDate - startDate)
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
        ///    Corresponds to Oracle EXTRACT(DAY FROM endDate - startDate)
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
        ///    Corresponds to Oracle EXTRACT(DAY FROM 24 * TO_DSINTERVAL(endDate - startDate))
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
        ///    Corresponds to Oracle EXTRACT(DAY FROM 24 * TO_DSINTERVAL(endDate - startDate))
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
        ///    Corresponds to Oracle EXTRACT(DAY FROM 24 * TO_DSINTERVAL(endDate - startDate))
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
        ///    Corresponds to Oracle EXTRACT(DAY FROM 24 * TO_DSINTERVAL(endDate - startDate))
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
        ///    Corresponds to Oracle EXTRACT(DAY FROM 60 * (24 * TO_DSINTERVAL(endDate - startDate)))
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
        ///    Corresponds to Oracle EXTRACT(DAY FROM 60 * (24 * TO_DSINTERVAL(endDate - startDate)))
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
        ///    Corresponds to Oracle EXTRACT(DAY FROM 60 * (24 * TO_DSINTERVAL(endDate - startDate)))
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
        ///    Corresponds to Oracle EXTRACT(DAY FROM 60 * (24 * TO_DSINTERVAL(endDate - startDate)))
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
        ///    Corresponds to Oracle EXTRACT(DAY FROM 60 * (60 * (24 * TO_DSINTERVAL(endDate - startDate))))
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
        ///    Corresponds to Oracle EXTRACT(DAY FROM 60 * (60 * (24 * TO_DSINTERVAL(endDate - startDate))))
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
        ///    Corresponds to Oracle EXTRACT(DAY FROM 60 * (60 * (24 * TO_DSINTERVAL(endDate - startDate))))
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
    }
}
