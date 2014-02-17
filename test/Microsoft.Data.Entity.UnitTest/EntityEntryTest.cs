// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Xunit;

namespace Microsoft.Data.Entity
{
    public class EntityEntryTest
    {
        [Fact]
        public void CanInitializeWithEntity()
        {
            var entity = new object();

            var entityEntry = new EntityEntry(entity);

            Assert.Same(entity, entityEntry.Entity);
            Assert.Equal(EntityState.Unchanged, entityEntry.EntityState);
        }
    }
}
