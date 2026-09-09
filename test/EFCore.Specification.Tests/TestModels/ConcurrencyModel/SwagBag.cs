// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel.DataAnnotations;

namespace Microsoft.EntityFrameworkCore.TestModels.ConcurrencyModel;

#nullable disable

[Owned]
public class SwagBag
{
    public class SwagBagProxy : SwagBag, IF1Proxy
    {
        public bool CreatedCalled { get; set; }
        public bool InitializingCalled { get; set; }
        public bool InitializedCalled { get; set; }
    }

    [Required]
    public string Stuff { get; set; }
}
