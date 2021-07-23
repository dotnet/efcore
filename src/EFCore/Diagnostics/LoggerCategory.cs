// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using Microsoft.Extensions.Logging;

namespace Microsoft.EntityFrameworkCore.Diagnostics
{
    /// <summary>
    ///     Generic helper class used to implement the <see cref="Name" /> property.
    /// </summary>
    /// <typeparam name="T"> The logger category type. </typeparam>
    public abstract class LoggerCategory<T>
    {
        /// <summary>
        ///     The logger category name, for use with <see cref="ILoggerProvider" />, etc.
        /// </summary>
        /// <returns> The category name. </returns>
        public static string Name { get; } = ToName(typeof(T));

        /// <summary>
        ///     The logger category name.
        /// </summary>
        /// <returns> The logger category name. </returns>
        public override string ToString()
            => Name;

        /// <summary>
        ///     The logger category name.
        /// </summary>
        /// <param name="loggerCategory"> The category. </param>
        public static implicit operator string(LoggerCategory<T> loggerCategory)
            => loggerCategory.ToString();

        private static string ToName(Type loggerCategoryType)
        {
            const string outerClassName = "." + nameof(DbLoggerCategory);

            var name = loggerCategoryType.FullName!.Replace('+', '.');
            var index = name.IndexOf(outerClassName, StringComparison.Ordinal);
            if (index >= 0)
            {
                name = name.Substring(0, index) + name.Substring(index + outerClassName.Length);
            }

            return name;
        }
    }
}
