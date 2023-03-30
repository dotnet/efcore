﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.SqlServer.Storage.Internal;

namespace Microsoft.EntityFrameworkCore.SqlServer.Test.Models.Migrations;

internal sealed class TypedArraySeedContext : MigrationContext<Patriarch, ConvertedPatriarch>
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        RemoveVariableModelAnnotations(modelBuilder);

        modelBuilder.Entity<Patriarch>().HasData(
            new Patriarch { Id = HierarchyId.GetRoot(), Name = "Eddard Stark" },
            new Patriarch { Id = HierarchyId.Parse("/1/"), Name = "Robb Stark" },
            new Patriarch { Id = HierarchyId.Parse("/2/"), Name = "Jon Snow" });

        modelBuilder.Entity<ConvertedPatriarch>(
            b =>
            {
                b.Property(e => e.HierarchyId)
                    .HasConversion(v => HierarchyId.Parse(v), v => v.ToString());

                b.HasData(
                    new ConvertedPatriarch
                    {
                        Id = 1,
                        HierarchyId = HierarchyId.GetRoot().ToString(),
                        Name = "Eddard Stark"
                    },
                    new ConvertedPatriarch
                    {
                        Id = 2,
                        HierarchyId = HierarchyId.Parse("/1/").ToString(),
                        Name = "Robb Stark"
                    },
                    new ConvertedPatriarch
                    {
                        Id = 3,
                        HierarchyId = HierarchyId.Parse("/2/").ToString(),
                        Name = "Jon Snow"
                    });
            });
    }

    public override string GetExpectedMigrationCode(string migrationName, string rootNamespace)
    {
        return $@"using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.SqlServer.Types;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace {rootNamespace}.Migrations
{{
    /// <inheritdoc />
    public partial class {migrationName} : Migration
    {{
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {{
            migrationBuilder.CreateTable(
                name: ""{nameof(ConvertedTestModels)}"",
                columns: table => new
                {{
                    {nameof(ConvertedPatriarch.Id)} = table.Column<int>(type: ""int"", nullable: false),
                    {nameof(ConvertedPatriarch.HierarchyId)} = table.Column<SqlHierarchyId>(type: ""hierarchyid"", nullable: true),
                    {nameof(ConvertedPatriarch.Name)} = table.Column<string>(type: ""nvarchar(max)"", nullable: true)
                }},
                constraints: table =>
                {{
                    table.PrimaryKey(""PK_{nameof(ConvertedTestModels)}"", x => x.{nameof(ConvertedPatriarch.Id)});
                }});

            migrationBuilder.CreateTable(
                name: ""{nameof(TestModels)}"",
                columns: table => new
                {{
                    {nameof(Patriarch.Id)} = table.Column<SqlHierarchyId>(type: ""hierarchyid"", nullable: false),
                    {nameof(Patriarch.Name)} = table.Column<string>(type: ""nvarchar(max)"", nullable: true)
                }},
                constraints: table =>
                {{
                    table.PrimaryKey(""PK_{nameof(TestModels)}"", x => x.{nameof(Patriarch.Id)});
                }});

            migrationBuilder.InsertData(
                table: ""ConvertedTestModels"",
                columns: new[] {{ ""Id"", ""HierarchyId"", ""Name"" }},
                values: new object[,]
                {{
                    {{ 1, Microsoft.SqlServer.Types.SqlHierarchyId.Parse(""/""), ""Eddard Stark"" }},
                    {{ 2, Microsoft.SqlServer.Types.SqlHierarchyId.Parse(""/1/""), ""Robb Stark"" }},
                    {{ 3, Microsoft.SqlServer.Types.SqlHierarchyId.Parse(""/2/""), ""Jon Snow"" }}
                }});

            migrationBuilder.InsertData(
                table: ""TestModels"",
                columns: new[] {{ ""Id"", ""Name"" }},
                values: new object[,]
                {{
                    {{ Microsoft.SqlServer.Types.SqlHierarchyId.Parse(""/""), ""Eddard Stark"" }},
                    {{ Microsoft.SqlServer.Types.SqlHierarchyId.Parse(""/1/""), ""Robb Stark"" }},
                    {{ Microsoft.SqlServer.Types.SqlHierarchyId.Parse(""/2/""), ""Jon Snow"" }}
                }});
        }}

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {{
            migrationBuilder.DropTable(
                name: ""ConvertedTestModels"");

            migrationBuilder.DropTable(
                name: ""{nameof(TestModels)}"");
        }}
    }}
}}
";
    }

    public override string GetExpectedSnapshotCode(string rootNamespace)
    {
        return $@"// <auto-generated />
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using {ThisType.Namespace};
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.SqlServer.Types;

#nullable disable

namespace {rootNamespace}.Migrations
{{
    [DbContext(typeof({ThisType.Name}))]
    partial class {ThisType.Name}ModelSnapshot : ModelSnapshot
    {{
        protected override void BuildModel(ModelBuilder modelBuilder)
        {{
#pragma warning disable 612, 618

            modelBuilder.Entity(""{ModelType2.FullName}"", b =>
                {{
                    b.Property<int>(""Id"")
                        .ValueGeneratedOnAdd()
                        .HasColumnType(""int"");

                    b.Property<HierarchyId>(""HierarchyId"")
                        .HasColumnType(""hierarchyid"");

                    b.Property<string>(""Name"")
                        .HasColumnType(""nvarchar(max)"");

                    b.HasKey(""Id"");

                    b.ToTable(""ConvertedTestModels"");

                    b.HasData(
                        new
                        {{
                            Id = 1,
                            HierarchyId = Microsoft.SqlServer.Types.SqlHierarchyId.Parse(""/""),
                            Name = ""Eddard Stark""
                        }},
                        new
                        {{
                            Id = 2,
                            HierarchyId = Microsoft.SqlServer.Types.SqlHierarchyId.Parse(""/1/""),
                            Name = ""Robb Stark""
                        }},
                        new
                        {{
                            Id = 3,
                            HierarchyId = Microsoft.SqlServer.Types.SqlHierarchyId.Parse(""/2/""),
                            Name = ""Jon Snow""
                        }});
                }});

            modelBuilder.Entity(""{ModelType1.FullName}"", b =>
                {{
                    b.Property<SqlHierarchyId>(""{nameof(Patriarch.Id)}"")
                        .HasColumnType(""hierarchyid"");

                    b.Property<string>(""{nameof(Patriarch.Name)}"")
                        .HasColumnType(""nvarchar(max)"");

                    b.HasKey(""{nameof(Patriarch.Id)}"");

                    b.ToTable(""{nameof(TestModels)}"");

                    b.HasData(
                        new
                        {{
                            {nameof(Patriarch.Id)} = Microsoft.SqlServer.Types.SqlHierarchyId.Parse(""/""),
                            {nameof(Patriarch.Name)} = ""Eddard Stark""
                        }},
                        new
                        {{
                            {nameof(Patriarch.Id)} = Microsoft.SqlServer.Types.SqlHierarchyId.Parse(""/1/""),
                            {nameof(Patriarch.Name)} = ""Robb Stark""
                        }},
                        new
                        {{
                            {nameof(Patriarch.Id)} = Microsoft.SqlServer.Types.SqlHierarchyId.Parse(""/2/""),
                            {nameof(Patriarch.Name)} = ""Jon Snow""
                        }});
                }});
#pragma warning restore 612, 618
        }}
    }}
}}
";
    }
}
