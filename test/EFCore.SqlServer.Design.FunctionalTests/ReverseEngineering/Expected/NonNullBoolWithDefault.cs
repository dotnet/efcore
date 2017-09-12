using System;
using System.Collections.Generic;

namespace E2ETest.Namespace
{
    public partial class NonNullBoolWithDefault
    {
        public int Id { get; set; }
        public bool? BoolWithDefaultValueSql { get; set; }
        public bool BoolWithDefaultValueSqlFalse1 { get; set; }
        public bool BoolWithDefaultValueSqlFalse2 { get; set; }
        public bool BoolWithDefaultValueSqlFalse3 { get; set; }
        public bool BoolWithDefaultValueSqlFalse4 { get; set; }
        public bool BoolWithDefaultValueSqlFalse5 { get; set; }
        public bool BoolWithoutDefaultValueSql { get; set; }
    }
}
