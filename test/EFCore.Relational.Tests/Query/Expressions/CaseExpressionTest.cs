// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Internal;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Query.Expressions
{
    public class CaseExpressionTest
    {
        [Fact]
        public void Ctor_checks_test_types()
        {
            var ex = Assert.Throws<ArgumentException>(
                () => new CaseExpression(
                    operand: Expression.Constant(false),
                    new CaseWhenClause(Expression.Constant(0), Expression.Constant(false))));

            Assert.Contains(
                RelationalStrings.CaseWhenClauseTestTypeUnexpected(typeof(int), typeof(bool)),
                ex.Message);
            Assert.Equal("whenClauses", ex.ParamName);
        }

        [Fact]
        public void Ctor_checks_when_result_types()
        {
            var ex = Assert.Throws<ArgumentException>(
                () => new CaseExpression(
                    new CaseWhenClause(Expression.Constant(true), Expression.Constant(false)),
                    new CaseWhenClause(Expression.Constant(false), Expression.Constant(0))));

            Assert.Contains(
                RelationalStrings.CaseWhenClauseResultTypeUnexpected(typeof(int), typeof(bool)),
                ex.Message);
            Assert.Equal("whenClauses", ex.ParamName);
        }

        [Fact]
        public void Ctor_checks_else_result_type()
        {
            var ex = Assert.Throws<ArgumentException>(
                () => new CaseExpression(
                    new[] { new CaseWhenClause(Expression.Constant(false), Expression.Constant(false)) },
                    Expression.Constant(0)));

            Assert.Contains(
                RelationalStrings.CaseElseResultTypeUnexpected(typeof(int), typeof(bool)),
                ex.Message);
            Assert.Equal("elseResult", ex.ParamName);
        }
    }
}
