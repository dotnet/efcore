// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Xunit;

namespace Microsoft.EntityFrameworkCore.SqlServer.Tests.Metadata
{
    public class SqlServerComplexPropertyTest
    {
        [Fact]
        public void Can_set_column_name_for_complex_property()
        {
            ComplexPropertyDefinition propDef11, propDef12, propDef21, propDef22;

            var complexUsage = BuildUsage(out propDef11, out propDef12, out propDef21, out propDef22);

            var property = complexUsage.FindProperty("StringProp");
            var nestedProperty = complexUsage.FindComplexTypeUsage("Nested").FindProperty("StringProp");

            Assert.Equal("StringProp", property.SqlServer().ColumnName);
            Assert.Equal("StringProp", nestedProperty.SqlServer().ColumnName);
            Assert.Null(propDef12.SqlServer().ColumnNameDefault);
            Assert.Null(propDef22.SqlServer().ColumnNameDefault);

            propDef12.SqlServer().ColumnNameDefault = "Col1";
            propDef22.SetAnnotation(SqlServerFullAnnotationNames.Instance.ColumnName, "Col2", ConfigurationSource.DataAnnotation);

            Assert.Equal("Col1", propDef12.SqlServer().ColumnNameDefault);
            Assert.Equal("Col2", propDef22.SqlServer().ColumnNameDefault);

            Assert.Equal(ConfigurationSource.Explicit, propDef12.FindAnnotation(SqlServerFullAnnotationNames.Instance.ColumnName).GetConfigurationSource());
            Assert.Equal(ConfigurationSource.DataAnnotation, propDef22.FindAnnotation(SqlServerFullAnnotationNames.Instance.ColumnName).GetConfigurationSource());

            Assert.Equal("Col1", property.SqlServer().ColumnName);
            Assert.Equal("Col2", nestedProperty.SqlServer().ColumnName);

            Assert.Null(property.FindAnnotation(SqlServerFullAnnotationNames.Instance.ColumnName));
            Assert.Null(nestedProperty.FindAnnotation(SqlServerFullAnnotationNames.Instance.ColumnName));

            property.Relational().ColumnName = "ColR1";
            nestedProperty.SetAnnotation(RelationalFullAnnotationNames.Instance.ColumnName, "ColR2", ConfigurationSource.Convention);

            Assert.Equal("ColR1", property.SqlServer().ColumnName);
            Assert.Equal("ColR2", nestedProperty.SqlServer().ColumnName);

            property.SqlServer().ColumnName = "Col3";
            nestedProperty.SetAnnotation(SqlServerFullAnnotationNames.Instance.ColumnName, "Col4", ConfigurationSource.Convention);

            Assert.Equal("Col3", property.SqlServer().ColumnName);
            Assert.Equal("Col4", nestedProperty.SqlServer().ColumnName);

            Assert.Equal(ConfigurationSource.Explicit, property.FindAnnotation(SqlServerFullAnnotationNames.Instance.ColumnName).GetConfigurationSource());
            Assert.Equal(ConfigurationSource.Convention, nestedProperty.FindAnnotation(SqlServerFullAnnotationNames.Instance.ColumnName).GetConfigurationSource());
        }

