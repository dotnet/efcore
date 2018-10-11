// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Xunit;

// ReSharper disable AssignNullToNotNullAttribute

namespace Microsoft.EntityFrameworkCore.Query
{
    public class ExpressionEqualityComparerTest
    {
        [Fact]
        public void Member_init_expressions_are_compared_correctly()
        {
            var expressionComparer = ExpressionEqualityComparer.Instance;

            var addMethod = typeof(List<string>).GetTypeInfo().GetDeclaredMethod("Add");

            var bindingMessages = Expression.ListBind(
                typeof(Node).GetProperty("Messages"),
                Expression.ElementInit(addMethod, Expression.Constant("Constant1"))
            );

            var bindingDescriptions = Expression.ListBind(
                typeof(Node).GetProperty("Descriptions"),
                Expression.ElementInit(addMethod, Expression.Constant("Constant2"))
            );

            Expression e1 = Expression.MemberInit(
                Expression.New(typeof(Node)),
                new List<MemberBinding>
                {
                    bindingMessages
                }
            );

            Expression e2 = Expression.MemberInit(
                Expression.New(typeof(Node)),
                new List<MemberBinding>
                {
                    bindingMessages,
                    bindingDescriptions
                }
            );

            Assert.NotEqual(expressionComparer.GetHashCode(e1), expressionComparer.GetHashCode(e2));
            Assert.False(expressionComparer.Equals(e1, e2));
            Assert.Equal(expressionComparer.GetHashCode(e1), expressionComparer.GetHashCode(e1));
            Assert.True(expressionComparer.Equals(e1, e1));
        }

        [Fact]
        public void Default_expressions_are_compared_correctly()
        {
            var expressionComparer = ExpressionEqualityComparer.Instance;
            
            Expression e1 = Expression.Default(typeof(int));
            Expression e2 = Expression.Default(typeof(int));
            Expression e3 = Expression.Default(typeof(string));            

            Assert.Equal(expressionComparer.GetHashCode(e1), expressionComparer.GetHashCode(e2));
            Assert.NotEqual(expressionComparer.GetHashCode(e1), expressionComparer.GetHashCode(e3));
            Assert.True(expressionComparer.Equals(e1, e2));
            Assert.False(expressionComparer.Equals(e1, e3));
            Assert.Equal(expressionComparer.GetHashCode(e1), expressionComparer.GetHashCode(e1));
            Assert.True(expressionComparer.Equals(e1, e1));
        }

        private class Node
        {
            [UsedImplicitly]
            public List<string> Messages { set; get; }

            [UsedImplicitly]
            public List<string> Descriptions { set; get; }
        }
    }
}
