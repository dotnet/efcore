// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

namespace Microsoft.EntityFrameworkCore
{
    public class LazyLoadProxyInMemoryTest : LazyLoadProxyTestBase<LazyLoadProxyInMemoryTest.LoadInMemoryFixture>
    {
        public LazyLoadProxyInMemoryTest(LoadInMemoryFixture fixture)
            : base(fixture)
        {
        }

        [ConditionalFact(Skip = "Issue#16963")]
        public override void Lazy_loading_finds_correct_entity_type_with_already_loaded_owned_types()
        {
            base.Lazy_loading_finds_correct_entity_type_with_already_loaded_owned_types();
        }

        [ConditionalFact(Skip = "Issue#16963")]
        public override void Lazy_loading_finds_correct_entity_type_with_alternate_model()
        {
            base.Lazy_loading_finds_correct_entity_type_with_alternate_model();
        }

        [ConditionalFact(Skip = "Issue#16963")]
        public override void Lazy_loading_finds_correct_entity_type_with_multiple_queries()
        {
            base.Lazy_loading_finds_correct_entity_type_with_multiple_queries();
        }

        [ConditionalFact(Skip = "Issue#16963")]
        public override void Lazy_loading_finds_correct_entity_type_with_opaque_predicate_and_multiple_queries()
        {
            base.Lazy_loading_finds_correct_entity_type_with_opaque_predicate_and_multiple_queries();
        }

        public class LoadInMemoryFixture : LoadFixtureBase
        {
            protected override ITestStoreFactory TestStoreFactory => InMemoryTestStoreFactory.Instance;
        }
    }
}
