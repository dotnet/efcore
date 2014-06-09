// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Framework.DependencyInjection;
using Xunit;

namespace Microsoft.Data.Entity.AzureTableStorage.Tests
{
    public class AtsOptionsExtensionTests : AtsOptionsExtension
    {
        [Fact]
        public void It_sets_up_batching_data_store()
        {
            var serviceCollection = new TestServiceCollecion();
            var builder = new EntityServicesBuilder(serviceCollection);
            UseBatching = true;
            ApplyServices(builder);
            Assert.Equal(typeof(AtsBatchedDataStore), serviceCollection.Services[typeof(AtsDataStore)]);
        }

        [Fact]
        public void It_defaults_to_no_batching()
        {
            var serviceCollection = new TestServiceCollecion();
            var builder = new EntityServicesBuilder(serviceCollection);

            ApplyServices(builder);

            Assert.NotEqual(typeof(AtsBatchedDataStore), serviceCollection.Services[typeof(AtsDataStore)]);
            Assert.Equal(typeof(AtsDataStore), serviceCollection.Services[typeof(AtsDataStore)]);
        }

        #region test service collection

        private class TestServiceCollecion : IServiceCollection
        {
            public readonly Dictionary<Type, Type> Services = new Dictionary<Type, Type>();

            public IEnumerator<IServiceDescriptor> GetEnumerator()
            {
                throw new NotImplementedException();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            public IServiceCollection Add(IServiceDescriptor descriptor)
            {
                throw new NotImplementedException();
            }

            public IServiceCollection Add(IEnumerable<IServiceDescriptor> descriptors)
            {
                throw new NotImplementedException();
            }

            public IServiceCollection AddTransient(Type service, Type implementationType)
            {
                Services.Add(service, implementationType);
                return this;
            }

            public IServiceCollection AddScoped(Type service, Type implementationType)
            {
                Services.Add(service, implementationType);
                return this;
            }

            public IServiceCollection AddSingleton(Type service, Type implementationType)
            {
                Services.Add(service, implementationType);
                return this;
            }

            public IServiceCollection AddInstance(Type service, object implementationInstance)
            {
                throw new NotImplementedException();
            }
        }

        #endregion
    }
}
