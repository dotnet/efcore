// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class OwnedQueryInMemoryTest : OwnedQueryTestBase<OwnedQueryInMemoryTest.OwnedQueryInMemoryFixture>
    {
        public OwnedQueryInMemoryTest(OwnedQueryInMemoryFixture fixture, ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            //TestLoggerFactory.TestOutputHelper = testOutputHelper;
        }

        public class OwnedQueryInMemoryFixture : OwnedQueryFixtureBase
        {
            protected override ITestStoreFactory TestStoreFactory => InMemoryTestStoreFactory.Instance;

            // #11474
            protected override void Seed(DbContext context)
            {
                context.Set<OwnedPerson>().AddRange(
                    new OwnedPerson
                    {
                        PersonAddress = new OwnedAddress
                        {
                            Country = new OwnedCountry { Name = "USA" }
                        }
                    },
                    new Branch
                    {
                        PersonAddress = new OwnedAddress
                        {
                            Country = new OwnedCountry { Name = "USA" }
                        },
                        BranchAddress = new OwnedAddress
                        {
                            Country = new OwnedCountry { Name = "Canada" }
                        }
                    },
                    new LeafA
                    {
                        PersonAddress = new OwnedAddress
                        {
                            Country = new OwnedCountry { Name = "USA" }
                        },
                        BranchAddress = new OwnedAddress
                        {
                            Country = new OwnedCountry { Name = "Canada" }
                        },
                        LeafAAddress = new OwnedAddress
                        {
                            Country = new OwnedCountry { Name = "Mexico" }
                        }
                    },
                    new LeafB
                    {
                        PersonAddress = new OwnedAddress
                        {
                            Country = new OwnedCountry { Name = "USA" }
                        },
                        LeafBAddress = new OwnedAddress
                        {
                            Country = new OwnedCountry { Name = "Panama" }
                        }
                    });

                context.SaveChanges();
            }
        }
    }
}
