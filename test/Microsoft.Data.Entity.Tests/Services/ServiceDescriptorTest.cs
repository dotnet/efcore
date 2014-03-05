// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Microsoft.Data.Entity.Services;
using Xunit;

namespace Microsoft.Data.Entity.Tests.Services
{
    public class ServiceDescriptorTest
    {
        #region Fixture

        public interface IComponent
        {
        }

        public class Component : IComponent
        {
        }

        #endregion

        [Fact]
        public void Singleton_creates_descriptor_for_type()
        {
            var serviceDescriptor = Service.Singleton<IComponent, Component>();

            Assert.Equal(typeof(IComponent), serviceDescriptor.ServiceType);
            Assert.Equal(typeof(Component), serviceDescriptor.ImplementationType);
            Assert.Null(serviceDescriptor.ImplementationInstance);
        }

        [Fact]
        public void Singleton_creates_descriptor_for_instance()
        {
            var implementationInstance = new Component();

            var serviceDescriptor = Service.Singleton<IComponent>(implementationInstance);

            Assert.Equal(typeof(IComponent), serviceDescriptor.ServiceType);
            Assert.Same(implementationInstance, serviceDescriptor.ImplementationInstance);
            Assert.Null(serviceDescriptor.ImplementationType);
        }

        [Fact]
        public void Scoped_creates_descriptor_for_type()
        {
            var serviceDescriptor = Service.Scoped<IComponent, Component>();

            Assert.Equal(typeof(IComponent), serviceDescriptor.ServiceType);
            Assert.Equal(typeof(Component), serviceDescriptor.ImplementationType);
            Assert.Null(serviceDescriptor.ImplementationInstance);
        }

        [Fact]
        public void Scoped_creates_descriptor_for_instance()
        {
            var implementationInstance = new Component();

            var serviceDescriptor = Service.Scoped<IComponent>(implementationInstance);

            Assert.Equal(typeof(IComponent), serviceDescriptor.ServiceType);
            Assert.Same(implementationInstance, serviceDescriptor.ImplementationInstance);
            Assert.Null(serviceDescriptor.ImplementationType);
        }

        [Fact]
        public void Transient_creates_descriptor_for_type()
        {
            var serviceDescriptor = Service.Transient<IComponent, Component>();

            Assert.Equal(typeof(IComponent), serviceDescriptor.ServiceType);
            Assert.Equal(typeof(Component), serviceDescriptor.ImplementationType);
            Assert.Null(serviceDescriptor.ImplementationInstance);
        }

        [Fact]
        public void Transient_creates_descriptor_for_instance()
        {
            var implementationInstance = new Component();

            var serviceDescriptor = Service.Transient<IComponent>(implementationInstance);

            Assert.Equal(typeof(IComponent), serviceDescriptor.ServiceType);
            Assert.Same(implementationInstance, serviceDescriptor.ImplementationInstance);
            Assert.Null(serviceDescriptor.ImplementationType);
        }
    }
}
