// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;
using Microsoft.Data.Entity.AzureTableStorage.Query;
using Microsoft.Data.Entity.AzureTableStorage.Tests.Helpers;
using Microsoft.Data.Entity.Metadata;
using Xunit;

namespace Microsoft.Data.Entity.AzureTableStorage.Tests.Query
{
    public class TableFilterFactoryTests
    {
        private TableFilterFactory _factory;
        private EntityType _entityType;

        public TableFilterFactoryTests()
        {
            _factory = new TableFilterFactory();
            _entityType = PocoTestType.EntityType();
        }

        [Fact]
        public void It_makes_member_to_constant_expression()
        {
            var expression =
                Expression.MakeBinary(
                    ExpressionType.Equal,
                    Expression.Constant(5),
                    Expression.MakeMemberAccess(
                        Expression.New(
                            typeof(PocoTestType)),
                        typeof(PocoTestType).GetProperty("Count")
                        )
                    );
            var filter = _factory.TryCreate(expression, _entityType);
            Assert.NotNull(filter);
            Assert.Equal("Count eq 5", filter.ToString());
        }

        [Fact]
        public void It_supports_static_field_expressions()
        {
            var expression =
                Expression.MakeBinary(
                    ExpressionType.Equal,
                    Expression.MakeMemberAccess(
                        Expression.New(
                            typeof(PocoTestType)),
                        typeof(PocoTestType).GetProperty("Count")
                        ),
                    Expression.Field(
                        null,
                        typeof(WithMethodType).GetField("StaticIntField")
                        )
                    );
            var filter = _factory.TryCreate(expression, _entityType);
            Assert.NotNull(filter);
            Assert.Equal("Count eq 34", filter.ToString());
        }

        [Fact]
        public void It_supports_instance_field_expressions()
        {
            var expression =
                Expression.MakeBinary(
                    ExpressionType.Equal,
                    Expression.MakeMemberAccess(
                        Expression.New(
                            typeof(PocoTestType)),
                        typeof(PocoTestType).GetProperty("Count")
                        ),
                    Expression.Field(
                        Expression.Constant(new WithMethodType()),
                        typeof(WithMethodType).GetField("InstanceIntField")
                        )
                    );
            var filter = _factory.TryCreate(expression, _entityType);
            Assert.NotNull(filter);
            Assert.Equal("Count eq 842", filter.ToString());
        }

        [Fact]
        public void It_supports_new_expressions()
        {
            var expression =
                Expression.MakeBinary(
                    ExpressionType.Equal,
                    Expression.MakeMemberAccess(
                        Expression.New(
                            typeof(PocoTestType)),
                        typeof(PocoTestType).GetProperty("Guid")
                        ),
                    Expression.New(
                        typeof(Guid).GetConstructor(new Type[] { typeof(string) }),
                        new Expression[] { Expression.Constant("ca761232ed4211cebacd00aa0057b223") }
                        )
                    );
            var filter = _factory.TryCreate(expression, _entityType);
            Assert.NotNull(filter);
            Assert.Equal("Guid eq guid'ca761232-ed42-11ce-bacd-00aa0057b223'", filter.ToString());
        }

        [Fact]
        public void It_ignores_nested_types()
        {
            var expression =
                Expression.MakeBinary(
                    ExpressionType.Equal,
                    Expression.MakeMemberAccess(
                        Expression.MakeMemberAccess(Expression.New(typeof(PocoTestType)), typeof(PocoTestType).GetProperty("NestedObj")),
                        typeof(PocoTestType).GetProperty("Count")
                        ),
                    Expression.Constant(5)
                    );
            var filter = _factory.TryCreate(expression, _entityType);
            Assert.Null(filter);
        }

        internal class WithMethodType : PocoTestType
        {
            public WithMethodType()
            {
                InstanceIntField = 842;
            }

            public static int StaticIntField = 34;
            public int InstanceIntField;
        }
    }
}
