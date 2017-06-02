using System;
using System.Collections.Generic;

namespace E2ETest.Namespace
{
    public partial class OneToOneSeparateFkprincipal
    {
        public int OneToOneSeparateFkprincipalId1 { get; set; }
        public int OneToOneSeparateFkprincipalId2 { get; set; }
        public string SomeOneToOneSeparateFkprincipalColumn { get; set; }

        public OneToOneSeparateFkdependent OneToOneSeparateFkdependent { get; set; }
    }
}
