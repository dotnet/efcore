// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.ObjectModel;

namespace Microsoft.EntityFrameworkCore.TestModels.ManyToManyFieldsModel;

#nullable disable

public class GeneratedKeysLeft
{
    public int Id;
    public string Name;

    public ICollection<GeneratedKeysRight> Rights = new ObservableCollection<GeneratedKeysRight>();
}
