// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Storage
{
    /// <summary>
    ///     <para>
    ///         Parameter object containing context needed for materialization of an entity.
    ///     </para>
    ///     <para>
    ///         This type is typically used by database providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    /// </summary>
    public readonly struct MaterializationContext
    {
        /// <summary>
        ///     The <see cref="MethodInfo" /> for the <see cref="ValueBuffer" /> get method.
        /// </summary>
        public static readonly MethodInfo GetValueBufferMethod
            = typeof(MaterializationContext).GetProperty(nameof(ValueBuffer)).GetMethod;

        internal static readonly PropertyInfo ContextProperty
            = typeof(MaterializationContext).GetProperty(nameof(Context));

        /// <summary>
        ///     Creates a new <see cref="MaterializationContext" /> instance.
        /// </summary>
        /// <param name="valueBuffer"> The <see cref="ValueBuffer" /> to use to materialize an entity. </param>
        /// <param name="context"> The current <see cref="DbContext" /> instance being used. </param>
        public MaterializationContext(
            in ValueBuffer valueBuffer,
            [NotNull] DbContext context)
        {
            Debug.Assert(context != null); // Hot path

            ValueBuffer = valueBuffer;
            Context = context;
        }

        /// <summary>
        ///     The <see cref="ValueBuffer" /> to use to materialize an entity.
        /// </summary>
        public ValueBuffer ValueBuffer
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get;
        }

        /// <summary>
        ///     The current <see cref="DbContext" /> instance being used.
        /// </summary>
        public DbContext Context
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get;
        }
    }
}