        [Fact]
        public void Can_set_column_type_for_complex_property()
        {
            ComplexPropertyDefinition propDef11, propDef12, propDef21, propDef22;

            var complexUsage = BuildUsage(out propDef11, out propDef12, out propDef21, out propDef22);

            var property = complexUsage.FindProperty("StringProp");
            var nestedProperty = complexUsage.FindComplexTypeUsage("Nested").FindProperty("StringProp");

            Assert.Null(property.SqlServer().ColumnType);
            Assert.Null(nestedProperty.SqlServer().ColumnType);
            Assert.Null(propDef12.SqlServer().ColumnTypeDefault);
            Assert.Null(propDef22.SqlServer().ColumnTypeDefault);

            propDef12.SqlServer().ColumnTypeDefault = "Col1";
            propDef22.SetAnnotation(SqlServerFullAnnotationNames.Instance.ColumnType, "Col2", ConfigurationSource.DataAnnotation);

            Assert.Equal("Col1", propDef12.SqlServer().ColumnTypeDefault);
            Assert.Equal("Col2", propDef22.SqlServer().ColumnTypeDefault);

            Assert.Equal(ConfigurationSource.Explicit, propDef12.FindAnnotation(SqlServerFullAnnotationNames.Instance.ColumnType).GetConfigurationSource());
            Assert.Equal(ConfigurationSource.DataAnnotation, propDef22.FindAnnotation(SqlServerFullAnnotationNames.Instance.ColumnType).GetConfigurationSource());

            Assert.Equal("Col1", property.SqlServer().ColumnType);
            Assert.Equal("Col2", nestedProperty.SqlServer().ColumnType);

            Assert.Null(property.FindAnnotation(SqlServerFullAnnotationNames.Instance.ColumnType));
            Assert.Null(nestedProperty.FindAnnotation(SqlServerFullAnnotationNames.Instance.ColumnType));

            property.Relational().ColumnType = "ColR1";
            nestedProperty.SetAnnotation(RelationalFullAnnotationNames.Instance.ColumnType, "ColR2", ConfigurationSource.Convention);

            Assert.Equal("ColR1", property.SqlServer().ColumnType);
            Assert.Equal("ColR2", nestedProperty.SqlServer().ColumnType);

            property.SqlServer().ColumnType = "Col3";
            nestedProperty.SetAnnotation(SqlServerFullAnnotationNames.Instance.ColumnType, "Col4", ConfigurationSource.Convention);

            Assert.Equal("Col3", property.SqlServer().ColumnType);
            Assert.Equal("Col4", nestedProperty.SqlServer().ColumnType);

            Assert.Equal(ConfigurationSource.Explicit, property.FindAnnotation(SqlServerFullAnnotationNames.Instance.ColumnType).GetConfigurationSource());
            Assert.Equal(ConfigurationSource.Convention, nestedProperty.FindAnnotation(SqlServerFullAnnotationNames.Instance.ColumnType).GetConfigurationSource());
        }

        [Fact]
        public void Can_set_default_value_SQL_for_complex_property()
        {
            ComplexPropertyDefinition propDef11, propDef12, propDef21, propDef22;

            var complexUsage = BuildUsage(out propDef11, out propDef12, out propDef21, out propDef22);

            var property = complexUsage.FindProperty("StringProp");
            var nestedProperty = complexUsage.FindComplexTypeUsage("Nested").FindProperty("StringProp");

            Assert.Null(property.SqlServer().DefaultValueSql);
            Assert.Null(nestedProperty.SqlServer().DefaultValueSql);
            Assert.Null(propDef12.SqlServer().DefaultValueSqlDefault);
            Assert.Null(propDef22.SqlServer().DefaultValueSqlDefault);

            propDef12.SqlServer().DefaultValueSqlDefault = "Col1";
            propDef22.SetAnnotation(SqlServerFullAnnotationNames.Instance.DefaultValueSql, "Col2", ConfigurationSource.DataAnnotation);

            Assert.Equal("Col1", propDef12.SqlServer().DefaultValueSqlDefault);
            Assert.Equal("Col2", propDef22.SqlServer().DefaultValueSqlDefault);

            Assert.Equal(ConfigurationSource.Explicit, propDef12.FindAnnotation(SqlServerFullAnnotationNames.Instance.DefaultValueSql).GetConfigurationSource());
            Assert.Equal(ConfigurationSource.DataAnnotation, propDef22.FindAnnotation(SqlServerFullAnnotationNames.Instance.DefaultValueSql).GetConfigurationSource());

            Assert.Equal("Col1", property.SqlServer().DefaultValueSql);
            Assert.Equal("Col2", nestedProperty.SqlServer().DefaultValueSql);

            Assert.Null(property.FindAnnotation(SqlServerFullAnnotationNames.Instance.DefaultValueSql));
            Assert.Null(nestedProperty.FindAnnotation(SqlServerFullAnnotationNames.Instance.DefaultValueSql));

            property.Relational().DefaultValueSql = "ColR1";
            nestedProperty.SetAnnotation(RelationalFullAnnotationNames.Instance.DefaultValueSql, "ColR2", ConfigurationSource.Convention);

            Assert.Equal("ColR1", property.SqlServer().DefaultValueSql);
            Assert.Equal("ColR2", nestedProperty.SqlServer().DefaultValueSql);

            property.SqlServer().DefaultValueSql = "Col3";
            nestedProperty.SetAnnotation(SqlServerFullAnnotationNames.Instance.DefaultValueSql, "Col4", ConfigurationSource.Convention);

            Assert.Equal("Col3", property.SqlServer().DefaultValueSql);
            Assert.Equal("Col4", nestedProperty.SqlServer().DefaultValueSql);

            Assert.Equal(ConfigurationSource.Explicit, property.FindAnnotation(SqlServerFullAnnotationNames.Instance.DefaultValueSql).GetConfigurationSource());
            Assert.Equal(ConfigurationSource.Convention, nestedProperty.FindAnnotation(SqlServerFullAnnotationNames.Instance.DefaultValueSql).GetConfigurationSource());
        }

