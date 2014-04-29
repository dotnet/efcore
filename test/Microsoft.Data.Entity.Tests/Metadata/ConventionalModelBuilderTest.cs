// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Microsoft.Data.Entity.Metadata.ModelConventions;
using Moq;
using Xunit;

namespace Microsoft.Data.Entity.Metadata
{
    public class ConventionalModelBuilderTest
    {
        private class Entity
        {
        }

        [Fact]
        public void OnEntityTypeAdded_calls_apply_on_conventions()
        {
            var model = new Model();
            var builder = new ConventionalModelBuilder(model);
            builder.Conventions.Clear();
            var convention = new Mock<IModelConvention>();
            builder.Conventions.Add(convention.Object);

            builder.Entity<Entity>();

            convention.Verify(c => c.Apply(It.Is<EntityType>(t => t.Type == typeof(Entity))));
        }
    }
}
