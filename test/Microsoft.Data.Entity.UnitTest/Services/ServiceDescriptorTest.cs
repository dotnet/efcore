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
        public void SingletonCreatesDescriptorForType()
        {
            var serviceDescriptor = ServiceDescriptor.Singleton<IComponent, Component>();

            Assert.Equal(typeof(IComponent), serviceDescriptor.ServiceType);
            Assert.Equal(typeof(Component), serviceDescriptor.ImplementationType);
            Assert.Null(serviceDescriptor.ImplementationInstance);
        }

        [Fact]
        public void SingletonCreatesDescriptorForInstance()
        {
            var implementationInstance = new Component();

            var serviceDescriptor = ServiceDescriptor.Singleton<IComponent>(implementationInstance);

            Assert.Equal(typeof(IComponent), serviceDescriptor.ServiceType);
            Assert.Same(implementationInstance, serviceDescriptor.ImplementationInstance);
            Assert.Null(serviceDescriptor.ImplementationType);
        }

        [Fact]
        public void ScopedCreatesDescriptorForType()
        {
            var serviceDescriptor = ServiceDescriptor.Scoped<IComponent, Component>();

            Assert.Equal(typeof(IComponent), serviceDescriptor.ServiceType);
            Assert.Equal(typeof(Component), serviceDescriptor.ImplementationType);
            Assert.Null(serviceDescriptor.ImplementationInstance);
        }

        [Fact]
        public void ScopedCreatesDescriptorForInstance()
        {
            var implementationInstance = new Component();

            var serviceDescriptor = ServiceDescriptor.Scoped<IComponent>(implementationInstance);

            Assert.Equal(typeof(IComponent), serviceDescriptor.ServiceType);
            Assert.Same(implementationInstance, serviceDescriptor.ImplementationInstance);
            Assert.Null(serviceDescriptor.ImplementationType);
        }
        [Fact]
        public void TransientCreatesDescriptorForType()
        {
            var serviceDescriptor = ServiceDescriptor.Transient<IComponent, Component>();

            Assert.Equal(typeof(IComponent), serviceDescriptor.ServiceType);
            Assert.Equal(typeof(Component), serviceDescriptor.ImplementationType);
            Assert.Null(serviceDescriptor.ImplementationInstance);
        }

        [Fact]
        public void TransientCreatesDescriptorForInstance()
        {
            var implementationInstance = new Component();

            var serviceDescriptor = ServiceDescriptor.Transient<IComponent>(implementationInstance);

            Assert.Equal(typeof(IComponent), serviceDescriptor.ServiceType);
            Assert.Same(implementationInstance, serviceDescriptor.ImplementationInstance);
            Assert.Null(serviceDescriptor.ImplementationType);
        }
    }
}
