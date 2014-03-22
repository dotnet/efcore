// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Linq;
using Microsoft.AspNet.Logging;
using Microsoft.Data.Entity.ChangeTracking;
using Microsoft.Data.Entity.Identity;
using Microsoft.Data.Entity.Metadata;
using Xunit;

namespace Microsoft.Data.Entity.Tests
{
    public class EntityServicesTest
    {
        [Fact]
        public void Can_get_default_services()
        {
            var services = EntityServices.GetDefaultServices().ToList();

            Assert.True(services.Any(sd => sd.ServiceType == typeof(ILoggerFactory)));
            Assert.True(services.Any(sd => sd.ServiceType == typeof(IModelSource)));
            Assert.True(services.Any(sd => sd.ServiceType == typeof(IdentityGeneratorFactory)));
            Assert.True(services.Any(sd => sd.ServiceType == typeof(ActiveIdentityGenerators)));
            Assert.True(services.Any(sd => sd.ServiceType == typeof(EntitySetFinder)));
            Assert.True(services.Any(sd => sd.ServiceType == typeof(EntitySetInitializer)));
            Assert.True(services.Any(sd => sd.ServiceType == typeof(EntityKeyFactorySource)));
            Assert.True(services.Any(sd => sd.ServiceType == typeof(ClrPropertyGetterSource)));
            Assert.True(services.Any(sd => sd.ServiceType == typeof(ClrPropertySetterSource)));
            Assert.True(services.Any(sd => sd.ServiceType == typeof(EntitySetSource)));
            Assert.True(services.Any(sd => sd.ServiceType == typeof(ClrCollectionAccessorSource)));
            Assert.True(services.Any(sd => sd.ServiceType == typeof(EntityMaterializerSource)));
            Assert.True(services.Any(sd => sd.ServiceType == typeof(StateEntryFactory)));
            Assert.True(services.Any(sd => sd.ServiceType == typeof(IEntityStateListener)));
            Assert.True(services.Any(sd => sd.ServiceType == typeof(StateEntryNotifier)));
            Assert.True(services.Any(sd => sd.ServiceType == typeof(ContextConfiguration)));
            Assert.True(services.Any(sd => sd.ServiceType == typeof(ContextEntitySets)));
            Assert.True(services.Any(sd => sd.ServiceType == typeof(StateManager)));
        }
    }
}
