// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.ObjectModel;

namespace Microsoft.EntityFrameworkCore.TestModels.ManyToManyFieldsModel;

#nullable disable

public class GeneratedKeysRight
{
    public int Id;
    public string Name;

    public ICollection<GeneratedKeysLeft> Lefts = new ObservableCollection<GeneratedKeysLeft>();
}
