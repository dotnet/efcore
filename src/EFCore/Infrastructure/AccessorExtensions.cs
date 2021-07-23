// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Infrastructure
{
    /// <summary>
    ///     <para>
    ///         Extension methods for <see cref="IInfrastructure{T}" />.
    ///     </para>
    ///     <para>
    ///         These methods are typically used by database providers (and other extensions). They are generally
    ///         not used in application code.
    ///     </para>
    ///     <para>
    ///         <see cref="IInfrastructure{T}" /> is used to hide properties that are not intended to be used in
    ///         application code but can be used in extension methods written by database providers etc.
    ///     </para>
    /// </summary>
    public static class AccessorExtensions
    {
        /// <summary>
        ///     <para>
        ///         Resolves a service from the <see cref="IServiceProvider" /> exposed from a type that implements
        ///         <see cref="IInfrastructure{IServiceProvider}" />.
        ///     </para>
        ///     <para>
        ///         This method is typically used by database providers (and other extensions). It is generally
        ///         not used in application code.
        ///     </para>
        ///     <para>
        ///         <see cref="IInfrastructure{T}" /> is used to hide properties that are not intended to be used in
        ///         application code but can be used in extension methods written by database providers etc.
        ///     </para>
        /// </summary>
        /// <typeparam name="TService"> The type of service to be resolved. </typeparam>
        /// <param name="accessor"> The object exposing the service provider. </param>
        /// <returns> The requested service. </returns>
        [DebuggerStepThrough]
        public static TService GetService<TService>(this IInfrastructure<IServiceProvider> accessor)
            where TService : class
            => InfrastructureExtensions.GetService<TService>(Check.NotNull(accessor, nameof(accessor)));

        /// <summary>
        ///     <para>
        ///         Gets the value from a property that is being hidden using <see cref="IInfrastructure{T}" />.
        ///     </para>
        ///     <para>
        ///         This method is typically used by database providers (and other extensions). It is generally
        ///         not used in application code.
        ///     </para>
        ///     <para>
        ///         <see cref="IInfrastructure{T}" /> is used to hide properties that are not intended to be used in
        ///         application code but can be used in extension methods written by database providers etc.
        ///     </para>
        /// </summary>
        /// <typeparam name="T"> The type of the property being hidden by <see cref="IInfrastructure{T}" />. </typeparam>
        /// <param name="accessor"> The object that exposes the property. </param>
        /// <returns> The object assigned to the property. </returns>
        [DebuggerStepThrough]
        public static T GetInfrastructure<T>(this IInfrastructure<T> accessor)
            => Check.NotNull(accessor, nameof(accessor)).Instance;
    }
}
