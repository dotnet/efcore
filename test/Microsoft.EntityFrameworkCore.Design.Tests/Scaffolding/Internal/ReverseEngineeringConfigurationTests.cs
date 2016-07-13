// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Scaffolding.Internal;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Design.Tests.Scaffolding.Internal
{
    public class ReverseEngineeringConfigurationTests
    {
        [Fact]
        public void Throws_exceptions_for_incorrect_configuration()
        {
            var configuration = new ReverseEngineeringConfiguration
            {
                ConnectionString = null,
                ProjectPath = null,
                OutputPath = null
            };

            Assert.Equal(DesignStrings.ConnectionStringRequired,
                Assert.Throws<ArgumentException>(
                    () => configuration.CheckValidity()).Message);

            configuration.ConnectionString = "NonEmptyConnectionString";
            Assert.Equal(DesignStrings.ProjectPathRequired,
                Assert.Throws<ArgumentException>(
                    () => configuration.CheckValidity()).Message);

            configuration.ProjectPath = "NonEmptyProjectPath";
            Assert.Equal(DesignStrings.RootNamespaceRequired,
                Assert.Throws<ArgumentException>(
                    () => configuration.CheckValidity()).Message);

            configuration.ContextClassName = @"Invalid!CSharp*Class&Name";
            Assert.Equal(DesignStrings.ContextClassNotValidCSharpIdentifier(@"Invalid!CSharp*Class&Name"),
                Assert.Throws<ArgumentException>(
                    () => configuration.CheckValidity()).Message);

            configuration.ContextClassName = "1CSharpClassNameCannotStartWithNumber";
            Assert.Equal(DesignStrings.ContextClassNotValidCSharpIdentifier("1CSharpClassNameCannotStartWithNumber"),
                Assert.Throws<ArgumentException>(
                    () => configuration.CheckValidity()).Message);

            configuration.ContextClassName = "volatile"; // cannot be C# keyword
            Assert.Equal(DesignStrings.ContextClassNotValidCSharpIdentifier("volatile"),
                Assert.Throws<ArgumentException>(
                    () => configuration.CheckValidity()).Message);

            configuration.ContextClassName = "GoodClassName";
            configuration.OutputPath = @"\AnAbsolutePath";
            Assert.Equal(DesignStrings.RootNamespaceRequired,
                Assert.Throws<ArgumentException>(
                    () => configuration.CheckValidity()).Message);

            configuration.OutputPath = @"A\Relative\Path";
            Assert.Equal(DesignStrings.RootNamespaceRequired,
                Assert.Throws<ArgumentException>(
                    () => configuration.CheckValidity()).Message);
        }
    }
}