        [Fact]
        public void Can_set_computed_column_SQL_for_complex_property()
        {
            ComplexPropertyDefinition propDef11, propDef12, propDef21, propDef22;

            var complexUsage = BuildUsage(out propDef11, out propDef12, out propDef21, out propDef22);

            var property = complexUsage.FindProperty("StringProp");
            var nestedProperty = complexUsage.FindComplexTypeUsage("Nested").FindProperty("StringProp");

            Assert.Null(property.SqlServer().ComputedColumnSql);
            Assert.Null(nestedProperty.SqlServer().ComputedColumnSql);
            Assert.Null(propDef12.SqlServer().ComputedColumnSqlDefault);
            Assert.Null(propDef22.SqlServer().ComputedColumnSqlDefault);

            propDef12.SqlServer().ComputedColumnSqlDefault = "Col1";
            propDef22.SetAnnotation(SqlServerFullAnnotationNames.Instance.ComputedColumnSql, "Col2", ConfigurationSource.DataAnnotation);

            Assert.Equal("Col1", propDef12.SqlServer().ComputedColumnSqlDefault);
            Assert.Equal("Col2", propDef22.SqlServer().ComputedColumnSqlDefault);

            Assert.Equal(ConfigurationSource.Explicit, propDef12.FindAnnotation(SqlServerFullAnnotationNames.Instance.ComputedColumnSql).GetConfigurationSource());
            Assert.Equal(ConfigurationSource.DataAnnotation, propDef22.FindAnnotation(SqlServerFullAnnotationNames.Instance.ComputedColumnSql).GetConfigurationSource());

            Assert.Equal("Col1", property.SqlServer().ComputedColumnSql);
            Assert.Equal("Col2", nestedProperty.SqlServer().ComputedColumnSql);

            Assert.Null(property.FindAnnotation(SqlServerFullAnnotationNames.Instance.ComputedColumnSql));
            Assert.Null(nestedProperty.FindAnnotation(SqlServerFullAnnotationNames.Instance.ComputedColumnSql));

            property.Relational().ComputedColumnSql = "ColR1";
            nestedProperty.SetAnnotation(RelationalFullAnnotationNames.Instance.ComputedColumnSql, "ColR2", ConfigurationSource.Convention);

            Assert.Equal("ColR1", property.SqlServer().ComputedColumnSql);
            Assert.Equal("ColR2", nestedProperty.SqlServer().ComputedColumnSql);

            property.SqlServer().ComputedColumnSql = "Col3";
            nestedProperty.SetAnnotation(SqlServerFullAnnotationNames.Instance.ComputedColumnSql, "Col4", ConfigurationSource.Convention);

            Assert.Equal("Col3", property.SqlServer().ComputedColumnSql);
            Assert.Equal("Col4", nestedProperty.SqlServer().ComputedColumnSql);

            Assert.Equal(ConfigurationSource.Explicit, property.FindAnnotation(SqlServerFullAnnotationNames.Instance.ComputedColumnSql).GetConfigurationSource());
            Assert.Equal(ConfigurationSource.Convention, nestedProperty.FindAnnotation(SqlServerFullAnnotationNames.Instance.ComputedColumnSql).GetConfigurationSource());
        }

