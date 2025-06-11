// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace Microsoft.EntityFrameworkCore.TestModels.ManyToManyModel;

#nullable disable

public class ImplicitManyToManyA
{
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public virtual int Id { get; set; }

    public virtual string Name { get; set; }

    public virtual ICollection<ImplicitManyToManyB> Bs { get; } = new ObservableCollection<ImplicitManyToManyB>();
}
