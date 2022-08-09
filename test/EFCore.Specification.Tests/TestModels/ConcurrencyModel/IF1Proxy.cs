// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.TestModels.ConcurrencyModel;

public interface IF1Proxy
{
    public bool CreatedCalled { get; set; }
    public bool InitializingCalled { get; set; }
    public bool InitializedCalled { get; set; }
}
