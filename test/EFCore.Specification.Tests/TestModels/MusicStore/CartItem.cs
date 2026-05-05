// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel.DataAnnotations;

namespace Microsoft.EntityFrameworkCore.TestModels.MusicStore;

#nullable disable

public class CartItem
{
    [Key]
    public int CartItemId { get; set; }

    [Required]
    public string CartId { get; set; }

    public int AlbumId { get; set; }
    public int Count { get; set; }

    [DataType(DataType.DateTime)]
    public DateTime DateCreated { get; set; }

    public virtual Album Album { get; set; }
}
