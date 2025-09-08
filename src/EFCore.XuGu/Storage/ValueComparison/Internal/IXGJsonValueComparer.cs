// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.XuGu.Storage.Internal;

namespace Microsoft.EntityFrameworkCore.XuGu.Storage.ValueComparison.Internal
{
    public interface IXGJsonValueComparer
    {
        ValueComparer Clone(XGJsonChangeTrackingOptions options);
    }
}
