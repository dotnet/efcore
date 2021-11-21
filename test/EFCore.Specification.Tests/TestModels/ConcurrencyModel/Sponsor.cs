// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.ObjectModel;

namespace Microsoft.EntityFrameworkCore.TestModels.ConcurrencyModel;

public class Sponsor
{
    public static readonly string ClientTokenPropertyName = "ClientToken";

    private readonly ObservableCollection<Team> _teams = new();

    public int Id { get; set; }
    public string Name { get; set; }

    public virtual ICollection<Team> Teams
        => _teams;
}
