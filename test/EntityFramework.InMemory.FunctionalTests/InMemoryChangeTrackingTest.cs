using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.Entity.FunctionalTests;
using Microsoft.Data.Entity.InMemory.FunctionalTests;

namespace Microsoft.Data.Entity.InMemory.Tests
{
    class InMemoryChangeTrackingTest : ChangeTrackingTestBase<InMemoryNorthwindQueryFixture>
    {
        public InMemoryChangeTrackingTest(InMemoryNorthwindQueryFixture fixture)
            : base(fixture)
        {
        }
    }
}
