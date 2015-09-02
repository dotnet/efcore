// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Xunit;

namespace Microsoft.Data.Entity.Relational.Design.ReverseEngineering.Tests
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
                RelativeOutputPath = null
            };

            Assert.Equal(Strings.ConnectionStringRequired,
                Assert.Throws<ArgumentException>(
                    () => configuration.CheckValidity()).Message);

            configuration.ConnectionString = "NonEmptyConnectionString";
            Assert.Equal(Strings.ProjectPathRequired,
                Assert.Throws<ArgumentException>(
                    () => configuration.CheckValidity()).Message);

            configuration.ProjectPath = "NonEmptyProjectPath";
            Assert.Equal(Strings.RootNamespaceRequired,
                Assert.Throws<ArgumentException>(
                    () => configuration.CheckValidity()).Message);

            configuration.RelativeOutputPath = @"\AnAbsolutePath";
            Assert.Equal(Strings.NotRelativePath(@"\AnAbsolutePath", "NonEmptyProjectPath"),
                Assert.Throws<ArgumentException>(
                    () => configuration.CheckValidity()).Message);

            configuration.RelativeOutputPath = @"Looks\Like\A\RelativePath\..\..\..\..\..\But\Is\Not";
            Assert.Equal(Strings.NotRelativePath(@"Looks\Like\A\RelativePath\..\..\..\..\..\But\Is\Not", "NonEmptyProjectPath"),
                Assert.Throws<ArgumentException>(
                    () => configuration.CheckValidity()).Message);

            configuration.RelativeOutputPath = @"A\Real\RelativePath";
            Assert.Equal(Strings.RootNamespaceRequired,
                Assert.Throws<ArgumentException>(
                    () => configuration.CheckValidity()).Message);
        }
    }
}
