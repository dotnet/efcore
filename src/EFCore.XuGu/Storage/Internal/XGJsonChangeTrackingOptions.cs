// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Microsoft.EntityFrameworkCore.XuGu.Storage.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [Flags]
    public enum XGJsonChangeTrackingOptions
    {
        /// <summary>
        /// The default is to serialize everything, which is the most precise, but also the slowest.
        /// </summary>
        None = 0,

        /// <summary>
        /// Do not track changes inside of JSON mapped properties but only for the root property itself.
        /// For example, if the JSON mapped property is a top level array of `int`, then changes to items of the
        /// array are not tracked, but changes to the array property itself (the reference) are.
        /// </summary>
        CompareRootPropertyOnly = 0x00000001 | CompareStringRootPropertyByEquals | CompareDomRootPropertyByEquals,

        /// <summary>
        /// Compare strings as is, without further processing. This means that adding whitespaces between inner
        /// properties of a JSON object, that have no effect at all to the JSON object itself, would lead to a change
        /// being discovered to the JSON object, resulting in the JSON mapped property being marked as modified.
        /// </summary>
        CompareStringRootPropertyByEquals = 0x00000002,

        /// <summary>
        /// Only check the JSON root property for DOM objects.
        /// </summary>
        CompareDomRootPropertyByEquals = 0x00000004,

        /// <summary>
        /// Traverse the DOM to check for changes.
        /// </summary>
        CompareDomSemantically = 0x00000008,

        /// <summary>
        /// Fully traverse the DOM to generate a hash.
        /// </summary>
        HashDomSemantially = 0x00010000,

        /// <summary>
        /// Traverse part of the DOM to generate a hash.
        /// </summary>
        HashDomSemantiallyOptimized = 0x00020000,

        /// <summary>
        /// Call DeepClone() whenever a type, for which a snapshot needs to be generated, implements it.
        /// </summary>
        SnapshotCallsDeepClone = 0x01000000,

        /// <summary>
        /// Call Clone() whenever a type, for which a snapshot needs to be generated, implements it.
        /// </summary>
        SnapshotCallsClone = 0x02000000,
    }
}
