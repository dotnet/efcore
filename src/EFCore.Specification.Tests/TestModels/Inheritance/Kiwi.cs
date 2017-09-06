// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.EntityFrameworkCore.TestModels.Inheritance
{
    public class Kiwi : Bird
    {
        public Island FoundOn { get; set; }
    }

    public enum Island : byte
    {
        North,
        South
    }

    public class KiwiQuery : BirdQuery
    {
        public Island FoundOn { get; set; }
    }
}
