// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Sqlite.Tests.Metadata
{
    public class SqliteComplexPropertyTest
    {
        [Fact]
        public void Can_set_column_name_for_complex_property()
        {
            ComplexPropertyDefinition propDef11, propDef12, propDef21, propDef22;

            var complexUsage = BuildUsage(out propDef11, out propDef12, out propDef21, out propDef22);

            var property = complexUsage.FindProperty("StringProp");
            var nestedProperty = complexUsage.FindComplexTypeUsage("Nested").FindProperty("StringProp");

            Assert.Equal("StringProp", property.Sqlite().ColumnName);
            Assert.Equal("StringProp", nestedProperty.Sqlite().ColumnName);
            Assert.Null(propDef12.Sqlite().ColumnNameDefault);
            Assert.Null(propDef22.Sqlite().ColumnNameDefault);

            propDef12.Sqlite().ColumnNameDefault = "Col1";
            propDef22.SetAnnotation(SqliteFullAnnotationNames.Instance.ColumnName, "Col2", ConfigurationSource.DataAnnotation);

            Assert.Equal("Col1", propDef12.Sqlite().ColumnNameDefault);
            Assert.Equal("Col2", propDef22.Sqlite().ColumnNameDefault);

            Assert.Equal(ConfigurationSource.Explicit, propDef12.FindAnnotation(SqliteFullAnnotationNames.Instance.ColumnName).GetConfigurationSource());
            Assert.Equal(ConfigurationSource.DataAnnotation, propDef22.FindAnnotation(SqliteFullAnnotationNames.Instance.ColumnName).GetConfigurationSource());

            Assert.Equal("Col1", property.Sqlite().ColumnName);
            Assert.Equal("Col2", nestedProperty.Sqlite().ColumnName);

            Assert.Null(property.FindAnnotation(SqliteFullAnnotationNames.Instance.ColumnName));
            Assert.Null(nestedProperty.FindAnnotation(SqliteFullAnnotationNames.Instance.ColumnName));

            property.Relational().ColumnName = "ColR1";
            nestedProperty.SetAnnotation(RelationalFullAnnotationNames.Instance.ColumnName, "ColR2", ConfigurationSource.Convention);

            Assert.Equal("ColR1", property.Sqlite().ColumnName);
            Assert.Equal("ColR2", nestedProperty.Sqlite().ColumnName);

            property.Sqlite().ColumnName = "Col3";
            nestedProperty.SetAnnotation(SqliteFullAnnotationNames.Instance.ColumnName, "Col4", ConfigurationSource.Convention);

            Assert.Equal("Col3", property.Sqlite().ColumnName);
            Assert.Equal("Col4", nestedProperty.Sqlite().ColumnName);

            Assert.Equal(ConfigurationSource.Explicit, property.FindAnnotation(SqliteFullAnnotationNames.Instance.ColumnName).GetConfigurationSource());
            Assert.Equal(ConfigurationSource.Convention, nestedProperty.FindAnnotation(SqliteFullAnnotationNames.Instance.ColumnName).GetConfigurationSource());
        }

        [Fact]
        public void Can_set_column_type_for_complex_property()
        {
            ComplexPropertyDefinition propDef11, propDef12, propDef21, propDef22;

            var complexUsage = BuildUsage(out propDef11, out propDef12, out propDef21, out propDef22);

            var property = complexUsage.FindProperty("StringProp");
            var nestedProperty = complexUsage.FindComplexTypeUsage("Nested").FindProperty("StringProp");

            Assert.Null(property.Sqlite().ColumnType);
            Assert.Null(nestedProperty.Sqlite().ColumnType);
            Assert.Null(propDef12.Sqlite().ColumnTypeDefault);
            Assert.Null(propDef22.Sqlite().ColumnTypeDefault);

            propDef12.Sqlite().ColumnTypeDefault = "Col1";
            propDef22.SetAnnotation(SqliteFullAnnotationNames.Instance.ColumnType, "Col2", ConfigurationSource.DataAnnotation);

            Assert.Equal("Col1", propDef12.Sqlite().ColumnTypeDefault);
            Assert.Equal("Col2", propDef22.Sqlite().ColumnTypeDefault);

            Assert.Equal(ConfigurationSource.Explicit, propDef12.FindAnnotation(SqliteFullAnnotationNames.Instance.ColumnType).GetConfigurationSource());
            Assert.Equal(ConfigurationSource.DataAnnotation, propDef22.FindAnnotation(SqliteFullAnnotationNames.Instance.ColumnType).GetConfigurationSource());

            Assert.Equal("Col1", property.Sqlite().ColumnType);
            Assert.Equal("Col2", nestedProperty.Sqlite().ColumnType);

            Assert.Null(property.FindAnnotation(SqliteFullAnnotationNames.Instance.ColumnType));
            Assert.Null(nestedProperty.FindAnnotation(SqliteFullAnnotationNames.Instance.ColumnType));

            property.Relational().ColumnType = "ColR1";
            nestedProperty.SetAnnotation(RelationalFullAnnotationNames.Instance.ColumnType, "ColR2", ConfigurationSource.Convention);

            Assert.Equal("ColR1", property.Sqlite().ColumnType);
            Assert.Equal("ColR2", nestedProperty.Sqlite().ColumnType);

            property.Sqlite().ColumnType = "Col3";
            nestedProperty.SetAnnotation(SqliteFullAnnotationNames.Instance.ColumnType, "Col4", ConfigurationSource.Convention);

            Assert.Equal("Col3", property.Sqlite().ColumnType);
            Assert.Equal("Col4", nestedProperty.Sqlite().ColumnType);

            Assert.Equal(ConfigurationSource.Explicit, property.FindAnnotation(SqliteFullAnnotationNames.Instance.ColumnType).GetConfigurationSource());
            Assert.Equal(ConfigurationSource.Convention, nestedProperty.FindAnnotation(SqliteFullAnnotationNames.Instance.ColumnType).GetConfigurationSource());
        }

        [Fact]
        public void Can_set_default_value_SQL_for_complex_property()
        {
            ComplexPropertyDefinition propDef11, propDef12, propDef21, propDef22;

            var complexUsage = BuildUsage(out propDef11, out propDef12, out propDef21, out propDef22);

            var property = complexUsage.FindProperty("StringProp");
            var nestedProperty = complexUsage.FindComplexTypeUsage("Nested").FindProperty("StringProp");

            Assert.Null(property.Sqlite().DefaultValueSql);
            Assert.Null(nestedProperty.Sqlite().DefaultValueSql);
            Assert.Null(propDef12.Sqlite().DefaultValueSqlDefault);
            Assert.Null(propDef22.Sqlite().DefaultValueSqlDefault);

            propDef12.Sqlite().DefaultValueSqlDefault = "Col1";
            propDef22.SetAnnotation(SqliteFullAnnotationNames.Instance.DefaultValueSql, "Col2", ConfigurationSource.DataAnnotation);

            Assert.Equal("Col1", propDef12.Sqlite().DefaultValueSqlDefault);
            Assert.Equal("Col2", propDef22.Sqlite().DefaultValueSqlDefault);

            Assert.Equal(ConfigurationSource.Explicit, propDef12.FindAnnotation(SqliteFullAnnotationNames.Instance.DefaultValueSql).GetConfigurationSource());
            Assert.Equal(ConfigurationSource.DataAnnotation, propDef22.FindAnnotation(SqliteFullAnnotationNames.Instance.DefaultValueSql).GetConfigurationSource());

            Assert.Equal("Col1", property.Sqlite().DefaultValueSql);
            Assert.Equal("Col2", nestedProperty.Sqlite().DefaultValueSql);

            Assert.Null(property.FindAnnotation(SqliteFullAnnotationNames.Instance.DefaultValueSql));
            Assert.Null(nestedProperty.FindAnnotation(SqliteFullAnnotationNames.Instance.DefaultValueSql));

            property.Relational().DefaultValueSql = "ColR1";
            nestedProperty.SetAnnotation(RelationalFullAnnotationNames.Instance.DefaultValueSql, "ColR2", ConfigurationSource.Convention);

            Assert.Equal("ColR1", property.Sqlite().DefaultValueSql);
            Assert.Equal("ColR2", nestedProperty.Sqlite().DefaultValueSql);

            property.Sqlite().DefaultValueSql = "Col3";
            nestedProperty.SetAnnotation(SqliteFullAnnotationNames.Instance.DefaultValueSql, "Col4", ConfigurationSource.Convention);

            Assert.Equal("Col3", property.Sqlite().DefaultValueSql);
            Assert.Equal("Col4", nestedProperty.Sqlite().DefaultValueSql);

            Assert.Equal(ConfigurationSource.Explicit, property.FindAnnotation(SqliteFullAnnotationNames.Instance.DefaultValueSql).GetConfigurationSource());
            Assert.Equal(ConfigurationSource.Convention, nestedProperty.FindAnnotation(SqliteFullAnnotationNames.Instance.DefaultValueSql).GetConfigurationSource());
        }

        [Fact]
        public void Can_set_computed_column_SQL_for_complex_property()
        {
            ComplexPropertyDefinition propDef11, propDef12, propDef21, propDef22;

            var complexUsage = BuildUsage(out propDef11, out propDef12, out propDef21, out propDef22);

            var property = complexUsage.FindProperty("StringProp");
            var nestedProperty = complexUsage.FindComplexTypeUsage("Nested").FindProperty("StringProp");

            Assert.Null(property.Sqlite().ComputedColumnSql);
            Assert.Null(nestedProperty.Sqlite().ComputedColumnSql);
            Assert.Null(propDef12.Sqlite().ComputedColumnSqlDefault);
            Assert.Null(propDef22.Sqlite().ComputedColumnSqlDefault);

            propDef12.Sqlite().ComputedColumnSqlDefault = "Col1";
            propDef22.SetAnnotation(SqliteFullAnnotationNames.Instance.ComputedColumnSql, "Col2", ConfigurationSource.DataAnnotation);

            Assert.Equal("Col1", propDef12.Sqlite().ComputedColumnSqlDefault);
            Assert.Equal("Col2", propDef22.Sqlite().ComputedColumnSqlDefault);

            Assert.Equal(ConfigurationSource.Explicit, propDef12.FindAnnotation(SqliteFullAnnotationNames.Instance.ComputedColumnSql).GetConfigurationSource());
            Assert.Equal(ConfigurationSource.DataAnnotation, propDef22.FindAnnotation(SqliteFullAnnotationNames.Instance.ComputedColumnSql).GetConfigurationSource());

            Assert.Equal("Col1", property.Sqlite().ComputedColumnSql);
            Assert.Equal("Col2", nestedProperty.Sqlite().ComputedColumnSql);

            Assert.Null(property.FindAnnotation(SqliteFullAnnotationNames.Instance.ComputedColumnSql));
            Assert.Null(nestedProperty.FindAnnotation(SqliteFullAnnotationNames.Instance.ComputedColumnSql));

            property.Relational().ComputedColumnSql = "ColR1";
            nestedProperty.SetAnnotation(RelationalFullAnnotationNames.Instance.ComputedColumnSql, "ColR2", ConfigurationSource.Convention);

            Assert.Equal("ColR1", property.Sqlite().ComputedColumnSql);
            Assert.Equal("ColR2", nestedProperty.Sqlite().ComputedColumnSql);

            property.Sqlite().ComputedColumnSql = "Col3";
            nestedProperty.SetAnnotation(SqliteFullAnnotationNames.Instance.ComputedColumnSql, "Col4", ConfigurationSource.Convention);

            Assert.Equal("Col3", property.Sqlite().ComputedColumnSql);
            Assert.Equal("Col4", nestedProperty.Sqlite().ComputedColumnSql);

            Assert.Equal(ConfigurationSource.Explicit, property.FindAnnotation(SqliteFullAnnotationNames.Instance.ComputedColumnSql).GetConfigurationSource());
            Assert.Equal(ConfigurationSource.Convention, nestedProperty.FindAnnotation(SqliteFullAnnotationNames.Instance.ComputedColumnSql).GetConfigurationSource());
        }

        [Fact]
        public void Can_set_default_value_for_complex_property()
        {
            ComplexPropertyDefinition propDef11, propDef12, propDef21, propDef22;

            var complexUsage = BuildUsage(out propDef11, out propDef12, out propDef21, out propDef22);

            var property = complexUsage.FindProperty("StringProp");
            var nestedProperty = complexUsage.FindComplexTypeUsage("Nested").FindProperty("StringProp");

            Assert.Null(property.Sqlite().DefaultValue);
            Assert.Null(nestedProperty.Sqlite().DefaultValue);
            Assert.Null(propDef12.Sqlite().DefaultValueDefault);
            Assert.Null(propDef22.Sqlite().DefaultValueDefault);

            propDef12.Sqlite().DefaultValueDefault = "Col1";
            propDef22.SetAnnotation(SqliteFullAnnotationNames.Instance.DefaultValue, "Col2", ConfigurationSource.DataAnnotation);

            Assert.Equal("Col1", propDef12.Sqlite().DefaultValueDefault);
            Assert.Equal("Col2", propDef22.Sqlite().DefaultValueDefault);

            Assert.Equal(ConfigurationSource.Explicit, propDef12.FindAnnotation(SqliteFullAnnotationNames.Instance.DefaultValue).GetConfigurationSource());
            Assert.Equal(ConfigurationSource.DataAnnotation, propDef22.FindAnnotation(SqliteFullAnnotationNames.Instance.DefaultValue).GetConfigurationSource());

            Assert.Equal("Col1", property.Sqlite().DefaultValue);
            Assert.Equal("Col2", nestedProperty.Sqlite().DefaultValue);

            Assert.Null(property.FindAnnotation(SqliteFullAnnotationNames.Instance.DefaultValue));
            Assert.Null(nestedProperty.FindAnnotation(SqliteFullAnnotationNames.Instance.DefaultValue));

            property.Relational().DefaultValue = "ColR1";
            nestedProperty.SetAnnotation(RelationalFullAnnotationNames.Instance.DefaultValue, "ColR2", ConfigurationSource.Convention);

            Assert.Equal("ColR1", property.Sqlite().DefaultValue);
            Assert.Equal("ColR2", nestedProperty.Sqlite().DefaultValue);

            property.Sqlite().DefaultValue = "Col3";
            nestedProperty.SetAnnotation(SqliteFullAnnotationNames.Instance.DefaultValue, "Col4", ConfigurationSource.Convention);

            Assert.Equal("Col3", property.Sqlite().DefaultValue);
            Assert.Equal("Col4", nestedProperty.Sqlite().DefaultValue);

            Assert.Equal(ConfigurationSource.Explicit, property.FindAnnotation(SqliteFullAnnotationNames.Instance.DefaultValue).GetConfigurationSource());
            Assert.Equal(ConfigurationSource.Convention, nestedProperty.FindAnnotation(SqliteFullAnnotationNames.Instance.DefaultValue).GetConfigurationSource());
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
