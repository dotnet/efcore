using System;
using System.Collections.Generic;

namespace E2ETest.Namespace
{
    public partial class OneToOneDependent
    {
        public int OneToOneDependentId1 { get; set; }
        public int OneToOneDependentId2 { get; set; }
        public string SomeDependentEndColumn { get; set; }

        public OneToOnePrincipal OneToOneDependentNavigation { get; set; }
    }
}
