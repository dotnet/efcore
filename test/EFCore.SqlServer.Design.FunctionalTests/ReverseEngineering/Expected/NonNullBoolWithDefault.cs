using System;
using System.Collections.Generic;

namespace E2ETest.Namespace
{
    public partial class NonNullBoolWithDefault
    {
        public int Id { get; set; }
        public bool? TestWithDefault { get; set; }
        public bool TestWithoutDefault { get; set; }
    }
}
