// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Query;

namespace Microsoft.EntityFrameworkCore
{
    /// <summary>
    ///     Static methods that are useful in application code where there is not an EF type for the method to be accessed from. For example,
    ///     referencing a shadow state property in a LINQ query.
    /// </summary>
    // ReSharper disable once InconsistentNaming
    public static partial class EF
    {
        internal static readonly MethodInfo PropertyMethod
            = typeof(EF).GetTypeInfo().GetDeclaredMethod(nameof(Property));

        /// <summary>
        ///     Addresses a given property on an entity instance. This is useful when you want to reference a shadow state property in a
        ///     LINQ query. Currently this method can only be used in LINQ queries and can not be used to access the value assigned to a
        ///     property in other scenarios.
        /// </summary>
        /// <example>
        ///     <para>
        ///         The following code performs a filter using the a LastUpdated shadow state property.
        ///     </para>
        ///     <code>
        /// var blogs = context.Blogs
        ///     .Where(b =&gt; EF.Property&lt;DateTime&gt;(b, "LastUpdated") > DateTime.Now.AddDays(-5));
        ///     </code>
        /// </example>
        /// <typeparam name="TProperty"> The type of the property being referenced. </typeparam>
        /// <param name="entity"> The entity to access the property on. </param>
        /// <param name="propertyName"> The name of the property. </param>
        /// <returns> The value assigned to the property. </returns>
        public static TProperty Property<TProperty>(
            [NotNull] object entity,
            [NotNull] [NotParameterized] string propertyName)
            => throw new InvalidOperationException(CoreStrings.PropertyMethodInvoked);

        /// <summary>
        ///     Provides CLR methods that get translated to database functions when used in LINQ to Entities queries.
        ///     Calling these methods in other contexts (e.g. LINQ to Objects) will throw a <see cref="NotSupportedException" />.
        /// </summary>
        public static DbFunctions Functions
            => DbFunctions.Instance;
    }
}
