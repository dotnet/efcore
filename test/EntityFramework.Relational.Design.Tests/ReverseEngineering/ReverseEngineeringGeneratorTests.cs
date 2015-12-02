// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using Microsoft.Data.Entity.Scaffolding.Internal;
using Xunit;

namespace Microsoft.Data.Entity.Relational.Design.ReverseEngineering
{
    public class ReverseEngineeringGeneratorTests
    {
        [Theory]
        [MemberData(nameof(NamespaceOptions))]
        public void Constructs_correct_namespace(
            string rootNamespace, string projectPath, string outputPath, string resultingNamespace)
        {
            var outputPaths = ReverseEngineeringGenerator.ConstructCanonicalizedPaths(projectPath, outputPath);
            Assert.Equal(resultingNamespace,
                ReverseEngineeringGenerator.ConstructNamespace(rootNamespace, outputPaths.CanonicalizedRelativeOutputPath));
        }

        [Theory]
        [MemberData(nameof(PathOptions))]
        public void Constructs_correct_canonical_paths(
            string projectPath, string outputPath,
            string canonicalizedFullOutputPath, string canonicalizedRelativeOutputPath)
        {
            var outputPaths = ReverseEngineeringGenerator.ConstructCanonicalizedPaths(projectPath, outputPath);
            Assert.Equal(canonicalizedFullOutputPath, outputPaths.CanonicalizedFullOutputPath);
            Assert.Equal(canonicalizedRelativeOutputPath, outputPaths.CanonicalizedRelativeOutputPath);
        }

        public static TheoryData NamespaceOptions
        {
            get
            {
                var data = new TheoryData<string, string, string, string>
                {
                    { "Root.Namespace", "project/Path", null, "Root.Namespace" },
                    { "Root.Namespace", "project/Path", "", "Root.Namespace" },
                    { "Root.Namespace", "project/Path", "/Absolute/Output/Path", "Root.Namespace" },
                    { "Root.Namespace", "project/Path", "../../Path/Outside/Project", "Root.Namespace" },
                    { "Root.Namespace", "project/Path", "Path/Inside/Project", "Root.Namespace.Path.Inside.Project" },
                    { "Root.Namespace", "project/Path", "Keyword/volatile/123/Bad!$&Chars", "Root.Namespace.Keyword._volatile._123.Bad___Chars" }
                };

                if (Path.DirectorySeparatorChar == '\\'
                    || Path.AltDirectorySeparatorChar == '\\')
                {
                    data.Add("Root.Namespace", @"project\Path", @"X:\Absolute\Output\Path", "Root.Namespace");
                    data.Add("Root.Namespace", @"project\Path", @"\Absolute\Output\Path", "Root.Namespace");
                    data.Add("Root.Namespace", @"project\Path", @"..\..\Path\Outside\Project", "Root.Namespace");
                    data.Add("Root.Namespace", @"project\Path", @"Path\Inside\Project", "Root.Namespace.Path.Inside.Project");
                    data.Add("Root.Namespace", @"project\Path", @"Keyword\volatile\123\Bad!$&Chars", "Root.Namespace.Keyword._volatile._123.Bad___Chars");
                }

                return data;
            }
        }

        public static TheoryData PathOptions
        {
            get
            {
                var data = new TheoryData<string, string, string, string>();

                if (Path.DirectorySeparatorChar == '\\'
                    || Path.AltDirectorySeparatorChar == '\\')
                {
                    data.Add(@"X:\project\Path", null, @"X:\project\Path", string.Empty);
                    data.Add(@"X:\project\Path", string.Empty, @"X:\project\Path", string.Empty);
                    data.Add(@"X:\project\Path", @"X:\Absolute\Output\Path", @"X:\Absolute\Output\Path", null);
                    data.Add(@"X:\project\Path", @"..\..\Path\Outside\Project", @"X:\Path\Outside\Project", null);
                    data.Add(@"X:\project\Path", @"Path\Inside\Project", @"X:\project\Path\Path\Inside\Project", @"Path\Inside\Project");
                    data.Add(@"X:\project\Path", @"Path\.\Inside\Project", @"X:\project\Path\Path\Inside\Project", @"Path\Inside\Project");
                    data.Add(@"X:\project\Path", @"FirstDir\IgnoreThisDir\..\AnotherDir", @"X:\project\Path\FirstDir\AnotherDir", @"FirstDir\AnotherDir");
                }
                else
                {
                    data.Add("/project/Path", null, "/project/Path", string.Empty);
                    data.Add("/project/Path", string.Empty, "project/Path", string.Empty);
                    data.Add("/project/Path", "/Absolute/Output/Path", "/Absolute/Output/Path", null);
                    data.Add("/project/Path", "../../Path/Outside/Project", "/Path/Outside/Project", null);
                    data.Add("/project/Path", "Path/Inside/Project", "/project/Path/Path/Inside/Project", "Path/Inside/Project");
                    data.Add("/project/Path", "Path/./Inside/Project", "/project/Path/Path/Inside/Project", "Path/Inside/Project");
                    data.Add("/project/Path", "FirstDir/IgnoreThisDir/../AnotherDir", "/project/Path/FirstDir/AnotherDir", "FirstDir/AnotherDir");
                }

                return data;
            }
        }
    }
}
