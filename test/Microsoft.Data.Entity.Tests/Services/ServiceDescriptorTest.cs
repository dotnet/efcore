// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Xunit;

namespace Microsoft.Data.Entity.Services
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
            var serviceDescriptor = ServiceDescriptor.Singleton<IComponent, Component>();

            Assert.Equal(typeof(IComponent), serviceDescriptor.ServiceType);
            Assert.Equal(typeof(Component), serviceDescriptor.ImplementationType);
            Assert.Null(serviceDescriptor.ImplementationInstance);
        }

        [Fact]
        public void Singleton_creates_descriptor_for_instance()
        {
            var implementationInstance = new Component();

            var serviceDescriptor = ServiceDescriptor.Singleton<IComponent>(implementationInstance);

            Assert.Equal(typeof(IComponent), serviceDescriptor.ServiceType);
            Assert.Same(implementationInstance, serviceDescriptor.ImplementationInstance);
            Assert.Null(serviceDescriptor.ImplementationType);
        }

        [Fact]
        public void Scoped_creates_descriptor_for_type()
        {
            var serviceDescriptor = ServiceDescriptor.Scoped<IComponent, Component>();

            Assert.Equal(typeof(IComponent), serviceDescriptor.ServiceType);
            Assert.Equal(typeof(Component), serviceDescriptor.ImplementationType);
            Assert.Null(serviceDescriptor.ImplementationInstance);
        }

        [Fact]
        public void Scoped_creates_descriptor_for_instance()
        {
            var implementationInstance = new Component();

            var serviceDescriptor = ServiceDescriptor.Scoped<IComponent>(implementationInstance);

            Assert.Equal(typeof(IComponent), serviceDescriptor.ServiceType);
            Assert.Same(implementationInstance, serviceDescriptor.ImplementationInstance);
            Assert.Null(serviceDescriptor.ImplementationType);
        }

        [Fact]
        public void Transient_creates_descriptor_for_type()
        {
            var serviceDescriptor = ServiceDescriptor.Transient<IComponent, Component>();

            Assert.Equal(typeof(IComponent), serviceDescriptor.ServiceType);
            Assert.Equal(typeof(Component), serviceDescriptor.ImplementationType);
            Assert.Null(serviceDescriptor.ImplementationInstance);
        }

        [Fact]
        public void Transient_creates_descriptor_for_instance()
        {
            var implementationInstance = new Component();

            var serviceDescriptor = ServiceDescriptor.Transient<IComponent>(implementationInstance);

            Assert.Equal(typeof(IComponent), serviceDescriptor.ServiceType);
            Assert.Same(implementationInstance, serviceDescriptor.ImplementationInstance);
            Assert.Null(serviceDescriptor.ImplementationType);
        }
    }
}
