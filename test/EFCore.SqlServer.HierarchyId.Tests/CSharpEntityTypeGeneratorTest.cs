// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Scaffolding;
using Xunit;

namespace Microsoft.EntityFrameworkCore.SqlServer;

public class CSharpEntityTypeGeneratorTest : ModelCodeGeneratorTestBase
{
    [ConditionalFact]
    public void Class_with_HierarchyId_key_is_generated()
        => Test(
            modelBuilder =>
            {
                modelBuilder.Entity(
                    "Patriarch",
                    b =>
                    {
                        b.Property<HierarchyId>("Id");
                        b.HasKey("Id");
                        b.Property<string>("Name");
                    });
            },
            new ModelCodeGenerationOptions { UseDataAnnotations = true },
            code =>
            {
                AssertFileContents(
                    @"using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TestNamespace;

public partial class Patriarch
{
    [Key]
    public HierarchyId Id { get; set; }

    public string Name { get; set; }
}
",
                    code.AdditionalFiles.Single(f => f.Path == "Patriarch.cs"));
            });

    [ConditionalFact]
    public void Class_with_HierarchyId_property_is_generated()
        => Test(
            modelBuilder =>
            {
                modelBuilder.Entity(
                    "Patriarch",
                    b =>
                    {
                        b.Property<int>("Id");
                        b.HasKey("Id");
                        b.Property<string>("Name");
                        b.Property<HierarchyId>("Hierarchy");
                    });
            },
            new ModelCodeGenerationOptions { UseDataAnnotations = true },
            code =>
            {
                AssertFileContents(
                    @"using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TestNamespace;

public partial class Patriarch
{
    [Key]
    public int Id { get; set; }

    public HierarchyId Hierarchy { get; set; }

    public string Name { get; set; }
}
",
                    code.AdditionalFiles.Single(f => f.Path == "Patriarch.cs"));
            });

    [ConditionalFact]
    public void Class_with_multiple_HierarchyId_properties_are_generated()
        => Test(
            modelBuilder =>
            {
                modelBuilder.Entity(
                    "Patriarch",
                    b =>
                    {
                        b.Property<HierarchyId>("Id");
                        b.HasKey("Id");
                        b.Property<string>("Name");
                        b.Property<HierarchyId>("Hierarchy");
                    });
            },
            new ModelCodeGenerationOptions { UseDataAnnotations = true },
            code =>
            {
                AssertFileContents(
                    @"using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TestNamespace;

public partial class Patriarch
{
    [Key]
    public HierarchyId Id { get; set; }

    public HierarchyId Hierarchy { get; set; }

    public string Name { get; set; }
}
",
                    code.AdditionalFiles.Single(f => f.Path == "Patriarch.cs"));
            });
}
