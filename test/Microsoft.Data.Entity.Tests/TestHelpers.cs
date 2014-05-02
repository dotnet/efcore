// Copyright (c) Microsoft Open Technologies, Inc.
// All Rights Reserved
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// THIS CODE IS PROVIDED *AS IS* BASIS, WITHOUT WARRANTIES OR
// CONDITIONS OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING
// WITHOUT LIMITATION ANY IMPLIED WARRANTIES OR CONDITIONS OF
// TITLE, FITNESS FOR A PARTICULAR PURPOSE, MERCHANTABLITY OR
// NON-INFRINGEMENT.
// See the Apache 2 License for the specific language governing
// permissions and limitations under the License.

using System;
using Microsoft.AspNet.DependencyInjection;
using Microsoft.AspNet.DependencyInjection.Fallback;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.InMemory;

namespace Microsoft.Data.Entity.Tests
{
    public static class TestHelpers
    {
        public static EntityConfiguration CreateEntityConfiguration(IModel model)
        {
            return new EntityConfigurationBuilder()
                .UseModel(model)
                .BuildConfiguration();
        }

        public static EntityConfiguration CreateEntityConfiguration()
        {
            return new EntityConfigurationBuilder()
                .BuildConfiguration();
        }

        public static IServiceProvider CreateServiceProvider()
        {
            return new ServiceCollection()
                .AddEntityFramework(s => s.AddInMemoryStore())
                .BuildServiceProvider();
        }

        public static ContextConfiguration CreateContextConfiguration(IServiceProvider serviceProvider, IModel model)
        {
            return new DbContext(serviceProvider, CreateEntityConfiguration(model)).Configuration;
        }

        public static ContextConfiguration CreateContextConfiguration(IServiceProvider serviceProvider)
        {
            return new DbContext(serviceProvider, CreateEntityConfiguration()).Configuration;
        }

        public static ContextConfiguration CreateContextConfiguration(IModel model)
        {
            return new DbContext(CreateServiceProvider(), CreateEntityConfiguration(model)).Configuration;
        }

        public static ContextConfiguration CreateContextConfiguration()
        {
            return new DbContext(CreateServiceProvider(), CreateEntityConfiguration()).Configuration;
        }
    }
}
