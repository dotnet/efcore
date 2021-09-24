// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Query;

namespace Microsoft.EntityFrameworkCore
{
    /// <summary>
    ///     Static methods that are useful in application code where there is not an EF type for the method to be accessed from. For example,
    ///     referencing a shadow state property in a LINQ query.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-database-functions">Database functions</see> and
    ///     <see href="https://aka.ms/efcore-docs-efproperty">Using EF.Property in EF Core queries</see> for more information.
    /// </remarks>
    // ReSharper disable once InconsistentNaming
    public static partial class EF
    {
        internal static readonly MethodInfo PropertyMethod
            = typeof(EF).GetRequiredDeclaredMethod(nameof(Property));

        /// <summary>
        ///     <para>
        ///         References a given property or navigation on an entity instance. This is useful for shadow state properties, for
        ///         which no CLR property exists. Currently this method can only be used in LINQ queries and can not be used to
        ///         access the value assigned to a property in other scenarios.
        ///     </para>
        ///     <para>
        ///         Note that this is a static method accessed through the top-level <see cref="EF" /> static type.
        ///     </para>
        /// </summary>
        /// <remarks>
        ///     See <see href="https://aka.ms/efcore-docs-efproperty">Using EF.Property in EF Core queries</see> for more information.
        /// </remarks>
        /// <example>
        ///     <para>
        ///         The following code performs a filter using the a LastUpdated shadow state property.
        ///     </para>
        ///     <code>
        /// var blogs = context.Blogs
        ///     .Where(b =&gt; EF.Property&lt;DateTime&gt;(b, "LastUpdated") > DateTime.Now.AddDays(-5));
        ///     </code>
        /// </example>
        /// <typeparam name="TProperty">The type of the property being referenced.</typeparam>
        /// <param name="entity">The entity to access the property on.</param>
        /// <param name="propertyName">The name of the property.</param>
        /// <returns>The value assigned to the property.</returns>
        public static TProperty Property<TProperty>(
            object entity,
            [NotParameterized] string propertyName)
            => throw new InvalidOperationException(CoreStrings.PropertyMethodInvoked);

        /// <summary>
        ///     <para>
        ///         Provides CLR methods that get translated to database functions when used in LINQ to Entities queries.
        ///         Calling these methods in other contexts (e.g. LINQ to Objects) will throw a <see cref="NotSupportedException" />.
        ///     </para>
        ///     <para>
        ///         Note that this is a static property accessed through the top-level <see cref="EF" /> static type.
        ///     </para>
        /// </summary>
        /// <remarks>
        ///     See <see href="https://aka.ms/efcore-docs-database-functions">Database functions</see> for more information.
        /// </remarks>
        public static DbFunctions Functions
            => DbFunctions.Instance;
    }
}