        [Fact]
        public void Can_set_default_value_for_complex_property()
        {
            ComplexPropertyDefinition propDef11, propDef12, propDef21, propDef22;

            var complexUsage = BuildUsage(out propDef11, out propDef12, out propDef21, out propDef22);

            var property = complexUsage.FindProperty("StringProp");
            var nestedProperty = complexUsage.FindComplexTypeUsage("Nested").FindProperty("StringProp");

            Assert.Null(property.SqlServer().DefaultValue);
            Assert.Null(nestedProperty.SqlServer().DefaultValue);
            Assert.Null(propDef12.SqlServer().DefaultValueDefault);
            Assert.Null(propDef22.SqlServer().DefaultValueDefault);

            propDef12.SqlServer().DefaultValueDefault = "Col1";
            propDef22.SetAnnotation(SqlServerFullAnnotationNames.Instance.DefaultValue, "Col2", ConfigurationSource.DataAnnotation);

            Assert.Equal("Col1", propDef12.SqlServer().DefaultValueDefault);
            Assert.Equal("Col2", propDef22.SqlServer().DefaultValueDefault);

            Assert.Equal(ConfigurationSource.Explicit, propDef12.FindAnnotation(SqlServerFullAnnotationNames.Instance.DefaultValue).GetConfigurationSource());
            Assert.Equal(ConfigurationSource.DataAnnotation, propDef22.FindAnnotation(SqlServerFullAnnotationNames.Instance.DefaultValue).GetConfigurationSource());

            Assert.Equal("Col1", property.SqlServer().DefaultValue);
            Assert.Equal("Col2", nestedProperty.SqlServer().DefaultValue);

            Assert.Null(property.FindAnnotation(SqlServerFullAnnotationNames.Instance.DefaultValue));
            Assert.Null(nestedProperty.FindAnnotation(SqlServerFullAnnotationNames.Instance.DefaultValue));

            property.Relational().DefaultValue = "ColR1";
            nestedProperty.SetAnnotation(RelationalFullAnnotationNames.Instance.DefaultValue, "ColR2", ConfigurationSource.Convention);

            Assert.Equal("ColR1", property.SqlServer().DefaultValue);
            Assert.Equal("ColR2", nestedProperty.SqlServer().DefaultValue);

            property.SqlServer().DefaultValue = "Col3";
            nestedProperty.SetAnnotation(SqlServerFullAnnotationNames.Instance.DefaultValue, "Col4", ConfigurationSource.Convention);

            Assert.Equal("Col3", property.SqlServer().DefaultValue);
            Assert.Equal("Col4", nestedProperty.SqlServer().DefaultValue);

            Assert.Equal(ConfigurationSource.Explicit, property.FindAnnotation(SqlServerFullAnnotationNames.Instance.DefaultValue).GetConfigurationSource());
            Assert.Equal(ConfigurationSource.Convention, nestedProperty.FindAnnotation(SqlServerFullAnnotationNames.Instance.DefaultValue).GetConfigurationSource());
        }

        [Fact]
        public void Can_set_value_generation_strategy_for_complex_property()
        {
            ComplexPropertyDefinition propDef11, propDef12, propDef21, propDef22;

            var complexUsage = BuildUsage(out propDef11, out propDef12, out propDef21, out propDef22);

            var property = complexUsage.FindProperty("IntProp");
            var nestedProperty = complexUsage.FindComplexTypeUsage("Nested").FindProperty("IntProp");

            property.ValueGenerated = ValueGenerated.OnAdd;
            nestedProperty.ValueGenerated = ValueGenerated.OnAdd;

            Assert.Null(property.SqlServer().ValueGenerationStrategy);
            Assert.Null(nestedProperty.SqlServer().ValueGenerationStrategy);
            Assert.Null(propDef11.SqlServer().ValueGenerationStrategyDefault);
            Assert.Null(propDef21.SqlServer().ValueGenerationStrategyDefault);

            propDef11.SqlServer().ValueGenerationStrategyDefault = SqlServerValueGenerationStrategy.IdentityColumn;
            propDef21.SetAnnotation(SqlServerFullAnnotationNames.Instance.ValueGenerationStrategy, SqlServerValueGenerationStrategy.IdentityColumn, ConfigurationSource.DataAnnotation);

            Assert.Equal(SqlServerValueGenerationStrategy.IdentityColumn, propDef11.SqlServer().ValueGenerationStrategyDefault);
            Assert.Equal(SqlServerValueGenerationStrategy.IdentityColumn, propDef21.SqlServer().ValueGenerationStrategyDefault);

            Assert.Equal(ConfigurationSource.Explicit, propDef11.FindAnnotation(SqlServerFullAnnotationNames.Instance.ValueGenerationStrategy).GetConfigurationSource());
            Assert.Equal(ConfigurationSource.DataAnnotation, propDef21.FindAnnotation(SqlServerFullAnnotationNames.Instance.ValueGenerationStrategy).GetConfigurationSource());

            Assert.Equal(SqlServerValueGenerationStrategy.IdentityColumn, property.SqlServer().ValueGenerationStrategy);
            Assert.Equal(SqlServerValueGenerationStrategy.IdentityColumn, nestedProperty.SqlServer().ValueGenerationStrategy);

            Assert.Null(property.FindAnnotation(SqlServerFullAnnotationNames.Instance.ValueGenerationStrategy));
            Assert.Null(nestedProperty.FindAnnotation(SqlServerFullAnnotationNames.Instance.ValueGenerationStrategy));

            property.SqlServer().ValueGenerationStrategy = SqlServerValueGenerationStrategy.SequenceHiLo;
            nestedProperty.SetAnnotation(SqlServerFullAnnotationNames.Instance.ValueGenerationStrategy, SqlServerValueGenerationStrategy.SequenceHiLo, ConfigurationSource.Convention);

            Assert.Equal(SqlServerValueGenerationStrategy.SequenceHiLo, property.SqlServer().ValueGenerationStrategy);
            Assert.Equal(SqlServerValueGenerationStrategy.SequenceHiLo, nestedProperty.SqlServer().ValueGenerationStrategy);

            Assert.Equal(ConfigurationSource.Explicit, property.FindAnnotation(SqlServerFullAnnotationNames.Instance.ValueGenerationStrategy).GetConfigurationSource());
            Assert.Equal(ConfigurationSource.Convention, nestedProperty.FindAnnotation(SqlServerFullAnnotationNames.Instance.ValueGenerationStrategy).GetConfigurationSource());
        }

