// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Data.Entity.Internal;
using Xunit;

namespace Microsoft.Data.Entity.Scaffolding
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

            Assert.Equal(RelationalDesignStrings.ConnectionStringRequired,
                Assert.Throws<ArgumentException>(
                    () => configuration.CheckValidity()).Message);

            configuration.ConnectionString = "NonEmptyConnectionString";
            Assert.Equal(RelationalDesignStrings.ProjectPathRequired,
                Assert.Throws<ArgumentException>(
                    () => configuration.CheckValidity()).Message);

            configuration.ProjectPath = "NonEmptyProjectPath";
            Assert.Equal(RelationalDesignStrings.RootNamespaceRequired,
                Assert.Throws<ArgumentException>(
                    () => configuration.CheckValidity()).Message);

            configuration.ContextClassName = @"Invalid!CSharp*Class&Name";
            Assert.Equal(RelationalDesignStrings.ContextClassNotValidCSharpIdentifier(@"Invalid!CSharp*Class&Name"),
                Assert.Throws<ArgumentException>(
                    () => configuration.CheckValidity()).Message);

            configuration.ContextClassName = "1CSharpClassNameCannotStartWithNumber";
            Assert.Equal(RelationalDesignStrings.ContextClassNotValidCSharpIdentifier("1CSharpClassNameCannotStartWithNumber"),
                Assert.Throws<ArgumentException>(
                    () => configuration.CheckValidity()).Message);

            configuration.ContextClassName = "volatile";  // cannot be C# keyword
            Assert.Equal(RelationalDesignStrings.ContextClassNotValidCSharpIdentifier("volatile"),
                Assert.Throws<ArgumentException>(
                    () => configuration.CheckValidity()).Message);

            configuration.ContextClassName = "GoodClassName";
            configuration.OutputPath = @"\AnAbsolutePath";
            Assert.Equal(RelationalDesignStrings.RootNamespaceRequired,
                Assert.Throws<ArgumentException>(
                    () => configuration.CheckValidity()).Message);

            configuration.OutputPath = @"A\Relative\Path";
            Assert.Equal(RelationalDesignStrings.RootNamespaceRequired,
                Assert.Throws<ArgumentException>(
                    () => configuration.CheckValidity()).Message);
        }
    }
}
