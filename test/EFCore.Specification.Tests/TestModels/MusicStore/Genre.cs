// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel.DataAnnotations;

namespace Microsoft.EntityFrameworkCore.TestModels.MusicStore;

#nullable disable

public class Genre
{
    public int GenreId { get; set; }

    [Required]
    public string Name { get; set; }

    public string Description { get; set; }

    public List<Album> Albums { get; set; }
}
