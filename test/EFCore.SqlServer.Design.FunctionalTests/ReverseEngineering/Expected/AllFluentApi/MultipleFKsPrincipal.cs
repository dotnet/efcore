using System;
using System.Collections.Generic;

namespace E2ETest.Namespace
{
    public partial class MultipleFksPrincipal
    {
        public MultipleFksPrincipal()
        {
            MultipleFksDependentRelationA = new HashSet<MultipleFksDependent>();
            MultipleFksDependentRelationB = new HashSet<MultipleFksDependent>();
            MultipleFksDependentRelationC = new HashSet<MultipleFksDependent>();
        }

        public int MultipleFksPrincipalId { get; set; }
        public string SomePrincipalColumn { get; set; }

        public ICollection<MultipleFksDependent> MultipleFksDependentRelationA { get; set; }
        public ICollection<MultipleFksDependent> MultipleFksDependentRelationB { get; set; }
        public ICollection<MultipleFksDependent> MultipleFksDependentRelationC { get; set; }
    }
}