        [Fact]
        public void Can_set_sequence_name_for_complex_property()
        {
            ComplexPropertyDefinition propDef11, propDef12, propDef21, propDef22;

            var complexUsage = BuildUsage(out propDef11, out propDef12, out propDef21, out propDef22);

            var property = complexUsage.FindProperty("IntProp");
            var nestedProperty = complexUsage.FindComplexTypeUsage("Nested").FindProperty("IntProp");

            Assert.Null(property.SqlServer().HiLoSequenceName);
            Assert.Null(nestedProperty.SqlServer().HiLoSequenceName);
            Assert.Null(propDef11.SqlServer().HiLoSequenceNameDefault);
            Assert.Null(propDef21.SqlServer().HiLoSequenceNameDefault);

            propDef11.SqlServer().HiLoSequenceNameDefault = "Seq1";
            propDef21.SetAnnotation(SqlServerFullAnnotationNames.Instance.HiLoSequenceName, "Seq2", ConfigurationSource.DataAnnotation);

            Assert.Equal("Seq1", propDef11.SqlServer().HiLoSequenceNameDefault);
            Assert.Equal("Seq2", propDef21.SqlServer().HiLoSequenceNameDefault);

            Assert.Equal(ConfigurationSource.Explicit, propDef11.FindAnnotation(SqlServerFullAnnotationNames.Instance.HiLoSequenceName).GetConfigurationSource());
            Assert.Equal(ConfigurationSource.DataAnnotation, propDef21.FindAnnotation(SqlServerFullAnnotationNames.Instance.HiLoSequenceName).GetConfigurationSource());

            Assert.Equal("Seq1", property.SqlServer().HiLoSequenceName);
            Assert.Equal("Seq2", nestedProperty.SqlServer().HiLoSequenceName);

            Assert.Null(property.FindAnnotation(SqlServerFullAnnotationNames.Instance.HiLoSequenceName));
            Assert.Null(nestedProperty.FindAnnotation(SqlServerFullAnnotationNames.Instance.HiLoSequenceName));

            property.SqlServer().HiLoSequenceName = "Seq3";
            nestedProperty.SetAnnotation(SqlServerFullAnnotationNames.Instance.HiLoSequenceName, "Seq4", ConfigurationSource.Convention);

            Assert.Equal("Seq3", property.SqlServer().HiLoSequenceName);
            Assert.Equal("Seq4", nestedProperty.SqlServer().HiLoSequenceName);

            Assert.Equal(ConfigurationSource.Explicit, property.FindAnnotation(SqlServerFullAnnotationNames.Instance.HiLoSequenceName).GetConfigurationSource());
            Assert.Equal(ConfigurationSource.Convention, nestedProperty.FindAnnotation(SqlServerFullAnnotationNames.Instance.HiLoSequenceName).GetConfigurationSource());
        }

