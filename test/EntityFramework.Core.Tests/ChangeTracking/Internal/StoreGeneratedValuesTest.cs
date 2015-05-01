// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using Microsoft.Data.Entity.ChangeTracking.Internal;
using Xunit;

namespace Microsoft.Data.Entity.Tests.ChangeTracking.Internal
{
    public class StoreGeneratedValuesTest : SidecarTest
    {
        [Fact]
        public void Is_set_up_for_transparent_access_and_auto_commit()
        {
            var sidecar = CreateSidecar();

            Assert.True(sidecar.TransparentRead);
            Assert.True(sidecar.TransparentWrite);
            Assert.True(sidecar.AutoCommit);
        }

        [Fact]
        public void Has_expected_name()
        {
            Assert.Equal(Sidecar.WellKnownNames.StoreGeneratedValues, CreateSidecar().Name);
        }

        protected override Sidecar CreateSidecar(InternalEntityEntry entry = null)
        {
            entry = entry ?? CreateInternalEntry();
            var properties = entry.EntityType.GetPrimaryKey().Properties
                .Concat(entry.EntityType.GetForeignKeys().SelectMany(fk => fk.Properties))
                .ToList();

            return new StoreGeneratedValuesFactory().Create(entry, properties);
        }
    }
}
