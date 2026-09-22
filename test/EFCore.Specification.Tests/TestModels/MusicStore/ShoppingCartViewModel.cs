// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.TestModels.MusicStore;

#nullable disable

public class ShoppingCartViewModel
{
    public List<CartItem> CartItems { get; set; }
    public decimal CartTotal { get; set; }
}
