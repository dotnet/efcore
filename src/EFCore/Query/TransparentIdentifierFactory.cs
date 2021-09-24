// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Query
{
    /// <summary>
    ///     <para>
    ///         A factory to create transparent identifier to create during query processing.
    ///         Transparent identifier is struct of outer and inner elements which is generally created as a result of join methods
    ///         as intermediate type to hold values from both sources.
    ///     </para>
    ///     <para>
    ///         This type is typically used by database providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-providers">Implementation of database providers and extensions</see>
    ///     and <see href="https://aka.ms/efcore-how-queries-work">How EF Core queries work</see> for more information.
    /// </remarks>
    public static class TransparentIdentifierFactory
    {
        /// <summary>
        ///     Creates new transparent identifier type for given types.
        /// </summary>
        /// <param name="outerType">The outer type of the transparent identifier.</param>
        /// <param name="innerType">The inner type of the transparent identifier.</param>
        /// <returns>The created transparent identifier type.</returns>
        public static Type Create(Type outerType, Type innerType)
        {
            Check.NotNull(outerType, nameof(outerType));
            Check.NotNull(innerType, nameof(innerType));

            return typeof(TransparentIdentifier<,>).MakeGenericType(outerType, innerType);
        }

        private readonly struct TransparentIdentifier<TOuter, TInner>
        {
            [UsedImplicitly]
            public TransparentIdentifier(TOuter outer, TInner inner)
            {
                Outer = outer;
                Inner = inner;
            }

            [UsedImplicitly]
            public readonly TOuter Outer;

            [UsedImplicitly]
            public readonly TInner Inner;
        }
    }
}
