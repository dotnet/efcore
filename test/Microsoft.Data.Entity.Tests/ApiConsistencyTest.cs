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

using System.Linq;
using System.Reflection;
using Microsoft.Data.Entity.Metadata;
using Xunit;

namespace Microsoft.Data.Entity.Tests
{
    public class ApiConsistencyTest : ApiConsistencyTestBase
    {
        [Fact]
        public void Fluent_api_methods_should_not_return_void()
        {
            var fluentApiTypes = new[] { typeof(ModelBuilder) };

            var voidMethods
                = from t in GetAllTypes(fluentApiTypes)
                    where t.IsVisible
                    from m in t.GetMethods(PublicInstance)
                    where m.DeclaringType != null
                          && m.DeclaringType.Assembly == TargetAssembly
                          && m.ReturnType == typeof(void)
                    select t.Name + "." + m.Name;

            Assert.Equal("", string.Join("\r\n", voidMethods));
        }

        protected override Assembly TargetAssembly
        {
            get { return typeof(EntityType).Assembly; }
        }
    }
}
