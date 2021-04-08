// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace Microsoft.EntityFrameworkCore.TestModels.InheritanceModel
{
    public class Eagle : Bird
    {
        public Eagle()
        {
            Prey = new List<Bird>();
        }

        public EagleGroup Group { get; set; }

        public ICollection<Bird> Prey { get; set; }
    }

    public enum EagleGroup
    {
        Fish,
        Booted,
        Snake,
        Harpy
    }

    public class EagleQuery : BirdQuery
    {
        public EagleGroup Group { get; set; }
    }
}
