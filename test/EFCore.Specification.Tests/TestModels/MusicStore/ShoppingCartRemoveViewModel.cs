// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.TestModels.MusicStore;

#nullable disable

public class ShoppingCartRemoveViewModel
{
    public string Message { get; set; }
    public decimal CartTotal { get; set; }
    public int CartCount { get; set; }
    public int ItemCount { get; set; }
    public int DeleteId { get; set; }
}
