using System;
using System.Collections.Generic;

namespace E2ETest.Namespace
{
    public partial class NonNullBoolWithDefault
    {
        public int Id { get; set; }
        public bool? BoolWithDefaultValueSql { get; set; }
        public bool BoolWithoutDefaultValueSql { get; set; }
    }
}
