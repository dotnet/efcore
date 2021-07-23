// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Query;

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
        ///         A DbFunction method stub that can be used in LINQ queries to target the SQL Server <c>FREETEXT</c> store function.
        ///     </para>
        /// </summary>
        /// <remarks>
        ///     This DbFunction method has no in-memory implementation and will throw if the query switches to client-evaluation.
        ///     This can happen if the query contains one or more expressions that could not be translated to the store.
        /// </remarks>
        /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
        /// <param name="propertyReference">The property on which the search will be performed.</param>
        /// <param name="freeText">The text that will be searched for in the property.</param>
        /// <param name="languageTerm">A Language ID from the <c>sys.syslanguages</c> table.</param>
        public static bool FreeText(
            this DbFunctions _,
            object propertyReference,
            string freeText,
            [NotParameterized] int languageTerm)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(FreeText)));

        /// <summary>
        ///     <para>
        ///         A DbFunction method stub that can be used in LINQ queries to target the SQL Server <c>FREETEXT</c> store function.
        ///     </para>
        /// </summary>
        /// <remarks>
        ///     This DbFunction method has no in-memory implementation and will throw if the query switches to client-evaluation.
        ///     This can happen if the query contains one or more expressions that could not be translated to the store.
        /// </remarks>
        /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
        /// <param name="propertyReference">The property on which the search will be performed.</param>
        /// <param name="freeText">The text that will be searched for in the property.</param>
        public static bool FreeText(
            this DbFunctions _,
            object propertyReference,
            string freeText)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(FreeText)));

        /// <summary>
        ///     <para>
        ///         A DbFunction method stub that can be used in LINQ queries to target the SQL Server <c>CONTAINS</c> store function.
        ///     </para>
        /// </summary>
        /// <remarks>
        ///     This DbFunction method has no in-memory implementation and will throw if the query switches to client-evaluation.
        ///     This can happen if the query contains one or more expressions that could not be translated to the store.
        /// </remarks>
        /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
        /// <param name="propertyReference">The property on which the search will be performed.</param>
        /// <param name="searchCondition">The text that will be searched for in the property and the condition for a match.</param>
        /// <param name="languageTerm">A Language ID from the <c>sys.syslanguages</c> table.</param>
        public static bool Contains(
            this DbFunctions _,
            object propertyReference,
            string searchCondition,
            [NotParameterized] int languageTerm)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(Contains)));

        /// <summary>
        ///     <para>
        ///         A DbFunction method stub that can be used in LINQ queries to target the SQL Server <c>CONTAINS</c> store function.
        ///     </para>
        /// </summary>
        /// <remarks>
        ///     This DbFunction method has no in-memory implementation and will throw if the query switches to client-evaluation.
        ///     This can happen if the query contains one or more expressions that could not be translated to the store.
        /// </remarks>
        /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
        /// <param name="propertyReference">The property on which the search will be performed.</param>
        /// <param name="searchCondition">The text that will be searched for in the property and the condition for a match.</param>
        public static bool Contains(
            this DbFunctions _,
            object propertyReference,
            string searchCondition)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(Contains)));

        /// <summary>
        ///     Counts the number of year boundaries crossed between the <paramref name="startDate" /> and <paramref name="endDate" />.
        ///     Corresponds to SQL Server's <c>DATEDIFF(year, @startDate, @endDate)</c>.
        /// </summary>
        /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
        /// <param name="startDate">Starting date for the calculation.</param>
        /// <param name="endDate">Ending date for the calculation.</param>
        /// <returns>Number of year boundaries crossed between the dates.</returns>
        public static int DateDiffYear(
            this DbFunctions _,
            DateTime startDate,
            DateTime endDate)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffYear)));

        /// <summary>
        ///     Counts the number of year boundaries crossed between <paramref name="startDate" /> and <paramref name="endDate" />.
        ///     Corresponds to SQL Server's <c>DATEDIFF(year, @startDate, @endDate)</c>.
        /// </summary>
        /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
        /// <param name="startDate">Starting date for the calculation.</param>
        /// <param name="endDate">Ending date for the calculation.</param>
        /// <returns>Number of year boundaries crossed between the dates.</returns>
        public static int? DateDiffYear(
            this DbFunctions _,
            DateTime? startDate,
            DateTime? endDate)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffYear)));

        /// <summary>
        ///     Counts the number of year boundaries crossed between the <paramref name="startDate" /> and <paramref name="endDate" />.
        ///     Corresponds to SQL Server's <c>DATEDIFF(year, @startDate, @endDate)</c>.
        /// </summary>
        /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
        /// <param name="startDate">Starting date for the calculation.</param>
        /// <param name="endDate">Ending date for the calculation.</param>
        /// <returns>Number of year boundaries crossed between the dates.</returns>
        public static int DateDiffYear(
            this DbFunctions _,
            DateTimeOffset startDate,
            DateTimeOffset endDate)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffYear)));

        /// <summary>
        ///     Counts the number of year boundaries crossed between the <paramref name="startDate" /> and <paramref name="endDate" />.
        ///     Corresponds to SQL Server's <c>DATEDIFF(year, @startDate, @endDate)</c>.
        /// </summary>
        /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
        /// <param name="startDate">Starting date for the calculation.</param>
        /// <param name="endDate">Ending date for the calculation.</param>
        /// <returns>Number of year boundaries crossed between the dates.</returns>
        public static int? DateDiffYear(
            this DbFunctions _,
            DateTimeOffset? startDate,
            DateTimeOffset? endDate)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffYear)));

        /// <summary>
        ///     Counts the number of month boundaries crossed between the <paramref name="startDate" /> and <paramref name="endDate" />.
        ///     Corresponds to SQL Server's <c>DATEDIFF(month, @startDate, @endDate)</c>.
        /// </summary>
        /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
        /// <param name="startDate">Starting date for the calculation.</param>
        /// <param name="endDate">Ending date for the calculation.</param>
        /// <returns>Number of month boundaries crossed between the dates.</returns>
        public static int DateDiffMonth(
            this DbFunctions _,
            DateTime startDate,
            DateTime endDate)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffMonth)));

        /// <summary>
        ///     Counts the number of month boundaries crossed between the <paramref name="startDate" /> and <paramref name="endDate" />.
        ///     Corresponds to SQL Server's <c>DATEDIFF(month, @startDate, @endDate)</c>.
        /// </summary>
        /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
        /// <param name="startDate">Starting date for the calculation.</param>
        /// <param name="endDate">Ending date for the calculation.</param>
        /// <returns>Number of month boundaries crossed between the dates.</returns>
        public static int? DateDiffMonth(
            this DbFunctions _,
            DateTime? startDate,
            DateTime? endDate)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffMonth)));

        /// <summary>
        ///     Counts the number of month boundaries crossed between the <paramref name="startDate" /> and <paramref name="endDate" />.
        ///     Corresponds to SQL Server's <c>DATEDIFF(month, @startDate, @endDate)</c>.
        /// </summary>
        /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
        /// <param name="startDate">Starting date for the calculation.</param>
        /// <param name="endDate">Ending date for the calculation.</param>
        /// <returns>Number of month boundaries crossed between the dates.</returns>
        public static int DateDiffMonth(
            this DbFunctions _,
            DateTimeOffset startDate,
            DateTimeOffset endDate)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffMonth)));

        /// <summary>
        ///     Counts the number of month boundaries crossed between the <paramref name="startDate" /> and <paramref name="endDate" />.
        ///     Corresponds to SQL Server's <c>DATEDIFF(month, @startDate, @endDate)</c>.
        /// </summary>
        /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
        /// <param name="startDate">Starting date for the calculation.</param>
        /// <param name="endDate">Ending date for the calculation.</param>
        /// <returns>Number of month boundaries crossed between the dates.</returns>
        public static int? DateDiffMonth(
            this DbFunctions _,
            DateTimeOffset? startDate,
            DateTimeOffset? endDate)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffMonth)));

        /// <summary>
        ///     Counts the number of day boundaries crossed between the <paramref name="startDate" /> and <paramref name="endDate" />.
        ///     Corresponds to SQL Server's <c>DATEDIFF(day, @startDate, @endDate)</c>.
        /// </summary>
        /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
        /// <param name="startDate">Starting date for the calculation.</param>
        /// <param name="endDate">Ending date for the calculation.</param>
        /// <returns>Number of day boundaries crossed between the dates.</returns>
        public static int DateDiffDay(
            this DbFunctions _,
            DateTime startDate,
            DateTime endDate)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffDay)));

        /// <summary>
        ///     Counts the number of day boundaries crossed between the <paramref name="startDate" /> and <paramref name="endDate" />.
        ///     Corresponds to SQL Server's <c>DATEDIFF(day, @startDate, @endDate)</c>.
        /// </summary>
        /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
        /// <param name="startDate">Starting date for the calculation.</param>
        /// <param name="endDate">Ending date for the calculation.</param>
        /// <returns>Number of day boundaries crossed between the dates.</returns>
        public static int? DateDiffDay(
            this DbFunctions _,
            DateTime? startDate,
            DateTime? endDate)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffDay)));

        /// <summary>
        ///     Counts the number of day boundaries crossed between the <paramref name="startDate" /> and <paramref name="endDate" />.
        ///     Corresponds to SQL Server's <c>DATEDIFF(day, @startDate, @endDate)</c>.
        /// </summary>
        /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
        /// <param name="startDate">Starting date for the calculation.</param>
        /// <param name="endDate">Ending date for the calculation.</param>
        /// <returns>Number of day boundaries crossed between the dates.</returns>
        public static int DateDiffDay(
            this DbFunctions _,
            DateTimeOffset startDate,
            DateTimeOffset endDate)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffDay)));

        /// <summary>
        ///     Counts the number of day boundaries crossed between the <paramref name="startDate" /> and <paramref name="endDate" />.
        ///     Corresponds to SQL Server's <c>DATEDIFF(day, @startDate, @endDate)</c>.
        /// </summary>
        /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
        /// <param name="startDate">Starting date for the calculation.</param>
        /// <param name="endDate">Ending date for the calculation.</param>
        /// <returns>Number of day boundaries crossed between the dates.</returns>
        public static int? DateDiffDay(
            this DbFunctions _,
            DateTimeOffset? startDate,
            DateTimeOffset? endDate)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffDay)));

        /// <summary>
        ///     Counts the number of hour boundaries crossed between the <paramref name="startDate" /> and <paramref name="endDate" />.
        ///     Corresponds to SQL Server's <c>DATEDIFF(hour, @startDate, @endDate)</c>.
        /// </summary>
        /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
        /// <param name="startDate">Starting date for the calculation.</param>
        /// <param name="endDate">Ending date for the calculation.</param>
        /// <returns>Number of hour boundaries crossed between the dates.</returns>
        public static int DateDiffHour(
            this DbFunctions _,
            DateTime startDate,
            DateTime endDate)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffHour)));

        /// <summary>
        ///     Counts the number of hour boundaries crossed between the <paramref name="startDate" /> and <paramref name="endDate" />.
        ///     Corresponds to SQL Server's <c>DATEDIFF(hour, @startDate, @endDate)</c>.
        /// </summary>
        /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
        /// <param name="startDate">Starting date for the calculation.</param>
        /// <param name="endDate">Ending date for the calculation.</param>
        /// <returns>Number of hour boundaries crossed between the dates.</returns>
        public static int? DateDiffHour(
            this DbFunctions _,
            DateTime? startDate,
            DateTime? endDate)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffHour)));

        /// <summary>
        ///     Counts the number of hour boundaries crossed between the <paramref name="startDate" /> and <paramref name="endDate" />.
        ///     Corresponds to SQL Server's <c>DATEDIFF(hour, @startDate, @endDate)</c>.
        /// </summary>
        /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
        /// <param name="startDate">Starting date for the calculation.</param>
        /// <param name="endDate">Ending date for the calculation.</param>
        /// <returns>Number of hour boundaries crossed between the dates.</returns>
        public static int DateDiffHour(
            this DbFunctions _,
            DateTimeOffset startDate,
            DateTimeOffset endDate)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffHour)));

        /// <summary>
        ///     Counts the number of hour boundaries crossed between the <paramref name="startDate" /> and <paramref name="endDate" />.
        ///     Corresponds to SQL Server's <c>DATEDIFF(hour, @startDate, @endDate)</c>.
        /// </summary>
        /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
        /// <param name="startDate">Starting date for the calculation.</param>
        /// <param name="endDate">Ending date for the calculation.</param>
        /// <returns>Number of hour boundaries crossed between the dates.</returns>
        public static int? DateDiffHour(
            this DbFunctions _,
            DateTimeOffset? startDate,
            DateTimeOffset? endDate)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffHour)));

        /// <summary>
        ///     Counts the number of hour boundaries crossed between the <paramref name="startTimeSpan" /> and
        ///     <paramref name="endTimeSpan" />. Corresponds to SQL Server's <c>DATEDIFF(hour, @startDate, @endDate)</c>.
        /// </summary>
        /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
        /// <param name="startTimeSpan">Starting timespan for the calculation.</param>
        /// <param name="endTimeSpan">Ending timespan for the calculation.</param>
        /// <returns>Number of hour boundaries crossed between the timespans.</returns>
        public static int DateDiffHour(
            this DbFunctions _,
            TimeSpan startTimeSpan,
            TimeSpan endTimeSpan)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffHour)));

        /// <summary>
        ///     Counts the number of hour boundaries crossed between the <paramref name="startTimeSpan" /> and
        ///     <paramref name="endTimeSpan" />. Corresponds to SQL Server's <c>DATEDIFF(hour, @startDate, @endDate)</c>.
        /// </summary>
        /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
        /// <param name="startTimeSpan">Starting timespan for the calculation.</param>
        /// <param name="endTimeSpan">Ending timespan for the calculation.</param>
        /// <returns>Number of hour boundaries crossed between the timespans.</returns>
        public static int? DateDiffHour(
            this DbFunctions _,
            TimeSpan? startTimeSpan,
            TimeSpan? endTimeSpan)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffHour)));

        /// <summary>
        ///     Counts the number of minute boundaries crossed between the <paramref name="startDate" /> and <paramref name="endDate" />.
        ///     Corresponds to SQL Server's <c>DATEDIFF(minute, @startDate, @endDate)</c>.
        /// </summary>
        /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
        /// <param name="startDate">Starting date for the calculation.</param>
        /// <param name="endDate">Ending date for the calculation.</param>
        /// <returns>Number of minute boundaries crossed between the dates.</returns>
        public static int DateDiffMinute(
            this DbFunctions _,
            DateTime startDate,
            DateTime endDate)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffMinute)));

        /// <summary>
        ///     Counts the number of minute boundaries crossed between the <paramref name="startDate" /> and <paramref name="endDate" />.
        ///     Corresponds to SQL Server's <c>DATEDIFF(minute, @startDate, @endDate)</c>.
        /// </summary>
        /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
        /// <param name="startDate">Starting date for the calculation.</param>
        /// <param name="endDate">Ending date for the calculation.</param>
        /// <returns>Number of minute boundaries crossed between the dates.</returns>
        public static int? DateDiffMinute(
            this DbFunctions _,
            DateTime? startDate,
            DateTime? endDate)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffMinute)));

        /// <summary>
        ///     Counts the number of minute boundaries crossed between the <paramref name="startDate" /> and <paramref name="endDate" />.
        ///     Corresponds to SQL Server's <c>DATEDIFF(minute, @startDate, @endDate)</c>.
        /// </summary>
        /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
        /// <param name="startDate">Starting date for the calculation.</param>
        /// <param name="endDate">Ending date for the calculation.</param>
        /// <returns>Number of minute boundaries crossed between the dates.</returns>
        public static int DateDiffMinute(
            this DbFunctions _,
            DateTimeOffset startDate,
            DateTimeOffset endDate)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffMinute)));

        /// <summary>
        ///     Counts the number of minute boundaries crossed between the <paramref name="startDate" /> and <paramref name="endDate" />.
        ///     Corresponds to SQL Server's <c>DATEDIFF(minute, @startDate, @endDate)</c>.
        /// </summary>
        /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
        /// <param name="startDate">Starting date for the calculation.</param>
        /// <param name="endDate">Ending date for the calculation.</param>
        /// <returns>Number of minute boundaries crossed between the dates.</returns>
        public static int? DateDiffMinute(
            this DbFunctions _,
            DateTimeOffset? startDate,
            DateTimeOffset? endDate)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffMinute)));

        /// <summary>
        ///     Counts the number of minute boundaries crossed between the <paramref name="startTimeSpan" /> and
        ///     <paramref name="endTimeSpan" />. Corresponds to SQL Server's <c>DATEDIFF(minute, @startDate, @endDate)</c>.
        /// </summary>
        /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
        /// <param name="startTimeSpan">Starting timespan for the calculation.</param>
        /// <param name="endTimeSpan">Ending timespan for the calculation.</param>
        /// <returns>Number of minute boundaries crossed between the timespans.</returns>
        public static int DateDiffMinute(
            this DbFunctions _,
            TimeSpan startTimeSpan,
            TimeSpan endTimeSpan)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffMinute)));

        /// <summary>
        ///     Counts the number of minute boundaries crossed between the <paramref name="startTimeSpan" /> and
        ///     <paramref name="endTimeSpan" />. Corresponds to SQL Server's <c>DATEDIFF(minute, @startDate, @endDate)</c>.
        /// </summary>
        /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
        /// <param name="startTimeSpan">Starting timespan for the calculation.</param>
        /// <param name="endTimeSpan">Ending timespan for the calculation.</param>
        /// <returns>Number of minute boundaries crossed between the timespans.</returns>
        public static int? DateDiffMinute(
            this DbFunctions _,
            TimeSpan? startTimeSpan,
            TimeSpan? endTimeSpan)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffMinute)));

        /// <summary>
        ///     Counts the number of second boundaries crossed between the <paramref name="startDate" /> and <paramref name="endDate" />.
        ///     Corresponds to SQL Server's <c>DATEDIFF(second, @startDate, @endDate)</c>.
        /// </summary>
        /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
        /// <param name="startDate">Starting date for the calculation.</param>
        /// <param name="endDate">Ending date for the calculation.</param>
        /// <returns>Number of second boundaries crossed between the dates.</returns>
        public static int DateDiffSecond(
            this DbFunctions _,
            DateTime startDate,
            DateTime endDate)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffSecond)));

        /// <summary>
        ///     Counts the number of second boundaries crossed between the <paramref name="startDate" /> and <paramref name="endDate" />.
        ///     Corresponds to SQL Server's <c>DATEDIFF(second, @startDate, @endDate)</c>.
        /// </summary>
        /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
        /// <param name="startDate">Starting date for the calculation.</param>
        /// <param name="endDate">Ending date for the calculation.</param>
        /// <returns>Number of second boundaries crossed between the dates.</returns>
        public static int? DateDiffSecond(
            this DbFunctions _,
            DateTime? startDate,
            DateTime? endDate)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffSecond)));

        /// <summary>
        ///     Counts the number of second boundaries crossed between the <paramref name="startDate" /> and <paramref name="endDate" />.
        ///     Corresponds to SQL Server's <c>DATEDIFF(second, @startDate, @endDate)</c>.
        /// </summary>
        /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
        /// <param name="startDate">Starting date for the calculation.</param>
        /// <param name="endDate">Ending date for the calculation.</param>
        /// <returns>Number of second boundaries crossed between the dates.</returns>
        public static int DateDiffSecond(
            this DbFunctions _,
            DateTimeOffset startDate,
            DateTimeOffset endDate)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffSecond)));

        /// <summary>
        ///     Counts the number of second boundaries crossed between the <paramref name="startDate" /> and <paramref name="endDate" />.
        ///     Corresponds to SQL Server's <c>DATEDIFF(second, @startDate, @endDate)</c>.
        /// </summary>
        /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
        /// <param name="startDate">Starting date for the calculation.</param>
        /// <param name="endDate">Ending date for the calculation.</param>
        /// <returns>Number of second boundaries crossed between the dates.</returns>
        public static int? DateDiffSecond(
            this DbFunctions _,
            DateTimeOffset? startDate,
            DateTimeOffset? endDate)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffSecond)));

        /// <summary>
        ///     Counts the number of second boundaries crossed between the <paramref name="startTimeSpan" /> and
        ///     <paramref name="endTimeSpan" />. Corresponds to SQL Server's <c>DATEDIFF(second, @startDate, @endDate)</c>.
        /// </summary>
        /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
        /// <param name="startTimeSpan">Starting timespan for the calculation.</param>
        /// <param name="endTimeSpan">Ending timespan for the calculation.</param>
        /// <returns>Number of second boundaries crossed between the timespans.</returns>
        public static int DateDiffSecond(
            this DbFunctions _,
            TimeSpan startTimeSpan,
            TimeSpan endTimeSpan)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffSecond)));

        /// <summary>
        ///     Counts the number of second boundaries crossed between the <paramref name="startTimeSpan" /> and
        ///     <paramref name="endTimeSpan" />. Corresponds to SQL Server's <c>DATEDIFF(second, @startDate, @endDate)</c>.
        /// </summary>
        /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
        /// <param name="startTimeSpan">Starting timespan for the calculation.</param>
        /// <param name="endTimeSpan">Ending timespan for the calculation.</param>
        /// <returns>Number of second boundaries crossed between the timespans.</returns>
        public static int? DateDiffSecond(
            this DbFunctions _,
            TimeSpan? startTimeSpan,
            TimeSpan? endTimeSpan)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffSecond)));

        /// <summary>
        ///     Counts the number of millisecond boundaries crossed between the <paramref name="startDate" /> and
        ///     <paramref name="endDate" />. Corresponds to SQL Server's <c>DATEDIFF(millisecond, @startDate, @endDate)</c>.
        /// </summary>
        /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
        /// <param name="startDate">Starting date for the calculation.</param>
        /// <param name="endDate">Ending date for the calculation.</param>
        /// <returns>Number of millisecond boundaries crossed between the dates.</returns>
        public static int DateDiffMillisecond(
            this DbFunctions _,
            DateTime startDate,
            DateTime endDate)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffMillisecond)));

        /// <summary>
        ///     Counts the number of millisecond boundaries crossed between the <paramref name="startDate" /> and
        ///     <paramref name="endDate" />. Corresponds to SQL Server's <c>DATEDIFF(millisecond, @startDate, @endDate)</c>.
        /// </summary>
        /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
        /// <param name="startDate">Starting date for the calculation.</param>
        /// <param name="endDate">Ending date for the calculation.</param>
        /// <returns>Number of millisecond boundaries crossed between the dates.</returns>
        public static int? DateDiffMillisecond(
            this DbFunctions _,
            DateTime? startDate,
            DateTime? endDate)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffMillisecond)));

        /// <summary>
        ///     Counts the number of millisecond boundaries crossed between the <paramref name="startDate" /> and
        ///     <paramref name="endDate" />. Corresponds to SQL Server's <c>DATEDIFF(millisecond, @startDate, @endDate)</c>.
        /// </summary>
        /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
        /// <param name="startDate">Starting date for the calculation.</param>
        /// <param name="endDate">Ending date for the calculation.</param>
        /// <returns>Number of millisecond boundaries crossed between the dates.</returns>
        public static int DateDiffMillisecond(
            this DbFunctions _,
            DateTimeOffset startDate,
            DateTimeOffset endDate)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffMillisecond)));

        /// <summary>
        ///     Counts the number of millisecond boundaries crossed between the <paramref name="startDate" /> and
        ///     <paramref name="endDate" />. Corresponds to SQL Server's <c>DATEDIFF(millisecond, @startDate, @endDate)</c>.
        /// </summary>
        /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
        /// <param name="startDate">Starting date for the calculation.</param>
        /// <param name="endDate">Ending date for the calculation.</param>
        /// <returns>Number of millisecond boundaries crossed between the dates.</returns>
        public static int? DateDiffMillisecond(
            this DbFunctions _,
            DateTimeOffset? startDate,
            DateTimeOffset? endDate)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffMillisecond)));

        /// <summary>
        ///     Counts the number of millisecond boundaries crossed between the <paramref name="startTimeSpan" /> and
        ///     <paramref name="endTimeSpan" />. Corresponds to SQL Server's <c>DATEDIFF(millisecond, @startDate, @endDate)</c>.
        /// </summary>
        /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
        /// <param name="startTimeSpan">Starting timespan for the calculation.</param>
        /// <param name="endTimeSpan">Ending timespan for the calculation.</param>
        /// <returns>Number of millisecond boundaries crossed between the timespans.</returns>
        public static int DateDiffMillisecond(
            this DbFunctions _,
            TimeSpan startTimeSpan,
            TimeSpan endTimeSpan)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffMillisecond)));

        /// <summary>
        ///     Counts the number of millisecond boundaries crossed between the <paramref name="startTimeSpan" /> and
        ///     <paramref name="endTimeSpan" />. Corresponds to SQL Server's <c>DATEDIFF(millisecond, @startDate, @endDate)</c>.
        /// </summary>
        /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
        /// <param name="startTimeSpan">Starting timespan for the calculation.</param>
        /// <param name="endTimeSpan">Ending timespan for the calculation.</param>
        /// <returns>Number of millisecond boundaries crossed between the timespans.</returns>
        public static int? DateDiffMillisecond(
            this DbFunctions _,
            TimeSpan? startTimeSpan,
            TimeSpan? endTimeSpan)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffMillisecond)));

        /// <summary>
        ///     Counts the number of microsecond boundaries crossed between the <paramref name="startDate" /> and
        ///     <paramref name="endDate" />. Corresponds to SQL Server's <c>DATEDIFF(microsecond, @startDate, @endDate)</c>.
        /// </summary>
        /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
        /// <param name="startDate">Starting date for the calculation.</param>
        /// <param name="endDate">Ending date for the calculation.</param>
        /// <returns>Number of microsecond boundaries crossed between the dates.</returns>
        public static int DateDiffMicrosecond(
            this DbFunctions _,
            DateTime startDate,
            DateTime endDate)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffMicrosecond)));

        /// <summary>
        ///     Counts the number of microsecond boundaries crossed between the <paramref name="startDate" /> and
        ///     <paramref name="endDate" />. Corresponds to SQL Server's <c>DATEDIFF(microsecond, @startDate, @endDate)</c>.
        /// </summary>
        /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
        /// <param name="startDate">Starting date for the calculation.</param>
        /// <param name="endDate">Ending date for the calculation.</param>
        /// <returns>Number of microsecond boundaries crossed between the dates.</returns>
        public static int? DateDiffMicrosecond(
            this DbFunctions _,
            DateTime? startDate,
            DateTime? endDate)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffMicrosecond)));

        /// <summary>
        ///     Counts the number of microsecond boundaries crossed between the <paramref name="startDate" /> and
        ///     <paramref name="endDate" />. Corresponds to SQL Server's <c>DATEDIFF(microsecond, @startDate, @endDate)</c>.
        /// </summary>
        /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
        /// <param name="startDate">Starting date for the calculation.</param>
        /// <param name="endDate">Ending date for the calculation.</param>
        /// <returns>Number of microsecond boundaries crossed between the dates.</returns>
        public static int DateDiffMicrosecond(
            this DbFunctions _,
            DateTimeOffset startDate,
            DateTimeOffset endDate)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffMicrosecond)));

        /// <summary>
        ///     Counts the number of microsecond boundaries crossed between the <paramref name="startDate" /> and
        ///     <paramref name="endDate" />. Corresponds to SQL Server's <c>DATEDIFF(microsecond, @startDate, @endDate)</c>.
        /// </summary>
        /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
        /// <param name="startDate">Starting date for the calculation.</param>
        /// <param name="endDate">Ending date for the calculation.</param>
        /// <returns>Number of microsecond boundaries crossed between the dates.</returns>
        public static int? DateDiffMicrosecond(
            this DbFunctions _,
            DateTimeOffset? startDate,
            DateTimeOffset? endDate)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffMicrosecond)));

        /// <summary>
        ///     Counts the number of microsecond boundaries crossed between the <paramref name="startTimeSpan" /> and
        ///     <paramref name="endTimeSpan" />. Corresponds to SQL Server's <c>DATEDIFF(microsecond, @startDate, @endDate)</c>.
        /// </summary>
        /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
        /// <param name="startTimeSpan">Starting timespan for the calculation.</param>
        /// <param name="endTimeSpan">Ending timespan for the calculation.</param>
        /// <returns>Number of microsecond boundaries crossed between the timespans.</returns>
        public static int DateDiffMicrosecond(
            this DbFunctions _,
            TimeSpan startTimeSpan,
            TimeSpan endTimeSpan)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffMicrosecond)));

        /// <summary>
        ///     Counts the number of microsecond boundaries crossed between the <paramref name="startTimeSpan" /> and
        ///     <paramref name="endTimeSpan" />. Corresponds to SQL Server's <c>DATEDIFF(microsecond, @startDate, @endDate)</c>.
        /// </summary>
        /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
        /// <param name="startTimeSpan">Starting timespan for the calculation.</param>
        /// <param name="endTimeSpan">Ending timespan for the calculation.</param>
        /// <returns>Number of microsecond boundaries crossed between the timespans.</returns>
        public static int? DateDiffMicrosecond(
            this DbFunctions _,
            TimeSpan? startTimeSpan,
            TimeSpan? endTimeSpan)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffMicrosecond)));

        /// <summary>
        ///     Counts the number of nanosecond boundaries crossed between the <paramref name="startDate" /> and
        ///     <paramref name="endDate" />. Corresponds to SQL Server's <c>DATEDIFF(nanosecond, @startDate, @endDate)</c>.
        /// </summary>
        /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
        /// <param name="startDate">Starting date for the calculation.</param>
        /// <param name="endDate">Ending date for the calculation.</param>
        /// <returns>Number of nanosecond boundaries crossed between the dates.</returns>
        public static int DateDiffNanosecond(
            this DbFunctions _,
            DateTime startDate,
            DateTime endDate)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffNanosecond)));

        /// <summary>
        ///     Counts the number of nanosecond boundaries crossed between the <paramref name="startDate" /> and
        ///     <paramref name="endDate" />. Corresponds to SQL Server's <c>DATEDIFF(nanosecond, @startDate, @endDate)</c>.
        /// </summary>
        /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
        /// <param name="startDate">Starting date for the calculation.</param>
        /// <param name="endDate">Ending date for the calculation.</param>
        /// <returns>Number of nanosecond boundaries crossed between the dates.</returns>
        public static int? DateDiffNanosecond(
            this DbFunctions _,
            DateTime? startDate,
            DateTime? endDate)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffNanosecond)));

        /// <summary>
        ///     Counts the number of nanosecond boundaries crossed between the <paramref name="startDate" /> and
        ///     <paramref name="endDate" />. Corresponds to SQL Server's <c>DATEDIFF(nanosecond, @startDate, @endDate)</c>.
        /// </summary>
        /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
        /// <param name="startDate">Starting date for the calculation.</param>
        /// <param name="endDate">Ending date for the calculation.</param>
        /// <returns>Number of nanosecond boundaries crossed between the dates.</returns>
        public static int DateDiffNanosecond(
            this DbFunctions _,
            DateTimeOffset startDate,
            DateTimeOffset endDate)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffNanosecond)));

        /// <summary>
        ///     Counts the number of nanosecond boundaries crossed between the <paramref name="startDate" /> and
        ///     <paramref name="endDate" />. Corresponds to SQL Server's <c>DATEDIFF(nanosecond, @startDate, @endDate)</c>.
        /// </summary>
        /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
        /// <param name="startDate">Starting date for the calculation.</param>
        /// <param name="endDate">Ending date for the calculation.</param>
        /// <returns>Number of nanosecond boundaries crossed between the dates.</returns>
        public static int? DateDiffNanosecond(
            this DbFunctions _,
            DateTimeOffset? startDate,
            DateTimeOffset? endDate)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffNanosecond)));

        /// <summary>
        ///     Counts the number of nanosecond boundaries crossed between the <paramref name="startTimeSpan" /> and
        ///     <paramref name="endTimeSpan" />. Corresponds to SQL Server's <c>DATEDIFF(nanosecond, @startDate, @endDate)</c>.
        /// </summary>
        /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
        /// <param name="startTimeSpan">Starting timespan for the calculation.</param>
        /// <param name="endTimeSpan">Ending timespan for the calculation.</param>
        /// <returns>Number of nanosecond boundaries crossed between the dates.</returns>
        public static int DateDiffNanosecond(
            this DbFunctions _,
            TimeSpan startTimeSpan,
            TimeSpan endTimeSpan)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffNanosecond)));

        /// <summary>
        ///     Counts the number of nanosecond boundaries crossed between the <paramref name="startTimeSpan" /> and
        ///     <paramref name="endTimeSpan" />. Corresponds to SQL Server's <c>DATEDIFF(nanosecond, @startDate, @endDate)</c>.
        /// </summary>
        /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
        /// <param name="startTimeSpan">Starting timespan for the calculation.</param>
        /// <param name="endTimeSpan">Ending timespan for the calculation.</param>
        /// <returns>Number of nanosecond boundaries crossed between the dates.</returns>
        public static int? DateDiffNanosecond(
            this DbFunctions _,
            TimeSpan? startTimeSpan,
            TimeSpan? endTimeSpan)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffNanosecond)));

        /// <summary>
        ///     Counts the number of week boundaries crossed between the <paramref name="startDate" /> and <paramref name="endDate" />.
        ///     Corresponds to SQL Server's <c>DATEDIFF(week, @startDate, @endDate)</c>.
        /// </summary>
        /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
        /// <param name="startDate">Starting date for the calculation.</param>
        /// <param name="endDate">Ending date for the calculation.</param>
        /// <returns>Number of week boundaries crossed between the dates.</returns>
        public static int DateDiffWeek(
            this DbFunctions _,
            DateTime startDate,
            DateTime endDate)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffWeek)));

        /// <summary>
        ///     Counts the number of week boundaries crossed between the <paramref name="startDate" /> and <paramref name="endDate" />.
        ///     Corresponds to SQL Server's <c>DATEDIFF(week, @startDate, @endDate)</c>.
        /// </summary>
        /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
        /// <param name="startDate">Starting date for the calculation.</param>
        /// <param name="endDate">Ending date for the calculation.</param>
        /// <returns>Number of week boundaries crossed between the dates.</returns>
        public static int? DateDiffWeek(
            this DbFunctions _,
            DateTime? startDate,
            DateTime? endDate)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffWeek)));

        /// <summary>
        ///     Counts the number of week boundaries crossed between the <paramref name="startDate" /> and <paramref name="endDate" />.
        ///     Corresponds to SQL Server's <c>DATEDIFF(week, @startDate, @endDate)</c>.
        /// </summary>
        /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
        /// <param name="startDate">Starting date for the calculation.</param>
        /// <param name="endDate">Ending date for the calculation.</param>
        /// <returns>Number of week boundaries crossed between the dates.</returns>
        public static int DateDiffWeek(
            this DbFunctions _,
            DateTimeOffset startDate,
            DateTimeOffset endDate)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffWeek)));

        /// <summary>
        ///     Counts the number of week boundaries crossed between the <paramref name="startDate" /> and <paramref name="endDate" />.
        ///     Corresponds to SQL Server's <c>DATEDIFF(week, @startDate, @endDate)</c>.
        /// </summary>
        /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
        /// <param name="startDate">Starting date for the calculation.</param>
        /// <param name="endDate">Ending date for the calculation.</param>
        /// <returns>Number of week boundaries crossed between the dates.</returns>
        public static int? DateDiffWeek(
            this DbFunctions _,
            DateTimeOffset? startDate,
            DateTimeOffset? endDate)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffWeek)));

        /// <summary>
        ///     Validate if the given string is a valid date.
        ///     Corresponds to the SQL Server's <c>ISDATE('date')</c>.
        /// </summary>
        /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
        /// <param name="expression">Expression to validate</param>
        /// <returns>true for valid date and false otherwise.</returns>
        public static bool IsDate(
            this DbFunctions _,
            string expression)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(IsDate)));

        /// <summary>
        ///     Initializes a new instance of the <see cref="DateTime" /> structure to the specified year, month, day, hour, minute, second,
        ///     and millisecond.
        ///     Corresponds to the SQL Server's <c>DATETIMEFROMPARTS(year, month, day, hour, minute, second, millisecond)</c>.
        /// </summary>
        /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
        /// <param name="year">The year (1753 through 9999).</param>
        /// <param name="month">The month (1 through 12).</param>
        /// <param name="day">The day (1 through the number of days in month).</param>
        /// <param name="hour">The hours (0 through 23).</param>
        /// <param name="minute">The minutes (0 through 59).</param>
        /// <param name="second">The seconds (0 through 59).</param>
        /// <param name="millisecond">The milliseconds (0 through 999).</param>
        /// <returns>
        ///     New instance of the <see cref="DateTime" /> structure to the specified year, month, day, hour, minute, second, and
        ///     millisecond.
        /// </returns>
        public static DateTime DateTimeFromParts(
            this DbFunctions _,
            int year,
            int month,
            int day,
            int hour,
            int minute,
            int second,
            int millisecond)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateTimeFromParts)));

        /// <summary>
        ///     Initializes a new instance of the <see cref="DateTime" /> structure to the specified year, month, day.
        ///     Corresponds to the SQL Server's <c>DATEFROMPARTS(year, month, day)</c>.
        /// </summary>
        /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
        /// <param name="year">The year (1753 through 9999).</param>
        /// <param name="month">The month (1 through 12).</param>
        /// <param name="day">The day (1 through the number of days in month).</param>
        /// <returns>New instance of the <see cref="DateTime" /> structure to the specified year, month, day.</returns>
        public static DateTime DateFromParts(
            this DbFunctions _,
            int year,
            int month,
            int day)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateFromParts)));

        /// <summary>
        ///     Initializes a new instance of the <see cref="DateTime" /> structure to the specified year, month, day, hour, minute, second,
        ///     fractions, and precision.
        ///     Corresponds to the SQL Server's <c>DATETIME2FROMPARTS(year, month, day, hour, minute, seconds, fractions, precision)</c>.
        /// </summary>
        /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
        /// <param name="year">The year (1753 through 9999).</param>
        /// <param name="month">The month (1 through 12).</param>
        /// <param name="day">The day (1 through the number of days in month).</param>
        /// <param name="hour">The hours (0 through 23).</param>
        /// <param name="minute">The minutes (0 through 59).</param>
        /// <param name="second">The seconds (0 through 59).</param>
        /// <param name="fractions">The fractional seconds (0 through 9999999).</param>
        /// <param name="precision">The precision of the datetime2 value (0 through 7).</param>
        /// <returns>
        ///     New instance of the <see cref="DateTime" /> structure to the specified year, month, day, hour, minute, second, fractions,
        ///     and precision.
        /// </returns>
        public static DateTime DateTime2FromParts(
            this DbFunctions _,
            int year,
            int month,
            int day,
            int hour,
            int minute,
            int second,
            int fractions,
            int precision)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateTime2FromParts)));

        /// <summary>
        ///     Initializes a new instance of the <see cref="DateTimeOffset" /> structure to the specified year, month, day, hour, minute,
        ///     second, fractions, hourOffset, minuteOffset and precision.
        ///     Corresponds to the SQL Server's <c>DATETIMEOFFSETFROMPARTS(year, month, day, hour, minute, seconds, fractions, hour_offset,
        ///     minute_offset, precision)</c>.
        /// </summary>
        /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
        /// <param name="year">The year (1753 through 9999).</param>
        /// <param name="month">The month (1 through 12).</param>
        /// <param name="day">The day (1 through the number of days in month).</param>
        /// <param name="hour">The hours (0 through 23).</param>
        /// <param name="minute">The minutes (0 through 59).</param>
        /// <param name="second">The seconds (0 through 59).</param>
        /// <param name="fractions">The fractional seconds (0 through 9999999).</param>
        /// <param name="hourOffset">The hour portion of the time zone offset (-14 through +14).</param>
        /// <param name="minuteOffset">The minute portion of the time zone offset (0 or 30).</param>
        /// <param name="precision">The precision of the datetimeoffset value (0 through 7).</param>
        /// <returns>
        ///     New instance of the <see cref="DateTimeOffset" /> structure to the specified year, month, day, hour, minute, second,
        ///     fractions, hourOffset, minuteOffset and precision.
        /// </returns>
        public static DateTimeOffset DateTimeOffsetFromParts(
            this DbFunctions _,
            int year,
            int month,
            int day,
            int hour,
            int minute,
            int second,
            int fractions,
            int hourOffset,
            int minuteOffset,
            int precision)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateTimeOffsetFromParts)));

        /// <summary>
        ///     Initializes a new instance of the <see cref="DateTime" /> structure to the specified year, month, day, hour and minute.
        ///     Corresponds to the SQL Server's <c>SMALLDATETIMEFROMPARTS(year, month, day, hour, minute)</c>.
        /// </summary>
        /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
        /// <param name="year">The year (1753 through 9999).</param>
        /// <param name="month">The month (1 through 12).</param>
        /// <param name="day">The day (1 through the number of days in month).</param>
        /// <param name="hour">The hours (0 through 23).</param>
        /// <param name="minute">The minutes (0 through 59).</param>
        /// <returns>New instance of the <see cref="DateTime" /> structure to the specified year, month, day, hour and minute.</returns>
        public static DateTime SmallDateTimeFromParts(
            this DbFunctions _,
            int year,
            int month,
            int day,
            int hour,
            int minute)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(SmallDateTimeFromParts)));

        /// <summary>
        ///     Initializes a new instance of the <see cref="TimeSpan" /> structure to the specified hour, minute, second, fractions, and
        ///     precision. Corresponds to the SQL Server's <c>TIMEFROMPARTS(hour, minute, seconds, fractions, precision)</c>.
        /// </summary>
        /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
        /// <param name="hour">The hours (0 through 23).</param>
        /// <param name="minute">The minutes (0 through 59).</param>
        /// <param name="second">The seconds (0 through 59).</param>
        /// <param name="fractions">The fractional seconds (0 through 9999999).</param>
        /// <param name="precision">The precision of the time value (0 through 7).</param>
        /// <returns>
        ///     New instance of the <see cref="TimeSpan" /> structure to the specified hour, minute, second, fractions, and precision.
        /// </returns>
        public static TimeSpan TimeFromParts(
            this DbFunctions _,
            int hour,
            int minute,
            int second,
            int fractions,
            int precision)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(TimeFromParts)));

        /// <summary>
        ///     Returns the number of bytes used to represent any expression.
        /// </summary>
        /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
        /// <param name="arg">The value to be examined for data length.</param>
        /// <returns>The number of bytes in the input value.</returns>
        public static int? DataLength(
            this DbFunctions _,
            string? arg)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DataLength)));

        /// <summary>
        ///     Returns the number of bytes used to represent any expression.
        /// </summary>
        /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
        /// <param name="arg">The value to be examined for data length.</param>
        /// <returns>The number of bytes in the input value.</returns>
        public static int? DataLength(
            this DbFunctions _,
            bool? arg)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DataLength)));

        /// <summary>
        ///     Returns the number of bytes used to represent any expression.
        /// </summary>
        /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
        /// <param name="arg">The value to be examined for data length.</param>
        /// <returns>The number of bytes in the input value.</returns>
        public static int? DataLength(
            this DbFunctions _,
            double? arg)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DataLength)));

        /// <summary>
        ///     Returns the number of bytes used to represent any expression.
        /// </summary>
        /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
        /// <param name="arg">The value to be examined for data length.</param>
        /// <returns>The number of bytes in the input value.</returns>
        public static int? DataLength(
            this DbFunctions _,
            decimal? arg)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DataLength)));

        /// <summary>
        ///     Returns the number of bytes used to represent any expression.
        /// </summary>
        /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
        /// <param name="arg">The value to be examined for data length.</param>
        /// <returns>The number of bytes in the input value.</returns>
        public static int? DataLength(
            this DbFunctions _,
            DateTime? arg)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DataLength)));

        /// <summary>
        ///     Returns the number of bytes used to represent any expression.
        /// </summary>
        /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
        /// <param name="arg">The value to be examined for data length.</param>
        /// <returns>The number of bytes in the input value.</returns>
        public static int? DataLength(
            this DbFunctions _,
            TimeSpan? arg)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DataLength)));

        /// <summary>
        ///     Returns the number of bytes used to represent any expression.
        /// </summary>
        /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
        /// <param name="arg">The value to be examined for data length.</param>
        /// <returns>The number of bytes in the input value.</returns>
        public static int? DataLength(
            this DbFunctions _,
            DateTimeOffset? arg)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DataLength)));

        /// <summary>
        ///     Returns the number of bytes used to represent any expression.
        /// </summary>
        /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
        /// <param name="arg">The value to be examined for data length.</param>
        /// <returns>The number of bytes in the input value.</returns>
        public static int? DataLength(
            this DbFunctions _,
            byte[]? arg)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DataLength)));

        /// <summary>
        ///     Returns the number of bytes used to represent any expression.
        /// </summary>
        /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
        /// <param name="arg">The value to be examined for data length.</param>
        /// <returns>The number of bytes in the input value.</returns>
        public static int? DataLength(
            this DbFunctions _,
            Guid? arg)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DataLength)));

        /// <summary>
        ///     Validate if the given string is a valid numeric.
        ///     Corresponds to the SQL Server's <c>ISNUMERIC(expression)</c>.
        /// </summary>
        /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
        /// <param name="expression">Expression to validate</param>
        /// <returns><see langword="true" /> for a valid numeric, otherwise <see langword="false" />.</returns>
        public static bool IsNumeric(
            this DbFunctions _,
            string expression)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(IsNumeric)));
    }
}
