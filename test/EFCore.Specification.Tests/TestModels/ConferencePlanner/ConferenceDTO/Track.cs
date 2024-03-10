// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel.DataAnnotations;

namespace Microsoft.EntityFrameworkCore.TestModels.ConferencePlanner.ConferenceDTO;

#nullable disable

public class Track
{
    public int Id { get; set; }

    [Required]
    [StringLength(200)]
    public string Name { get; set; }
}
