// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Xunit;
using System.Reflection;
using System.Linq.Expressions;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Specification.Tests.TestUtilities.Xunit;

namespace Microsoft.EntityFrameworkCore.Tests
{
    public class ExpressionCompareByHashCodeTest
    {
        [ConditionalFact]
        public void Compare_member_init_expressions_by_hash_code()
        {
            MethodInfo addMethod = typeof(List<string>).GetMethod("Add");

            MemberListBinding bindingMessages = Expression.ListBind(
                typeof(Node).GetProperty("Messages"),
                Expression.ElementInit(addMethod, Expression.Constant("Greeting from PVS-Studio developers!"))
            );

            MemberListBinding bindingDescriptions = Expression.ListBind(
                typeof(Node).GetProperty("Descriptions"),
                Expression.ElementInit(addMethod, Expression.Constant("PVS-Studio is a static code analyzer for C, C++ and C#."))
            );

            Expression query1 = Expression.MemberInit(
                Expression.New(typeof(Node)),
                new List<MemberBinding>() {
                    bindingMessages
                }
            );

            Expression query2 = Expression.MemberInit(
                Expression.New(typeof(Node)),
                new List<MemberBinding>() {
                    bindingMessages,
                    bindingDescriptions
                }
            );

            var comparer = new ExpressionEqualityComparer();
            var key1Hash = comparer.GetHashCode(query1);
            var key2Hash = comparer.GetHashCode(query2);

            Assert.NotEqual(key1Hash, key2Hash);
        }

        private class Node
        {
            public List<string> Messages { set; get; }
            public List<string> Descriptions { set; get; }
        }
    }
}
