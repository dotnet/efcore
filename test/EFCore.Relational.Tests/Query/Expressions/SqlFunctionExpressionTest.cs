// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Query.Expressions
{
    public class SqlFunctionExpressionTest
    {
        [Fact]
        public void Ctor_checks_for_unexpected_instance_mapping()
        {
            var ex = Assert.Throws<ArgumentException>(
                () => new SqlFunctionExpression(
                    instance: null,
                    "Func",
                    returnType: typeof(int),
                    arguments: Enumerable.Empty<Expression>(),
                    instanceTypeMapping: new IntTypeMapping("int")));

            Assert.Contains(RelationalStrings.SqlFunctionUnexpectedInstanceMapping, ex.Message);
            Assert.Equal("instanceTypeMapping", ex.ParamName);
        }

        [Fact]
        public void Ctor_checks_argument_type_mapping_count()
        {
            var ex = Assert.Throws<ArgumentException>(
                () => new SqlFunctionExpression(
                    instance: null,
                    "Func",
                    returnType: typeof(int),
                    arguments: new[] { Expression.Constant(0) },
                    argumentTypeMappings: Enumerable.Empty<RelationalTypeMapping>()));

            Assert.Contains(RelationalStrings.SqlFunctionArgumentsAndMappingsMismatch, ex.Message);
            Assert.Equal("argumentTypeMappings", ex.ParamName);
        }

        [Fact]
        public void Ctor_checks_for_null_argument_type_mappings()
        {
            var ex = Assert.Throws<ArgumentException>(
                () => new SqlFunctionExpression(
                    instance: null,
                    "Func",
                    returnType: typeof(int),
                    arguments: new[] { Expression.Constant(0) },
                    argumentTypeMappings: new RelationalTypeMapping[] { null }));

            Assert.Contains(RelationalStrings.SqlFunctionNullArgumentMapping, ex.Message);
            Assert.Equal("argumentTypeMappings", ex.ParamName);
        }
    }
}
