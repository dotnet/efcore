// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.IO;
using Microsoft.EntityFrameworkCore.Scaffolding.Internal;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Design.Tests.Scaffolding.Internal
{
    public class ReverseEngineeringGeneratorTests
    {
        [Theory]
        [MemberData(nameof(NamespaceAndPathOptions))]
        public void Constructs_correct_canonical_paths(
            string rootNamespace, string projectPath, string outputPath, string expectedNamepace,
            string canonicalizedFullOutputPath, string canonicalizedRelativeOutputPath)
        {
            var outputNamespaceAndPaths = ReverseEngineeringGenerator.ConstructNamespaceAndCanonicalizedPaths(
                rootNamespace, projectPath, outputPath);
            Assert.Equal(expectedNamepace, outputNamespaceAndPaths.Namespace);
            Assert.Equal(canonicalizedFullOutputPath, outputNamespaceAndPaths.CanonicalizedFullOutputPath);
            Assert.Equal(canonicalizedRelativeOutputPath, outputNamespaceAndPaths.CanonicalizedRelativeOutputPath);
        }

        public static TheoryData NamespaceAndPathOptions
        {
            get
            {
                var data = new TheoryData<string, string, string, string, string, string>();

                if (Path.DirectorySeparatorChar == '\\'
                    || Path.AltDirectorySeparatorChar == '\\')
                {
                    data.Add("Root.Namespace", @"X:\project\Path", null, "Root.Namespace", @"X:\project\Path", string.Empty);
                    data.Add("Root.Namespace", @"X:\project\Path", string.Empty, "Root.Namespace", @"X:\project\Path", string.Empty);
                    data.Add("Root.Namespace", @"X:\project\Path", @"X:\Absolute\Output\Path", "Root.Namespace", @"X:\Absolute\Output\Path", null);
                    data.Add("Root.Namespace", @"X:\project\Path", @"..\..\Path\Outside\Project", "Root.Namespace", @"X:\Path\Outside\Project", null);
                    data.Add("Root.Namespace", @"X:\project\Path", @"Path\Inside\Project", "Root.Namespace.Path.Inside.Project", @"X:\project\Path\Path\Inside\Project", @"Path\Inside\Project");
                    data.Add("Root.Namespace", @"X:\project\Path", @"Path\.\Inside\Project", "Root.Namespace.Path.Inside.Project", @"X:\project\Path\Path\Inside\Project", @"Path\Inside\Project");
                    data.Add("Root.Namespace", @"X:\project\Path", @"FirstDir\IgnoreThisDir\..\AnotherDir", "Root.Namespace.FirstDir.AnotherDir", @"X:\project\Path\FirstDir\AnotherDir", @"FirstDir\AnotherDir");
                    data.Add("Root.Namespace", @"X:\project\Path", @"Keyword\volatile\123\Bad!$&Chars", "Root.Namespace.Keyword._volatile._123.Bad___Chars", @"X:\project\Path\Keyword\volatile\123\Bad!$&Chars", @"Keyword\volatile\123\Bad!$&Chars");
                }
                else
                {
                    data.Add("Root.Namespace", "/project/Path", null, "Root.Namespace", "/project/Path", string.Empty);
                    data.Add("Root.Namespace", "/project/Path", string.Empty, "Root.Namespace", "/project/Path", string.Empty);
                    data.Add("Root.Namespace", "/project/Path", "/Absolute/Output/Path", "Root.Namespace", "/Absolute/Output/Path", null);
                    data.Add("Root.Namespace", "/project/Path", "../../Path/Outside/Project", "Root.Namespace", "/Path/Outside/Project", null);
                    data.Add("Root.Namespace", "/project/Path", "Path/Inside/Project", "Root.Namespace.Path.Inside.Project", "/project/Path/Path/Inside/Project", "Path/Inside/Project");
                    data.Add("Root.Namespace", "/project/Path", "Path/./Inside/Project", "Root.Namespace.Path.Inside.Project", "/project/Path/Path/Inside/Project", "Path/Inside/Project");
                    data.Add("Root.Namespace", "/project/Path", "FirstDir/IgnoreThisDir/../AnotherDir", "Root.Namespace.FirstDir.AnotherDir", "/project/Path/FirstDir/AnotherDir", "FirstDir/AnotherDir");
                    data.Add("Root.Namespace", "/project/Path", "Keyword/volatile/123/Bad!$&Chars", "Root.Namespace.Keyword._volatile._123.Bad___Chars", "/project/Path/Keyword/volatile/123/Bad!$&Chars", "Keyword/volatile/123/Bad!$&Chars");
                }

                return data;
            }
        }
    }

    public class TheoryData<T1, T2, T3, T4, T5, T6> : TheoryData
    {
        public void Add(T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6) => AddRow(t1, t2, t3, t4, t5, t6);
    }
}
