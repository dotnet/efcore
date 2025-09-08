// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using Microsoft.EntityFrameworkCore.XuGu.Storage.Internal;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore
{
    public static class XGCommonJsonChangeTrackingOptionsExtensions
    {
        public static XGJsonChangeTrackingOptions ToJsonChangeTrackingOptions(this XGCommonJsonChangeTrackingOptions options)
            => options switch
            {
                XGCommonJsonChangeTrackingOptions.RootPropertyOnly => XGJsonChangeTrackingOptions.CompareRootPropertyOnly,
                XGCommonJsonChangeTrackingOptions.FullHierarchyOptimizedFast => XGJsonChangeTrackingOptions.CompareStringRootPropertyByEquals |
                                                                                   XGJsonChangeTrackingOptions.CompareDomRootPropertyByEquals |
                                                                                   XGJsonChangeTrackingOptions.SnapshotCallsDeepClone |
                                                                                   XGJsonChangeTrackingOptions.SnapshotCallsClone,
                XGCommonJsonChangeTrackingOptions.FullHierarchyOptimizedSemantically => XGJsonChangeTrackingOptions.CompareStringRootPropertyByEquals |
                                                                                           XGJsonChangeTrackingOptions.CompareDomSemantically |
                                                                                           XGJsonChangeTrackingOptions.HashDomSemantiallyOptimized |
                                                                                           XGJsonChangeTrackingOptions.SnapshotCallsDeepClone |
                                                                                           XGJsonChangeTrackingOptions.SnapshotCallsClone,
                XGCommonJsonChangeTrackingOptions.FullHierarchySemantically => XGJsonChangeTrackingOptions.None,
                _ => throw new ArgumentOutOfRangeException(nameof(options)),
            };
    }
}