        [Fact]
        public void Can_set_sequence_schema_for_complex_property()
        {
            ComplexPropertyDefinition propDef11, propDef12, propDef21, propDef22;

            var complexUsage = BuildUsage(out propDef11, out propDef12, out propDef21, out propDef22);

            var property = complexUsage.FindProperty("IntProp");
            var nestedProperty = complexUsage.FindComplexTypeUsage("Nested").FindProperty("IntProp");

            Assert.Null(property.SqlServer().HiLoSequenceSchema);
            Assert.Null(nestedProperty.SqlServer().HiLoSequenceSchema);
            Assert.Null(propDef11.SqlServer().HiLoSequenceSchemaDefault);
            Assert.Null(propDef21.SqlServer().HiLoSequenceSchemaDefault);

            propDef11.SqlServer().HiLoSequenceSchemaDefault = "Seq1";
            propDef21.SetAnnotation(SqlServerFullAnnotationNames.Instance.HiLoSequenceSchema, "Seq2", ConfigurationSource.DataAnnotation);

            Assert.Equal("Seq1", propDef11.SqlServer().HiLoSequenceSchemaDefault);
            Assert.Equal("Seq2", propDef21.SqlServer().HiLoSequenceSchemaDefault);

            Assert.Equal(ConfigurationSource.Explicit, propDef11.FindAnnotation(SqlServerFullAnnotationNames.Instance.HiLoSequenceSchema).GetConfigurationSource());
            Assert.Equal(ConfigurationSource.DataAnnotation, propDef21.FindAnnotation(SqlServerFullAnnotationNames.Instance.HiLoSequenceSchema).GetConfigurationSource());

            Assert.Equal("Seq1", property.SqlServer().HiLoSequenceSchema);
            Assert.Equal("Seq2", nestedProperty.SqlServer().HiLoSequenceSchema);

            Assert.Null(property.FindAnnotation(SqlServerFullAnnotationNames.Instance.HiLoSequenceSchema));
            Assert.Null(nestedProperty.FindAnnotation(SqlServerFullAnnotationNames.Instance.HiLoSequenceSchema));

            property.SqlServer().HiLoSequenceSchema = "Seq3";
            nestedProperty.SetAnnotation(SqlServerFullAnnotationNames.Instance.HiLoSequenceSchema, "Seq4", ConfigurationSource.Convention);

            Assert.Equal("Seq3", property.SqlServer().HiLoSequenceSchema);
            Assert.Equal("Seq4", nestedProperty.SqlServer().HiLoSequenceSchema);

            Assert.Equal(ConfigurationSource.Explicit, property.FindAnnotation(SqlServerFullAnnotationNames.Instance.HiLoSequenceSchema).GetConfigurationSource());
            Assert.Equal(ConfigurationSource.Convention, nestedProperty.FindAnnotation(SqlServerFullAnnotationNames.Instance.HiLoSequenceSchema).GetConfigurationSource());
        }

        private static ComplexTypeUsage BuildUsage(
            out ComplexPropertyDefinition propDef11,
            out ComplexPropertyDefinition propDef12,
            out ComplexPropertyDefinition propDef21,
            out ComplexPropertyDefinition propDef22)
        {
            var model = new Model();

            var complexDef1 = model.AddComplexTypeDefinition(typeof(Complex1));
            propDef11 = complexDef1.AddPropertyDefinition("IntProp");
            propDef12 = complexDef1.AddPropertyDefinition("StringProp");

            var complexDef2 = model.AddComplexTypeDefinition(typeof(Complex2));
            propDef21 = complexDef2.AddPropertyDefinition("IntProp");
            propDef22 = complexDef2.AddPropertyDefinition("StringProp");

            var entityType = model.AddEntityType(typeof(Entity1));

            var usage1 = entityType.AddComplexTypeUsage("Usage1", complexDef1);

            var nested1 = usage1.AddComplexTypeUsage(complexDef1.AddComplexTypeReferenceDefinition("Nested", complexDef2));

            usage1.AddProperty(propDef11);
            usage1.AddProperty(propDef12);

            nested1.AddProperty(propDef21);
            nested1.AddProperty(propDef22);

            return usage1;
        }

        private class Entity1
        {
            public int Id { get; set; }
            public Complex1 Usage { get; set; }
        }

        private class Complex1
        {
            public int IntProp { get; set; }
            public string StringProp { get; set; }

            public Complex2 Nested { get; set; }
        }

        private class Complex2
        {
            public int IntProp { get; set; }
            public string StringProp { get; set; }
        }
    }
}
