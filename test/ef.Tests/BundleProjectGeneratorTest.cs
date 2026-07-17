// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Tools.Generators;
using System.Xml.Linq;

namespace Microsoft.EntityFrameworkCore.Tools;

public class BundleProjectGeneratorTest
{
    [Fact]
    public void Generates_conditional_package_references_for_CPM()
    {
        var generator = new BundleProjectGenerator
        {
            Session = new Dictionary<string, object>
            {
                ["TargetFramework"] = "net11.0",
                ["EFCoreVersion"] = "11.0.0",
                ["Project"] = "/src/project/project.csproj",
                ["StartupProject"] = "/src/project/project.csproj"
            }
        };
        generator.Initialize();

        var project = generator.TransformText();
        var packageReferences = XDocument.Parse(project)
            .Descendants("PackageReference")
            .Where(
                e => string.Equals(
                    (string?)e.Attribute("Include"),
                    "Microsoft.EntityFrameworkCore.Design",
                    StringComparison.Ordinal))
            .ToList();

        Assert.Equal(2, packageReferences.Count);
        Assert.Contains(
            packageReferences,
            e => (string?)e.Attribute("Version") == "11.0.0"
                && string.Equals(
                    (string?)e.Attribute("Condition"),
                    "'$(ManagePackageVersionsCentrally)' != 'true'",
                    StringComparison.Ordinal));
        Assert.Contains(
            packageReferences,
            e => e.Attribute("Version") == null
                && string.Equals(
                    (string?)e.Attribute("Condition"),
                    "'$(ManagePackageVersionsCentrally)' == 'true'",
                    StringComparison.Ordinal));
    }
}
