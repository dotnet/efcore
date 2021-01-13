// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore
{
    public class ServiceProviderExtensionsTest
    {
        [ConditionalFact]
        public void GetRequiredService_throws_useful_exception_if_service_not_registered()
        {
            var serviceProvider = new ServiceCollection().BuildServiceProvider();

            Assert.Throws<InvalidOperationException>(
                () => serviceProvider.GetRequiredService<IPilkington>());
        }

        [ConditionalFact]
        public void Non_generic_GetRequiredService_throws_useful_exception_if_service_not_registered()
        {
            var serviceProvider = new ServiceCollection().BuildServiceProvider();

            Assert.Throws<InvalidOperationException>(
                () => serviceProvider.GetRequiredService(typeof(IPilkington)));
        }

        [ConditionalFact]
        public void GetRequiredService_throws_useful_exception_if_resolution_fails()
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddScoped<IPilkington, Karl>();
            var serviceProvider = serviceCollection.BuildServiceProvider();

            Assert.Equal(
                KarlQuote,
                Assert.Throws<NotSupportedException>(
                    () => serviceProvider.GetRequiredService<IPilkington>()).Message);
        }

        [ConditionalFact]
        public void Non_generic_GetRequiredService_throws_useful_exception_if_resolution_fails()
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddScoped<IPilkington, Karl>();

            var serviceProvider = serviceCollection.BuildServiceProvider();

            Assert.Equal(
                KarlQuote,
                Assert.Throws<NotSupportedException>(
                    () => serviceProvider.GetRequiredService(typeof(IPilkington))).Message);
        }

        [ConditionalFact]
        public void GetService_returns_null_if_service_not_registered()
        {
            var serviceProvider = new ServiceCollection().BuildServiceProvider();

            Assert.Null(serviceProvider.GetService<IPilkington>());
        }

        [ConditionalFact]
        public void Non_generic_GetService_returns_null_if_service_not_registered()
        {
            var serviceProvider = new ServiceCollection().BuildServiceProvider();

            Assert.Null(serviceProvider.GetService(typeof(IPilkington)));
        }

        [ConditionalFact]
        public void GetService_throws_useful_exception_if_resolution_fails()
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddScoped<IPilkington, Karl>();
            var serviceProvider = serviceCollection.BuildServiceProvider();

            Assert.Equal(
                KarlQuote,
                Assert.Throws<NotSupportedException>(
                    () => serviceProvider.GetService<IPilkington>()).Message);
        }

        [ConditionalFact]
        public void Non_generic_GetService_throws_useful_exception_if_resolution_fails()
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddScoped<IPilkington, Karl>();

            var serviceProvider = serviceCollection.BuildServiceProvider();

            Assert.Equal(
                KarlQuote,
                Assert.Throws<NotSupportedException>(
                    () => serviceProvider.GetService(typeof(IPilkington))).Message);
        }

        private const string KarlQuote = "You can only talk rubbish if you're aware of knowledge.";

        private interface IPilkington
        {
        }

        private class Karl : IPilkington
        {
            public Karl()
            {
                throw new NotSupportedException(KarlQuote);
            }
        }
    }
}
