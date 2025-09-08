// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Diagnostics;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore
{
    // TODO: Change method return types for units of `SECOND` and smaller from `int` to `long`.

    /// <summary>
    ///     Provides CLR methods that get translated to database functions when used in LINQ to Entities queries.
    ///     The methods on this class are accessed via <see cref="EF.Functions" />.
    /// </summary>
    public static class XGDbFunctionsExtensions
    {
        #region ConvertTimeZone

        /// <summary>
        ///     Converts the `DateTime` value <paramref name="dateTime"/> from the time zone given by <paramref name="fromTimeZone"/> to the time zone given by <paramref name="toTimeZone"/> and returns the resulting value.
        ///     Corresponds to ``CONVERT_TZ(dateTime, fromTimeZone, toTimeZone)``.
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="dateTime">The `DateTime` value to convert.</param>
        /// <param name="fromTimeZone">The time zone to convert from.</param>
        /// <param name="toTimeZone">The time zone to convert to.</param>
        /// <returns>The converted value.</returns>
        public static DateTime? ConvertTimeZone(
            [CanBeNull] this DbFunctions _,
            DateTime dateTime,
            string fromTimeZone,
            string toTimeZone)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(ConvertTimeZone)));

        /// <summary>
        ///     Converts the `DateOnly` value <paramref name="dateOnly"/> from the time zone given by <paramref name="fromTimeZone"/> to the time zone given by <paramref name="toTimeZone"/> and returns the resulting value.
        ///     Corresponds to ``CONVERT_TZ(dateTime, fromTimeZone, toTimeZone)`.`.
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="dateOnly">The `DateOnly` value to convert.</param>
        /// <param name="fromTimeZone">The time zone to convert from.</param>
        /// <param name="toTimeZone">The time zone to convert to.</param>
        /// <returns>The converted value.</returns>
        public static DateOnly? ConvertTimeZone(
            [CanBeNull] this DbFunctions _,
            DateOnly dateOnly,
            string fromTimeZone,
            string toTimeZone)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(ConvertTimeZone)));

        /// <summary>
        ///     Converts the `DateTime?` value <paramref name="dateTime"/> from the time zone given by <paramref name="fromTimeZone"/> to the time zone given by <paramref name="toTimeZone"/> and returns the resulting value.
        ///     Corresponds to ``CONVERT_TZ(dateTime, fromTimeZone, toTimeZone)``.
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="dateTime">The `DateTime?` value to convert.</param>
        /// <param name="fromTimeZone">The time zone to convert from.</param>
        /// <param name="toTimeZone">The time zone to convert to.</param>
        /// <returns>The converted value.</returns>
        public static DateTime? ConvertTimeZone(
            [CanBeNull] this DbFunctions _,
            DateTime? dateTime,
            string fromTimeZone,
            string toTimeZone)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(ConvertTimeZone)));

        /// <summary>
        ///     Converts the `DateOnly?` value <paramref name="dateOnly"/> from the time zone given by <paramref name="fromTimeZone"/> to the time zone given by <paramref name="toTimeZone"/> and returns the resulting value.
        ///     Corresponds to ``CONVERT_TZ(dateTime, fromTimeZone, toTimeZone)`.`.
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="dateOnly">The `DateOnly?` value to convert.</param>
        /// <param name="fromTimeZone">The time zone to convert from.</param>
        /// <param name="toTimeZone">The time zone to convert to.</param>
        /// <returns>The converted value.</returns>
        public static DateOnly? ConvertTimeZone(
            [CanBeNull] this DbFunctions _,
            DateOnly? dateOnly,
            string fromTimeZone,
            string toTimeZone)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(ConvertTimeZone)));

        /// <summary>
        ///     Converts the `DateTime` value <paramref name="dateTime"/> from `@@session.time_zone` to the time zone given by <paramref name="toTimeZone"/> and returns the resulting value.
        ///     Corresponds to ``CONVERT_TZ(dateTime, @@session.time_zone, toTimeZone)``.
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="dateTime">The `DateTime` value to convert.</param>
        /// <param name="toTimeZone">The time zone to convert to.</param>
        /// <returns>The converted value.</returns>
        public static DateTime? ConvertTimeZone(
            [CanBeNull] this DbFunctions _,
            DateTime dateTime,
            string toTimeZone)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(ConvertTimeZone)));

        /// <summary>
        ///     Converts the `DateTimeOffset` value <paramref name="dateTimeOffset"/> from `+00:00`/UTC to the time zone given by <paramref name="toTimeZone"/> and returns the resulting value as a `DateTime`.
        ///     Corresponds to ``CONVERT_TZ(dateTime, '+00:00', toTimeZone)``.
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="dateTimeOffset">The `DateTimeOffset` value to convert.</param>
        /// <param name="toTimeZone">The time zone to convert to.</param>
        /// <returns>The converted `DateTime?` value.</returns>
        public static DateTime? ConvertTimeZone(
            [CanBeNull] this DbFunctions _,
            DateTimeOffset dateTimeOffset,
            string toTimeZone)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(ConvertTimeZone)));

        /// <summary>
        ///     Converts the `DateOnly` value <paramref name="dateOnly"/> from `@@session.time_zone` to the time zone given by <paramref name="toTimeZone"/> and returns the resulting value.
        ///     Corresponds to ``CONVERT_TZ(dateTime, @@session.time_zone, toTimeZone)``.
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="dateOnly">The `DateOnly` value to convert.</param>
        /// <param name="toTimeZone">The time zone to convert to.</param>
        /// <returns>The converted value.</returns>
        public static DateOnly? ConvertTimeZone(
            [CanBeNull] this DbFunctions _,
            DateOnly dateOnly,
            string toTimeZone)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(ConvertTimeZone)));

        /// <summary>
        ///     Converts the `DateTime?` value <paramref name="dateTime"/> from `@@session.time_zone` to the time zone given by <paramref name="toTimeZone"/> and returns the resulting value.
        ///     Corresponds to ``CONVERT_TZ(dateTime, @@session.time_zone, toTimeZone)``.
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="dateTime">The `DateTime?` value to convert.</param>
        /// <param name="toTimeZone">The time zone to convert to.</param>
        /// <returns>The converted value.</returns>
        public static DateTime? ConvertTimeZone(
            [CanBeNull] this DbFunctions _,
            DateTime? dateTime,
            string toTimeZone)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(ConvertTimeZone)));

        /// <summary>
        ///     Converts the `DateTimeOffset?` value <paramref name="dateTimeOffset"/> from `+00:00`/UTC to the time zone given by <paramref name="toTimeZone"/> and returns the resulting value as a `DateTime`.
        ///     Corresponds to ``CONVERT_TZ(dateTime, '+00:00', toTimeZone)``.
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="dateTimeOffset">The `DateTimeOffset?` value to convert.</param>
        /// <param name="toTimeZone">The time zone to convert to.</param>
        /// <returns>The converted `DateTime?` value.</returns>
        public static DateTime? ConvertTimeZone(
            [CanBeNull] this DbFunctions _,
            DateTimeOffset? dateTimeOffset,
            string toTimeZone)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(ConvertTimeZone)));

        /// <summary>
        ///     Converts the `DateOnly?` value <paramref name="dateOnly"/> from `@@session.time_zone` to the time zone given by <paramref name="toTimeZone"/> and returns the resulting value.
        ///     Corresponds to ``CONVERT_TZ(dateTime, @@session.time_zone, toTimeZone)``.
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="dateOnly">The `DateOnly?` value to convert.</param>
        /// <param name="toTimeZone">The time zone to convert to.</param>
        /// <returns>The converted value.</returns>
        public static DateOnly? ConvertTimeZone(
            [CanBeNull] this DbFunctions _,
            DateOnly? dateOnly,
            string toTimeZone)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(ConvertTimeZone)));

        #endregion ConvertTimeZone

        #region DateDiffYear

        /// <summary>
        ///     Counts the number of year boundaries crossed between the startDate and endDate.
        ///     Corresponds to `TIMESTAMPDIFF(YEAR,startDate,endDate)`.
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="startDate">Starting date for the calculation.</param>
        /// <param name="endDate">Ending date for the calculation.</param>
        /// <returns>Number of year boundaries crossed between the dates.</returns>
        public static int DateDiffYear(
            [CanBeNull] this DbFunctions _,
            DateTime startDate,
            DateTime endDate)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffYear)));

        /// <summary>
        ///     Counts the number of year boundaries crossed between the startDate and endDate.
        ///     Corresponds to `TIMESTAMPDIFF(YEAR,startDate,endDate)`.
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="startDate">Starting date for the calculation.</param>
        /// <param name="endDate">Ending date for the calculation.</param>
        /// <returns>Number of year boundaries crossed between the dates.</returns>
        public static int? DateDiffYear(
            [CanBeNull] this DbFunctions _,
            DateTime? startDate,
            DateTime? endDate)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffYear)));

        /// <summary>
        ///     Counts the number of year boundaries crossed between the startDate and endDate.
        ///     Corresponds to `TIMESTAMPDIFF(YEAR,startDate,endDate)`.
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="startDate">Starting date for the calculation.</param>
        /// <param name="endDate">Ending date for the calculation.</param>
        /// <returns>Number of year boundaries crossed between the dates.</returns>
        public static int DateDiffYear(
            [CanBeNull] this DbFunctions _,
            DateTimeOffset startDate,
            DateTimeOffset endDate)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffYear)));

        /// <summary>
        ///     Counts the number of year boundaries crossed between the startDate and endDate.
        ///     Corresponds to `TIMESTAMPDIFF(YEAR,startDate,endDate)`.
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="startDate">Starting date for the calculation.</param>
        /// <param name="endDate">Ending date for the calculation.</param>
        /// <returns>Number of year boundaries crossed between the dates.</returns>
        public static int? DateDiffYear(
            [CanBeNull] this DbFunctions _,
            DateTimeOffset? startDate,
            DateTimeOffset? endDate)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffYear)));

        /// <summary>
        ///     Counts the number of year boundaries crossed between the startDate and endDate.
        ///     Corresponds to `TIMESTAMPDIFF(YEAR,startDate,endDate)`.
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="startDate">Starting date for the calculation.</param>
        /// <param name="endDate">Ending date for the calculation.</param>
        /// <returns>Number of year boundaries crossed between the dates.</returns>
        public static int DateDiffYear(
            [CanBeNull] this DbFunctions _,
            DateOnly startDate,
            DateOnly endDate)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffYear)));

        /// <summary>
        ///     Counts the number of year boundaries crossed between the startDate and endDate.
        ///     Corresponds to `TIMESTAMPDIFF(YEAR,startDate,endDate)`.
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="startDate">Starting date for the calculation.</param>
        /// <param name="endDate">Ending date for the calculation.</param>
        /// <returns>Number of year boundaries crossed between the dates.</returns>
        public static int? DateDiffYear(
            [CanBeNull] this DbFunctions _,
            DateOnly? startDate,
            DateOnly? endDate)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffYear)));

        #endregion DateDiffYear

        #region DateDiffQuarter

        /// <summary>
        ///     Counts the number of quarter boundaries crossed between the startDate and endDate.
        ///     Corresponds to `TIMESTAMPDIFF(QUARTER,startDate,endDate)`.
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="startDate">Starting date for the calculation.</param>
        /// <param name="endDate">Ending date for the calculation.</param>
        /// <returns>Number of quarter boundaries crossed between the dates.</returns>
        public static int DateDiffQuarter(
            [CanBeNull] this DbFunctions _,
            DateTime startDate,
            DateTime endDate)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffQuarter)));

        /// <summary>
        ///     Counts the number of quarter boundaries crossed between the startDate and endDate.
        ///     Corresponds to `TIMESTAMPDIFF(QUARTER,startDate,endDate)`.
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="startDate">Starting date for the calculation.</param>
        /// <param name="endDate">Ending date for the calculation.</param>
        /// <returns>Number of quarter boundaries crossed between the dates.</returns>
        public static int? DateDiffQuarter(
            [CanBeNull] this DbFunctions _,
            DateTime? startDate,
            DateTime? endDate)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffQuarter)));

        /// <summary>
        ///     Counts the number of quarter boundaries crossed between the startDate and endDate.
        ///     Corresponds to `TIMESTAMPDIFF(QUARTER,startDate,endDate)`.
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="startDate">Starting date for the calculation.</param>
        /// <param name="endDate">Ending date for the calculation.</param>
        /// <returns>Number of quarter boundaries crossed between the dates.</returns>
        public static int DateDiffQuarter(
            [CanBeNull] this DbFunctions _,
            DateTimeOffset startDate,
            DateTimeOffset endDate)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffQuarter)));

        /// <summary>
        ///     Counts the number of quarter boundaries crossed between the startDate and endDate.
        ///     Corresponds to `TIMESTAMPDIFF(QUARTER,startDate,endDate)`.
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="startDate">Starting date for the calculation.</param>
        /// <param name="endDate">Ending date for the calculation.</param>
        /// <returns>Number of quarter boundaries crossed between the dates.</returns>
        public static int? DateDiffQuarter(
            [CanBeNull] this DbFunctions _,
            DateTimeOffset? startDate,
            DateTimeOffset? endDate)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffQuarter)));

        /// <summary>
        ///     Counts the number of quarter boundaries crossed between the startDate and endDate.
        ///     Corresponds to `TIMESTAMPDIFF(QUARTER,startDate,endDate)`.
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="startDate">Starting date for the calculation.</param>
        /// <param name="endDate">Ending date for the calculation.</param>
        /// <returns>Number of quarter boundaries crossed between the dates.</returns>
        public static int DateDiffQuarter(
            [CanBeNull] this DbFunctions _,
            DateOnly startDate,
            DateOnly endDate)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffQuarter)));

        /// <summary>
        ///     Counts the number of quarter boundaries crossed between the startDate and endDate.
        ///     Corresponds to `TIMESTAMPDIFF(QUARTER,startDate,endDate)`.
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="startDate">Starting date for the calculation.</param>
        /// <param name="endDate">Ending date for the calculation.</param>
        /// <returns>Number of quarter boundaries crossed between the dates.</returns>
        public static int? DateDiffQuarter(
            [CanBeNull] this DbFunctions _,
            DateOnly? startDate,
            DateOnly? endDate)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffQuarter)));

        #endregion DateDiffQuarter

        #region DateDiffMonth

        /// <summary>
        ///     Counts the number of month boundaries crossed between the startDate and endDate.
        ///     Corresponds to `TIMESTAMPDIFF(MONTH,startDate,endDate)`.
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="startDate">Starting date for the calculation.</param>
        /// <param name="endDate">Ending date for the calculation.</param>
        /// <returns>Number of month boundaries crossed between the dates.</returns>
        public static int DateDiffMonth(
            [CanBeNull] this DbFunctions _,
            DateTime startDate,
            DateTime endDate)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffMonth)));

        /// <summary>
        ///     Counts the number of month boundaries crossed between the startDate and endDate.
        ///     Corresponds to `TIMESTAMPDIFF(MONTH,startDate,endDate)`.
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="startDate">Starting date for the calculation.</param>
        /// <param name="endDate">Ending date for the calculation.</param>
        /// <returns>Number of month boundaries crossed between the dates.</returns>
        public static int? DateDiffMonth(
            [CanBeNull] this DbFunctions _,
            DateTime? startDate,
            DateTime? endDate)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffMonth)));

        /// <summary>
        ///     Counts the number of month boundaries crossed between the startDate and endDate.
        ///     Corresponds to `TIMESTAMPDIFF(MONTH,startDate,endDate)`.
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="startDate">Starting date for the calculation.</param>
        /// <param name="endDate">Ending date for the calculation.</param>
        /// <returns>Number of month boundaries crossed between the dates.</returns>
        public static int DateDiffMonth(
            [CanBeNull] this DbFunctions _,
            DateTimeOffset startDate,
            DateTimeOffset endDate)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffMonth)));

        /// <summary>
        ///     Counts the number of month boundaries crossed between the startDate and endDate.
        ///     Corresponds to `TIMESTAMPDIFF(MONTH,startDate,endDate)`.
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="startDate">Starting date for the calculation.</param>
        /// <param name="endDate">Ending date for the calculation.</param>
        /// <returns>Number of month boundaries crossed between the dates.</returns>
        public static int? DateDiffMonth(
            [CanBeNull] this DbFunctions _,
            DateTimeOffset? startDate,
            DateTimeOffset? endDate)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffMonth)));

        /// <summary>
        ///     Counts the number of month boundaries crossed between the startDate and endDate.
        ///     Corresponds to `TIMESTAMPDIFF(MONTH,startDate,endDate)`.
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="startDate">Starting date for the calculation.</param>
        /// <param name="endDate">Ending date for the calculation.</param>
        /// <returns>Number of month boundaries crossed between the dates.</returns>
        public static int DateDiffMonth(
            [CanBeNull] this DbFunctions _,
            DateOnly startDate,
            DateOnly endDate)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffMonth)));

        /// <summary>
        ///     Counts the number of month boundaries crossed between the startDate and endDate.
        ///     Corresponds to `TIMESTAMPDIFF(MONTH,startDate,endDate)`.
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="startDate">Starting date for the calculation.</param>
        /// <param name="endDate">Ending date for the calculation.</param>
        /// <returns>Number of month boundaries crossed between the dates.</returns>
        public static int? DateDiffMonth(
            [CanBeNull] this DbFunctions _,
            DateOnly? startDate,
            DateOnly? endDate)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffMonth)));

        #endregion DateDiffMonth

        #region DateDiffWeek

        /// <summary>
        ///     Counts the number of week boundaries crossed between the startDate and endDate.
        ///     Corresponds to `TIMESTAMPDIFF(WEEK,startDate,endDate)`.
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="startDate">Starting date for the calculation.</param>
        /// <param name="endDate">Ending date for the calculation.</param>
        /// <returns>Number of week boundaries crossed between the dates.</returns>
        public static int DateDiffWeek(
            [CanBeNull] this DbFunctions _,
            DateTime startDate,
            DateTime endDate)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffWeek)));

        /// <summary>
        ///     Counts the number of week boundaries crossed between the startDate and endDate.
        ///     Corresponds to `TIMESTAMPDIFF(WEEK,startDate,endDate)`.
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="startDate">Starting date for the calculation.</param>
        /// <param name="endDate">Ending date for the calculation.</param>
        /// <returns>Number of week boundaries crossed between the dates.</returns>
        public static int? DateDiffWeek(
            [CanBeNull] this DbFunctions _,
            DateTime? startDate,
            DateTime? endDate)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffWeek)));

        /// <summary>
        ///     Counts the number of week boundaries crossed between the startDate and endDate.
        ///     Corresponds to `TIMESTAMPDIFF(WEEK,startDate,endDate)`.
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="startDate">Starting date for the calculation.</param>
        /// <param name="endDate">Ending date for the calculation.</param>
        /// <returns>Number of week boundaries crossed between the dates.</returns>
        public static int DateDiffWeek(
            [CanBeNull] this DbFunctions _,
            DateTimeOffset startDate,
            DateTimeOffset endDate)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffWeek)));

        /// <summary>
        ///     Counts the number of week boundaries crossed between the startDate and endDate.
        ///     Corresponds to `TIMESTAMPDIFF(WEEK,startDate,endDate)`.
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="startDate">Starting date for the calculation.</param>
        /// <param name="endDate">Ending date for the calculation.</param>
        /// <returns>Number of week boundaries crossed between the dates.</returns>
        public static int? DateDiffWeek(
            [CanBeNull] this DbFunctions _,
            DateTimeOffset? startDate,
            DateTimeOffset? endDate)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffWeek)));

        /// <summary>
        ///     Counts the number of week boundaries crossed between the startDate and endDate.
        ///     Corresponds to `TIMESTAMPDIFF(WEEK,startDate,endDate)`.
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="startDate">Starting date for the calculation.</param>
        /// <param name="endDate">Ending date for the calculation.</param>
        /// <returns>Number of week boundaries crossed between the dates.</returns>
        public static int DateDiffWeek(
            [CanBeNull] this DbFunctions _,
            DateOnly startDate,
            DateOnly endDate)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffWeek)));

        /// <summary>
        ///     Counts the number of week boundaries crossed between the startDate and endDate.
        ///     Corresponds to `TIMESTAMPDIFF(WEEK,startDate,endDate)`.
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="startDate">Starting date for the calculation.</param>
        /// <param name="endDate">Ending date for the calculation.</param>
        /// <returns>Number of week boundaries crossed between the dates.</returns>
        public static int? DateDiffWeek(
            [CanBeNull] this DbFunctions _,
            DateOnly? startDate,
            DateOnly? endDate)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffWeek)));

        #endregion DateDiffWeek

        #region DateDiffDay

        /// <summary>
        ///     Counts the number of day boundaries crossed between the startDate and endDate.
        ///     Corresponds to `TIMESTAMPDIFF(DAY,startDate,endDate)`.
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="startDate">Starting date for the calculation.</param>
        /// <param name="endDate">Ending date for the calculation.</param>
        /// <returns>Number of day boundaries crossed between the dates.</returns>
        public static int DateDiffDay(
            [CanBeNull] this DbFunctions _,
            DateTime startDate,
            DateTime endDate)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffDay)));

        /// <summary>
        ///     Counts the number of day boundaries crossed between the startDate and endDate.
        ///     Corresponds to `TIMESTAMPDIFF(DAY,startDate,endDate)`.
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="startDate">Starting date for the calculation.</param>
        /// <param name="endDate">Ending date for the calculation.</param>
        /// <returns>Number of day boundaries crossed between the dates.</returns>
        public static int? DateDiffDay(
            [CanBeNull] this DbFunctions _,
            DateTime? startDate,
            DateTime? endDate)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffDay)));

        /// <summary>
        ///     Counts the number of day boundaries crossed between the startDate and endDate.
        ///     Corresponds to `TIMESTAMPDIFF(DAY,startDate,endDate)`.
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="startDate">Starting date for the calculation.</param>
        /// <param name="endDate">Ending date for the calculation.</param>
        /// <returns>Number of day boundaries crossed between the dates.</returns>
        public static int DateDiffDay(
            [CanBeNull] this DbFunctions _,
            DateTimeOffset startDate,
            DateTimeOffset endDate)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffDay)));

        /// <summary>
        ///     Counts the number of day boundaries crossed between the startDate and endDate.
        ///     Corresponds to `TIMESTAMPDIFF(DAY,startDate,endDate)`.
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="startDate">Starting date for the calculation.</param>
        /// <param name="endDate">Ending date for the calculation.</param>
        /// <returns>Number of day boundaries crossed between the dates.</returns>
        public static int? DateDiffDay(
            [CanBeNull] this DbFunctions _,
            DateTimeOffset? startDate,
            DateTimeOffset? endDate)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffDay)));

        /// <summary>
        ///     Counts the number of day boundaries crossed between the startDate and endDate.
        ///     Corresponds to `TIMESTAMPDIFF(DAY,startDate,endDate)`.
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="startDate">Starting date for the calculation.</param>
        /// <param name="endDate">Ending date for the calculation.</param>
        /// <returns>Number of day boundaries crossed between the dates.</returns>
        public static int DateDiffDay(
            [CanBeNull] this DbFunctions _,
            DateOnly startDate,
            DateOnly endDate)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffDay)));

        /// <summary>
        ///     Counts the number of day boundaries crossed between the startDate and endDate.
        ///     Corresponds to `TIMESTAMPDIFF(DAY,startDate,endDate)`.
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="startDate">Starting date for the calculation.</param>
        /// <param name="endDate">Ending date for the calculation.</param>
        /// <returns>Number of day boundaries crossed between the dates.</returns>
        public static int? DateDiffDay(
            [CanBeNull] this DbFunctions _,
            DateOnly? startDate,
            DateOnly? endDate)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffDay)));

        #endregion DateDiffDay

        #region DateDiffHour

        /// <summary>
        ///     Counts the number of hour boundaries crossed between the startDate and endDate.
        ///     Corresponds to `TIMESTAMPDIFF(HOUR,startDate,endDate)`.
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="startDate">Starting date for the calculation.</param>
        /// <param name="endDate">Ending date for the calculation.</param>
        /// <returns>Number of hour boundaries crossed between the dates.</returns>
        public static int DateDiffHour(
            [CanBeNull] this DbFunctions _,
            DateTime startDate,
            DateTime endDate)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffHour)));

        /// <summary>
        ///     Counts the number of hour boundaries crossed between the startDate and endDate.
        ///     Corresponds to `TIMESTAMPDIFF(HOUR,startDate,endDate)`.
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="startDate">Starting date for the calculation.</param>
        /// <param name="endDate">Ending date for the calculation.</param>
        /// <returns>Number of hour boundaries crossed between the dates.</returns>
        public static int? DateDiffHour(
            [CanBeNull] this DbFunctions _,
            DateTime? startDate,
            DateTime? endDate)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffHour)));

        /// <summary>
        ///     Counts the number of hour boundaries crossed between the startDate and endDate.
        ///     Corresponds to `TIMESTAMPDIFF(HOUR,startDate,endDate)`.
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="startDate">Starting date for the calculation.</param>
        /// <param name="endDate">Ending date for the calculation.</param>
        /// <returns>Number of hour boundaries crossed between the dates.</returns>
        public static int DateDiffHour(
            [CanBeNull] this DbFunctions _,
            DateTimeOffset startDate,
            DateTimeOffset endDate)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffHour)));

        /// <summary>
        ///     Counts the number of hour boundaries crossed between the startDate and endDate.
        ///     Corresponds to `TIMESTAMPDIFF(HOUR,startDate,endDate)`.
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="startDate">Starting date for the calculation.</param>
        /// <param name="endDate">Ending date for the calculation.</param>
        /// <returns>Number of hour boundaries crossed between the dates.</returns>
        public static int? DateDiffHour(
            [CanBeNull] this DbFunctions _,
            DateTimeOffset? startDate,
            DateTimeOffset? endDate)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffHour)));

        /// <summary>
        ///     Counts the number of hour boundaries crossed between the startDate and endDate.
        ///     Corresponds to `TIMESTAMPDIFF(HOUR,startDate,endDate)`.
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="startDate">Starting date for the calculation.</param>
        /// <param name="endDate">Ending date for the calculation.</param>
        /// <returns>Number of hour boundaries crossed between the dates.</returns>
        public static int DateDiffHour(
            [CanBeNull] this DbFunctions _,
            DateOnly startDate,
            DateOnly endDate)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffHour)));

        /// <summary>
        ///     Counts the number of hour boundaries crossed between the startDate and endDate.
        ///     Corresponds to `TIMESTAMPDIFF(HOUR,startDate,endDate)`.
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="startDate">Starting date for the calculation.</param>
        /// <param name="endDate">Ending date for the calculation.</param>
        /// <returns>Number of hour boundaries crossed between the dates.</returns>
        public static int? DateDiffHour(
            [CanBeNull] this DbFunctions _,
            DateOnly? startDate,
            DateOnly? endDate)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffHour)));

        #endregion DateDiffHour

        #region DateDiffMinute

        /// <summary>
        ///     Counts the number of minute boundaries crossed between the startDate and endDate.
        ///     Corresponds to `TIMESTAMPDIFF(MINUTE,startDate,endDate)`.
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="startDate">Starting date for the calculation.</param>
        /// <param name="endDate">Ending date for the calculation.</param>
        /// <returns>Number of minute boundaries crossed between the dates.</returns>
        public static int DateDiffMinute(
            [CanBeNull] this DbFunctions _,
            DateTime startDate,
            DateTime endDate)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffMinute)));

        /// <summary>
        ///     Counts the number of minute boundaries crossed between the startDate and endDate.
        ///     Corresponds to `TIMESTAMPDIFF(MINUTE,startDate,endDate)`.
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="startDate">Starting date for the calculation.</param>
        /// <param name="endDate">Ending date for the calculation.</param>
        /// <returns>Number of minute boundaries crossed between the dates.</returns>
        public static int? DateDiffMinute(
            [CanBeNull] this DbFunctions _,
            DateTime? startDate,
            DateTime? endDate)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffMinute)));

        /// <summary>
        ///     Counts the number of minute boundaries crossed between the startDate and endDate.
        ///     Corresponds to `TIMESTAMPDIFF(MINUTE,startDate,endDate)`.
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="startDate">Starting date for the calculation.</param>
        /// <param name="endDate">Ending date for the calculation.</param>
        /// <returns>Number of minute boundaries crossed between the dates.</returns>
        public static int DateDiffMinute(
            [CanBeNull] this DbFunctions _,
            DateTimeOffset startDate,
            DateTimeOffset endDate)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffMinute)));

        /// <summary>
        ///     Counts the number of minute boundaries crossed between the startDate and endDate.
        ///     Corresponds to `TIMESTAMPDIFF(MINUTE,startDate,endDate)`.
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="startDate">Starting date for the calculation.</param>
        /// <param name="endDate">Ending date for the calculation.</param>
        /// <returns>Number of minute boundaries crossed between the dates.</returns>
        public static int? DateDiffMinute(
            [CanBeNull] this DbFunctions _,
            DateTimeOffset? startDate,
            DateTimeOffset? endDate)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffMinute)));

        /// <summary>
        ///     Counts the number of minute boundaries crossed between the startDate and endDate.
        ///     Corresponds to `TIMESTAMPDIFF(MINUTE,startDate,endDate)`.
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="startDate">Starting date for the calculation.</param>
        /// <param name="endDate">Ending date for the calculation.</param>
        /// <returns>Number of minute boundaries crossed between the dates.</returns>
        public static int DateDiffMinute(
            [CanBeNull] this DbFunctions _,
            DateOnly startDate,
            DateOnly endDate)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffMinute)));

        /// <summary>
        ///     Counts the number of minute boundaries crossed between the startDate and endDate.
        ///     Corresponds to `TIMESTAMPDIFF(MINUTE,startDate,endDate)`.
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="startDate">Starting date for the calculation.</param>
        /// <param name="endDate">Ending date for the calculation.</param>
        /// <returns>Number of minute boundaries crossed between the dates.</returns>
        public static int? DateDiffMinute(
            [CanBeNull] this DbFunctions _,
            DateOnly? startDate,
            DateOnly? endDate)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffMinute)));

        #endregion DateDiffMinute

        #region DateDiffSecond

        /// <summary>
        ///     Counts the number of second boundaries crossed between the startDate and endDate.
        ///     Corresponds to `TIMESTAMPDIFF(SECOND,startDate,endDate)`.
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="startDate">Starting date for the calculation.</param>
        /// <param name="endDate">Ending date for the calculation.</param>
        /// <returns>Number of second boundaries crossed between the dates.</returns>
        public static int DateDiffSecond(
            [CanBeNull] this DbFunctions _,
            DateTime startDate,
            DateTime endDate)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffSecond)));

        /// <summary>
        ///     Counts the number of second boundaries crossed between the startDate and endDate.
        ///     Corresponds to `TIMESTAMPDIFF(SECOND,startDate,endDate)`.
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="startDate">Starting date for the calculation.</param>
        /// <param name="endDate">Ending date for the calculation.</param>
        /// <returns>Number of second boundaries crossed between the dates.</returns>
        public static int? DateDiffSecond(
            [CanBeNull] this DbFunctions _,
            DateTime? startDate,
            DateTime? endDate)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffSecond)));

        /// <summary>
        ///     Counts the number of second boundaries crossed between the startDate and endDate.
        ///     Corresponds to `TIMESTAMPDIFF(SECOND,startDate,endDate)`.
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="startDate">Starting date for the calculation.</param>
        /// <param name="endDate">Ending date for the calculation.</param>
        /// <returns>Number of second boundaries crossed between the dates.</returns>
        public static int DateDiffSecond(
            [CanBeNull] this DbFunctions _,
            DateTimeOffset startDate,
            DateTimeOffset endDate)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffSecond)));

        /// <summary>
        ///     Counts the number of second boundaries crossed between the startDate and endDate.
        ///     Corresponds to `TIMESTAMPDIFF(SECOND,startDate,endDate)`.
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="startDate">Starting date for the calculation.</param>
        /// <param name="endDate">Ending date for the calculation.</param>
        /// <returns>Number of second boundaries crossed between the dates.</returns>
        public static int? DateDiffSecond(
            [CanBeNull] this DbFunctions _,
            DateTimeOffset? startDate,
            DateTimeOffset? endDate)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffSecond)));

        /// <summary>
        ///     Counts the number of second boundaries crossed between the startDate and endDate.
        ///     Corresponds to `TIMESTAMPDIFF(SECOND,startDate,endDate)`.
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="startDate">Starting date for the calculation.</param>
        /// <param name="endDate">Ending date for the calculation.</param>
        /// <returns>Number of second boundaries crossed between the dates.</returns>
        public static int DateDiffSecond(
            [CanBeNull] this DbFunctions _,
            DateOnly startDate,
            DateOnly endDate)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffSecond)));

        /// <summary>
        ///     Counts the number of second boundaries crossed between the startDate and endDate.
        ///     Corresponds to `TIMESTAMPDIFF(SECOND,startDate,endDate)`.
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="startDate">Starting date for the calculation.</param>
        /// <param name="endDate">Ending date for the calculation.</param>
        /// <returns>Number of second boundaries crossed between the dates.</returns>
        public static int? DateDiffSecond(
            [CanBeNull] this DbFunctions _,
            DateOnly? startDate,
            DateOnly? endDate)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffSecond)));

        #endregion DateDiffSecond

        #region DateDiffMillisecond

        /// <summary>
        ///     Counts the number of millisecond boundaries crossed between the startDate and endDate.
        ///     Corresponds to `TIMESTAMPDIFF(MICROSECOND,startDate,endDate) DIV 1000`.
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="startDate">Starting date for the calculation.</param>
        /// <param name="endDate">Ending date for the calculation.</param>
        /// <returns>Number of millisecond boundaries crossed between the dates.</returns>
        public static int DateDiffMillisecond(
            [CanBeNull] this DbFunctions _,
            DateTime startDate,
            DateTime endDate)
         => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffMillisecond)));

        /// <summary>
        ///     Counts the number of millisecond boundaries crossed between the startDate and endDate.
        ///     Corresponds to `TIMESTAMPDIFF(MICROSECOND,startDate,endDate) DIV 1000`.
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="startDate">Starting date for the calculation.</param>
        /// <param name="endDate">Ending date for the calculation.</param>
        /// <returns>Number of millisecond boundaries crossed between the dates.</returns>
        public static int? DateDiffMillisecond(
            [CanBeNull] this DbFunctions _,
            DateTime? startDate,
            DateTime? endDate)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffMillisecond)));

        /// <summary>
        ///     Counts the number of millisecond boundaries crossed between the startDate and endDate.
        ///     Corresponds to `TIMESTAMPDIFF(MICROSECOND,startDate,endDate) DIV 1000`.
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="startDate">Starting date for the calculation.</param>
        /// <param name="endDate">Ending date for the calculation.</param>
        /// <returns>Number of millisecond boundaries crossed between the dates.</returns>
        public static int DateDiffMillisecond(
            [CanBeNull] this DbFunctions _,
            DateTimeOffset startDate,
            DateTimeOffset endDate)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffMillisecond)));

        /// <summary>
        ///     Counts the number of millisecond boundaries crossed between the startDate and endDate.
        ///     Corresponds to `TIMESTAMPDIFF(MICROSECOND,startDate,endDate) DIV 1000`.
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="startDate">Starting date for the calculation.</param>
        /// <param name="endDate">Ending date for the calculation.</param>
        /// <returns>Number of millisecond boundaries crossed between the dates.</returns>
        public static int? DateDiffMillisecond(
            [CanBeNull] this DbFunctions _,
            DateTimeOffset? startDate,
            DateTimeOffset? endDate)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffMillisecond)));

        /// <summary>
        ///     Counts the number of millisecond boundaries crossed between the startDate and endDate.
        ///     Corresponds to `TIMESTAMPDIFF(MICROSECOND,startDate,endDate) DIV 1000`.
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="startDate">Starting date for the calculation.</param>
        /// <param name="endDate">Ending date for the calculation.</param>
        /// <returns>Number of millisecond boundaries crossed between the dates.</returns>
        public static int DateDiffMillisecond(
            [CanBeNull] this DbFunctions _,
            DateOnly startDate,
            DateOnly endDate)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffMillisecond)));

        /// <summary>
        ///     Counts the number of millisecond boundaries crossed between the startDate and endDate.
        ///     Corresponds to `TIMESTAMPDIFF(MICROSECOND,startDate,endDate) DIV 1000`.
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="startDate">Starting date for the calculation.</param>
        /// <param name="endDate">Ending date for the calculation.</param>
        /// <returns>Number of millisecond boundaries crossed between the dates.</returns>
        public static int? DateDiffMillisecond(
            [CanBeNull] this DbFunctions _,
            DateOnly? startDate,
            DateOnly? endDate)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffMillisecond)));

        #endregion DateDiffMillisecond

        #region DateDiffMicrosecond

        /// <summary>
        ///     Counts the number of microsecond boundaries crossed between the startDate and endDate.
        ///     Corresponds to `TIMESTAMPDIFF(MICROSECOND,startDate,endDate)`.
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="startDate">Starting date for the calculation.</param>
        /// <param name="endDate">Ending date for the calculation.</param>
        /// <returns>Number of microsecond boundaries crossed between the dates.</returns>
        public static int DateDiffMicrosecond(
            [CanBeNull] this DbFunctions _,
            DateTime startDate,
            DateTime endDate)
         => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffMicrosecond)));

        /// <summary>
        ///     Counts the number of microsecond boundaries crossed between the startDate and endDate.
        ///     Corresponds to `TIMESTAMPDIFF(MICROSECOND,startDate,endDate)`.
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="startDate">Starting date for the calculation.</param>
        /// <param name="endDate">Ending date for the calculation.</param>
        /// <returns>Number of microsecond boundaries crossed between the dates.</returns>
        public static int? DateDiffMicrosecond(
            [CanBeNull] this DbFunctions _,
            DateTime? startDate,
            DateTime? endDate)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffMicrosecond)));

        /// <summary>
        ///     Counts the number of microsecond boundaries crossed between the startDate and endDate.
        ///     Corresponds to `TIMESTAMPDIFF(MICROSECOND,startDate,endDate)`.
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="startDate">Starting date for the calculation.</param>
        /// <param name="endDate">Ending date for the calculation.</param>
        /// <returns>Number of microsecond boundaries crossed between the dates.</returns>
        public static int DateDiffMicrosecond(
            [CanBeNull] this DbFunctions _,
            DateTimeOffset startDate,
            DateTimeOffset endDate)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffMicrosecond)));

        /// <summary>
        ///     Counts the number of microsecond boundaries crossed between the startDate and endDate.
        ///     Corresponds to `TIMESTAMPDIFF(MICROSECOND,startDate,endDate)`.
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="startDate">Starting date for the calculation.</param>
        /// <param name="endDate">Ending date for the calculation.</param>
        /// <returns>Number of microsecond boundaries crossed between the dates.</returns>
        public static int? DateDiffMicrosecond(
            [CanBeNull] this DbFunctions _,
            DateTimeOffset? startDate,
            DateTimeOffset? endDate)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffMicrosecond)));

        /// <summary>
        ///     Counts the number of microsecond boundaries crossed between the startDate and endDate.
        ///     Corresponds to `TIMESTAMPDIFF(MICROSECOND,startDate,endDate)`.
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="startDate">Starting date for the calculation.</param>
        /// <param name="endDate">Ending date for the calculation.</param>
        /// <returns>Number of microsecond boundaries crossed between the dates.</returns>
        public static int DateDiffMicrosecond(
            [CanBeNull] this DbFunctions _,
            DateOnly startDate,
            DateOnly endDate)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffMicrosecond)));

        /// <summary>
        ///     Counts the number of microsecond boundaries crossed between the startDate and endDate.
        ///     Corresponds to `TIMESTAMPDIFF(MICROSECOND,startDate,endDate)`.
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="startDate">Starting date for the calculation.</param>
        /// <param name="endDate">Ending date for the calculation.</param>
        /// <returns>Number of microsecond boundaries crossed between the dates.</returns>
        public static int? DateDiffMicrosecond(
            [CanBeNull] this DbFunctions _,
            DateOnly? startDate,
            DateOnly? endDate)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffMicrosecond)));

        #endregion DateDiffMicrosecond

        #region DateDiffTick

        /// <summary>
        ///     Counts the number of tick boundaries crossed between the startDate and endDate.
        ///     Corresponds to `TIMESTAMPDIFF(MICROSECOND,startDate,endDate) * 10`.
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="startDate">Starting date for the calculation.</param>
        /// <param name="endDate">Ending date for the calculation.</param>
        /// <returns>Number of tick boundaries crossed between the dates.</returns>
        public static int DateDiffTick(
            [CanBeNull] this DbFunctions _,
            DateTime startDate,
            DateTime endDate)
         => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffTick)));

        /// <summary>
        ///     Counts the number of tick boundaries crossed between the startDate and endDate.
        ///     Corresponds to `TIMESTAMPDIFF(MICROSECOND,startDate,endDate) * 10`.
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="startDate">Starting date for the calculation.</param>
        /// <param name="endDate">Ending date for the calculation.</param>
        /// <returns>Number of tick boundaries crossed between the dates.</returns>
        public static int? DateDiffTick(
            [CanBeNull] this DbFunctions _,
            DateTime? startDate,
            DateTime? endDate)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffTick)));

        /// <summary>
        ///     Counts the number of tick boundaries crossed between the startDate and endDate.
        ///     Corresponds to `TIMESTAMPDIFF(MICROSECOND,startDate,endDate) * 10`.
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="startDate">Starting date for the calculation.</param>
        /// <param name="endDate">Ending date for the calculation.</param>
        /// <returns>Number of tick boundaries crossed between the dates.</returns>
        public static int DateDiffTick(
            [CanBeNull] this DbFunctions _,
            DateTimeOffset startDate,
            DateTimeOffset endDate)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffTick)));

        /// <summary>
        ///     Counts the number of tick boundaries crossed between the startDate and endDate.
        ///     Corresponds to `TIMESTAMPDIFF(MICROSECOND,startDate,endDate) * 10`.
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="startDate">Starting date for the calculation.</param>
        /// <param name="endDate">Ending date for the calculation.</param>
        /// <returns>Number of tick boundaries crossed between the dates.</returns>
        public static int? DateDiffTick(
            [CanBeNull] this DbFunctions _,
            DateTimeOffset? startDate,
            DateTimeOffset? endDate)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffTick)));

        /// <summary>
        ///     Counts the number of tick boundaries crossed between the startDate and endDate.
        ///     Corresponds to `TIMESTAMPDIFF(MICROSECOND,startDate,endDate) * 10`.
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="startDate">Starting date for the calculation.</param>
        /// <param name="endDate">Ending date for the calculation.</param>
        /// <returns>Number of tick boundaries crossed between the dates.</returns>
        public static int DateDiffTick(
            [CanBeNull] this DbFunctions _,
            DateOnly startDate,
            DateOnly endDate)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffTick)));

        /// <summary>
        ///     Counts the number of tick boundaries crossed between the startDate and endDate.
        ///     Corresponds to `TIMESTAMPDIFF(MICROSECOND,startDate,endDate) * 10`.
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="startDate">Starting date for the calculation.</param>
        /// <param name="endDate">Ending date for the calculation.</param>
        /// <returns>Number of tick boundaries crossed between the dates.</returns>
        public static int? DateDiffTick(
            [CanBeNull] this DbFunctions _,
            DateOnly? startDate,
            DateOnly? endDate)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffTick)));

        #endregion DateDiffTick

        #region DateDiffNanosecond

        /// <summary>
        ///     Counts the number of nanosecond boundaries crossed between the startDate and endDate.
        ///     Corresponds to `TIMESTAMPDIFF(MICROSECOND,startDate,endDate) * 1000`.
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="startDate">Starting date for the calculation.</param>
        /// <param name="endDate">Ending date for the calculation.</param>
        /// <returns>Number of nanosecond boundaries crossed between the dates.</returns>
        public static int DateDiffNanosecond(
            [CanBeNull] this DbFunctions _,
            DateTime startDate,
            DateTime endDate)
         => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffNanosecond)));

        /// <summary>
        ///     Counts the number of nanosecond boundaries crossed between the startDate and endDate.
        ///     Corresponds to `TIMESTAMPDIFF(MICROSECOND,startDate,endDate) * 1000`.
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="startDate">Starting date for the calculation.</param>
        /// <param name="endDate">Ending date for the calculation.</param>
        /// <returns>Number of nanosecond boundaries crossed between the dates.</returns>
        public static int? DateDiffNanosecond(
            [CanBeNull] this DbFunctions _,
            DateTime? startDate,
            DateTime? endDate)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffNanosecond)));

        /// <summary>
        ///     Counts the number of nanosecond boundaries crossed between the startDate and endDate.
        ///     Corresponds to `TIMESTAMPDIFF(MICROSECOND,startDate,endDate) * 1000`.
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="startDate">Starting date for the calculation.</param>
        /// <param name="endDate">Ending date for the calculation.</param>
        /// <returns>Number of nanosecond boundaries crossed between the dates.</returns>
        public static int DateDiffNanosecond(
            [CanBeNull] this DbFunctions _,
            DateTimeOffset startDate,
            DateTimeOffset endDate)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffNanosecond)));

        /// <summary>
        ///     Counts the number of nanosecond boundaries crossed between the startDate and endDate.
        ///     Corresponds to `TIMESTAMPDIFF(MICROSECOND,startDate,endDate) * 1000`.
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="startDate">Starting date for the calculation.</param>
        /// <param name="endDate">Ending date for the calculation.</param>
        /// <returns>Number of nanosecond boundaries crossed between the dates.</returns>
        public static int? DateDiffNanosecond(
            [CanBeNull] this DbFunctions _,
            DateTimeOffset? startDate,
            DateTimeOffset? endDate)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffNanosecond)));

        /// <summary>
        ///     Counts the number of nanosecond boundaries crossed between the startDate and endDate.
        ///     Corresponds to `TIMESTAMPDIFF(MICROSECOND,startDate,endDate) * 1000`.
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="startDate">Starting date for the calculation.</param>
        /// <param name="endDate">Ending date for the calculation.</param>
        /// <returns>Number of nanosecond boundaries crossed between the dates.</returns>
        public static int DateDiffNanosecond(
            [CanBeNull] this DbFunctions _,
            DateOnly startDate,
            DateOnly endDate)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffNanosecond)));

        /// <summary>
        ///     Counts the number of nanosecond boundaries crossed between the startDate and endDate.
        ///     Corresponds to `TIMESTAMPDIFF(MICROSECOND,startDate,endDate) * 1000`.
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="startDate">Starting date for the calculation.</param>
        /// <param name="endDate">Ending date for the calculation.</param>
        /// <returns>Number of nanosecond boundaries crossed between the dates.</returns>
        public static int? DateDiffNanosecond(
            [CanBeNull] this DbFunctions _,
            DateOnly? startDate,
            DateOnly? endDate)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffNanosecond)));

        #endregion DateDiffNanosecond

        #region Like

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
        /// <param name="matchExpression">The property of entity that is to be matched.</param>
        /// <param name="pattern">
        ///     The pattern which may involve the wildcards `%` and `_`. The character `\` is used to escape wildcards and itself.
        /// </param>
        /// <returns>true if there is a match.</returns>
        public static bool Like<T>(
            [CanBeNull] this DbFunctions _,
            [CanBeNull] T matchExpression,
            [CanBeNull] string pattern)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(Like)));

        /// <summary>
        ///     <para>
        ///         An implementation of the SQL LIKE operation. On relational databases this is usually directly
        ///         translated to SQL.
        ///     </para>
        ///     <para>
        ///         Note that if this function is translated into SQL, then the semantics of the comparison will
        ///         depend on the database configuration. In particular, it may be either case-sensitive or
        ///         case-insensitive.
        ///     </para>
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="matchExpression">The property of entity that is to be matched.</param>
        /// <param name="pattern">The pattern which may involve the wildcards `%` and `_`.</param>
        /// <param name="escapeCharacter">
        ///     The escape character (as a single character string) to use in front of `%` and `_` (if they are not used as wildcards), and
        ///     itself.
        /// </param>
        /// <returns>true if there is a match.</returns>
        public static bool Like<T>(
            [CanBeNull] this DbFunctions _,
            [CanBeNull] T matchExpression,
            [CanBeNull] string pattern,
            [CanBeNull] string escapeCharacter)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(Like)));

        #endregion Like

        #region Match

        /// <summary>
        ///     <para>
        ///         An implementation of the SQL MATCH operation for Full Text search.
        ///     </para>
        ///     <para>
        ///         The semantics of the comparison will depend on the database configuration.
        ///         In particular, it may be either case-sensitive or case-insensitive.
        ///     </para>
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="property">The property of entity that is to be matched.</param>
        /// <param name="pattern">The pattern against which Full Text search is performed</param>
        /// <param name="searchMode">The mode to perform the search with.</param>
        /// <returns>true if there is a match.</returns>
        /// <exception cref="InvalidOperationException">Throws when query switched to client-evaluation.</exception>
        public static bool IsMatch(
            [CanBeNull] this DbFunctions _,
            [CanBeNull] string property,
            [CanBeNull] string pattern,
            XGMatchSearchMode searchMode)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(IsMatch)));

        /// <summary>
        ///     <para>
        ///         An implementation of the SQL MATCH operation for Full Text search.
        ///     </para>
        ///     <para>
        ///         The semantics of the comparison will depend on the database configuration.
        ///         In particular, it may be either case-sensitive or case-insensitive.
        ///     </para>
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="properties">The propertys of entity that is to be matched.</param>
        /// <param name="pattern">The pattern against which Full Text search is performed</param>
        /// <param name="searchMode">The mode to perform the search with.</param>
        /// <returns>true if there is a match.</returns>
        /// <exception cref="InvalidOperationException">Throws when query switched to client-evaluation.</exception>
        public static bool IsMatch(
            [CanBeNull] this DbFunctions _,
            [NotNull] string[] properties,
            [CanBeNull] string pattern,
            XGMatchSearchMode searchMode)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(IsMatch)));

        /// <summary>
        ///     <para>
        ///         An implementation of the SQL MATCH operation for Full Text search.
        ///     </para>
        ///     <para>
        ///         The semantics of the comparison will depend on the database configuration.
        ///         In particular, it may be either case-sensitive or case-insensitive.
        ///     </para>
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="property">The property of entity that is to be matched.</param>
        /// <param name="pattern">The pattern against which Full Text search is performed</param>
        /// <param name="searchMode">The mode to perform the search with. Needs to be a constant value or throws otherwise.</param>
        /// <returns>The relevance value of the match.</returns>
        /// <exception cref="InvalidOperationException">Throws when query switched to client-evaluation.</exception>
        public static double Match(
            [CanBeNull] this DbFunctions _,
            [CanBeNull] string property,
            [CanBeNull] string pattern,
            XGMatchSearchMode searchMode)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(Match)));

        /// <summary>
        ///     <para>
        ///         An implementation of the SQL MATCH operation for Full Text search.
        ///     </para>
        ///     <para>
        ///         The semantics of the comparison will depend on the database configuration.
        ///         In particular, it may be either case-sensitive or case-insensitive.
        ///     </para>
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="properties">The propertys of entity that is to be matched.</param>
        /// <param name="pattern">The pattern against which Full Text search is performed</param>
        /// <param name="searchMode">The mode to perform the search with. Needs to be a constant value or throws otherwise.</param>
        /// <returns>The relevance value of the match.</returns>
        /// <exception cref="InvalidOperationException">Throws when query switched to client-evaluation.</exception>
        public static double Match(
            [CanBeNull] this DbFunctions _,
            [NotNull] string[] properties,
            [CanBeNull] string pattern,
            XGMatchSearchMode searchMode)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(Match)));

        #endregion Match

        #region Misc

        /// <summary>
        ///     <para>
        ///         For a string argument `value`, Hex() returns a hexadecimal string representation of `value` where
        ///         each byte of each character in `value` is converted to two hexadecimal digits.
        ///     </para>
        ///     <para>
        ///         For a numeric argument `value`, Hex() returns a hexadecimal string representation of `value`
        ///         treated as a `Int64` (BIGINT) number.
        ///     </para>
        ///     <para>
        ///
        ///     </para>
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="value">The string or number to convert to a hexadecimal string.</param>
        /// <returns>The hexadecimal string or `null`.</returns>
        public static string Hex<T>(
            [CanBeNull] this DbFunctions _,
            [CanBeNull] T value)
         => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(Hex)));

        /// <summary>
        /// For a string argument `value`, Unhex() interprets each pair of characters in the argument as a hexadecimal
        /// number and converts it to the byte represented by the number.
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="value">The hexadecimal string to convert to a character string.</param>
        /// <returns>The string or `null`.</returns>
        public static string Unhex(
            [CanBeNull] this DbFunctions _,
            [CanBeNull] string value)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(Hex)));

        /// <summary>
        /// Converts radians to degrees.
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="radians">The value in radians.</param>
        /// <returns>The value in degrees.</returns>
        public static double Degrees(
            this DbFunctions _,
            double radians)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(Degrees)));

        /// <summary>
        /// Converts radians to degrees.
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="radians">The value in radians.</param>
        /// <returns>The value in degrees.</returns>
        public static float Degrees(
            this DbFunctions _,
            float radians)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(Degrees)));

        /// <summary>
        /// Converts degrees to radians.
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="degrees">The value in degrees.</param>
        /// <returns>The value in radians.</returns>
        public static double Radians(
            this DbFunctions _,
            double degrees)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(Radians)));

        /// <summary>
        /// Converts degrees to radians.
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="degrees">The value in degrees.</param>
        /// <returns>The value in radians.</returns>
        public static float Radians(
            this DbFunctions _,
            float degrees)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(Radians)));

        #endregion Misc
    }
}
