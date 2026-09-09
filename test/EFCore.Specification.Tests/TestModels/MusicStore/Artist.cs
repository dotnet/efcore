// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel.DataAnnotations;

namespace Microsoft.EntityFrameworkCore.TestModels.MusicStore;

#nullable disable

public class Artist
{
    public int ArtistId { get; set; }

    [Required]
    public string Name { get; set; }
}
