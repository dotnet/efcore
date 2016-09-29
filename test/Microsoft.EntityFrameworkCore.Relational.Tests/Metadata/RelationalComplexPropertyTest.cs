// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Relational.Tests.Metadata
{
    public class RelationalComplexPropertyTest
    {
        [Fact]
        public void Can_set_column_name_for_complex_property()
        {
            ComplexPropertyDefinition propDef11, propDef12, propDef21, propDef22;

            var complexUsage = BuildUsage(out propDef11, out propDef12, out propDef21, out propDef22);

            var property = complexUsage.FindProperty("StringProp");
            var nestedProperty = complexUsage.FindComplexTypeUsage("Nested").FindProperty("StringProp");

            Assert.Equal("StringProp", property.Relational().ColumnName);
            Assert.Equal("StringProp", nestedProperty.Relational().ColumnName);
            Assert.Null(propDef12.Relational().ColumnNameDefault);
            Assert.Null(propDef22.Relational().ColumnNameDefault);

            propDef12.Relational().ColumnNameDefault = "Col1";
            propDef22.SetAnnotation(RelationalFullAnnotationNames.Instance.ColumnName, "Col2", ConfigurationSource.DataAnnotation);

            Assert.Equal("Col1", propDef12.Relational().ColumnNameDefault);
            Assert.Equal("Col2", propDef22.Relational().ColumnNameDefault);

            Assert.Equal(ConfigurationSource.Explicit, propDef12.FindAnnotation(RelationalFullAnnotationNames.Instance.ColumnName).GetConfigurationSource());
            Assert.Equal(ConfigurationSource.DataAnnotation, propDef22.FindAnnotation(RelationalFullAnnotationNames.Instance.ColumnName).GetConfigurationSource());

            Assert.Equal("Col1", property.Relational().ColumnName);
            Assert.Equal("Col2", nestedProperty.Relational().ColumnName);

            Assert.Null(property.FindAnnotation(RelationalFullAnnotationNames.Instance.ColumnName));
            Assert.Null(nestedProperty.FindAnnotation(RelationalFullAnnotationNames.Instance.ColumnName));

            property.Relational().ColumnName = "Col3";
            nestedProperty.SetAnnotation(RelationalFullAnnotationNames.Instance.ColumnName, "Col4", ConfigurationSource.Convention);

            Assert.Equal("Col3", property.Relational().ColumnName);
            Assert.Equal("Col4", nestedProperty.Relational().ColumnName);

            Assert.Equal(ConfigurationSource.Explicit, property.FindAnnotation(RelationalFullAnnotationNames.Instance.ColumnName).GetConfigurationSource());
            Assert.Equal(ConfigurationSource.Convention, nestedProperty.FindAnnotation(RelationalFullAnnotationNames.Instance.ColumnName).GetConfigurationSource());
        }

        [Fact]
        public void Can_set_column_type_for_complex_property()
        {
            ComplexPropertyDefinition propDef11, propDef12, propDef21, propDef22;

            var complexUsage = BuildUsage(out propDef11, out propDef12, out propDef21, out propDef22);

            var property = complexUsage.FindProperty("StringProp");
            var nestedProperty = complexUsage.FindComplexTypeUsage("Nested").FindProperty("StringProp");

            Assert.Null(property.Relational().ColumnType);
            Assert.Null(nestedProperty.Relational().ColumnType);
            Assert.Null(propDef12.Relational().ColumnTypeDefault);
            Assert.Null(propDef22.Relational().ColumnTypeDefault);

            propDef12.Relational().ColumnTypeDefault = "Col1";
            propDef22.SetAnnotation(RelationalFullAnnotationNames.Instance.ColumnType, "Col2", ConfigurationSource.DataAnnotation);

            Assert.Equal("Col1", propDef12.Relational().ColumnTypeDefault);
            Assert.Equal("Col2", propDef22.Relational().ColumnTypeDefault);

            Assert.Equal(ConfigurationSource.Explicit, propDef12.FindAnnotation(RelationalFullAnnotationNames.Instance.ColumnType).GetConfigurationSource());
            Assert.Equal(ConfigurationSource.DataAnnotation, propDef22.FindAnnotation(RelationalFullAnnotationNames.Instance.ColumnType).GetConfigurationSource());

            Assert.Equal("Col1", property.Relational().ColumnType);
            Assert.Equal("Col2", nestedProperty.Relational().ColumnType);

            Assert.Null(property.FindAnnotation(RelationalFullAnnotationNames.Instance.ColumnType));
            Assert.Null(nestedProperty.FindAnnotation(RelationalFullAnnotationNames.Instance.ColumnType));

            property.Relational().ColumnType = "Col3";
            nestedProperty.SetAnnotation(RelationalFullAnnotationNames.Instance.ColumnType, "Col4", ConfigurationSource.Convention);

            Assert.Equal("Col3", property.Relational().ColumnType);
            Assert.Equal("Col4", nestedProperty.Relational().ColumnType);

            Assert.Equal(ConfigurationSource.Explicit, property.FindAnnotation(RelationalFullAnnotationNames.Instance.ColumnType).GetConfigurationSource());
            Assert.Equal(ConfigurationSource.Convention, nestedProperty.FindAnnotation(RelationalFullAnnotationNames.Instance.ColumnType).GetConfigurationSource());
        }

        [Fact]
        public void Can_set_default_value_SQL_for_complex_property()
        {
            ComplexPropertyDefinition propDef11, propDef12, propDef21, propDef22;

            var complexUsage = BuildUsage(out propDef11, out propDef12, out propDef21, out propDef22);

            var property = complexUsage.FindProperty("StringProp");
            var nestedProperty = complexUsage.FindComplexTypeUsage("Nested").FindProperty("StringProp");

            Assert.Null(property.Relational().DefaultValueSql);
            Assert.Null(nestedProperty.Relational().DefaultValueSql);
            Assert.Null(propDef12.Relational().DefaultValueSqlDefault);
            Assert.Null(propDef22.Relational().DefaultValueSqlDefault);

            propDef12.Relational().DefaultValueSqlDefault = "Col1";
            propDef22.SetAnnotation(RelationalFullAnnotationNames.Instance.DefaultValueSql, "Col2", ConfigurationSource.DataAnnotation);

            Assert.Equal("Col1", propDef12.Relational().DefaultValueSqlDefault);
            Assert.Equal("Col2", propDef22.Relational().DefaultValueSqlDefault);

            Assert.Equal(ConfigurationSource.Explicit, propDef12.FindAnnotation(RelationalFullAnnotationNames.Instance.DefaultValueSql).GetConfigurationSource());
            Assert.Equal(ConfigurationSource.DataAnnotation, propDef22.FindAnnotation(RelationalFullAnnotationNames.Instance.DefaultValueSql).GetConfigurationSource());

            Assert.Equal("Col1", property.Relational().DefaultValueSql);
            Assert.Equal("Col2", nestedProperty.Relational().DefaultValueSql);

            Assert.Null(property.FindAnnotation(RelationalFullAnnotationNames.Instance.DefaultValueSql));
            Assert.Null(nestedProperty.FindAnnotation(RelationalFullAnnotationNames.Instance.DefaultValueSql));

            property.Relational().DefaultValueSql = "Col3";
            nestedProperty.SetAnnotation(RelationalFullAnnotationNames.Instance.DefaultValueSql, "Col4", ConfigurationSource.Convention);

            Assert.Equal("Col3", property.Relational().DefaultValueSql);
            Assert.Equal("Col4", nestedProperty.Relational().DefaultValueSql);

            Assert.Equal(ConfigurationSource.Explicit, property.FindAnnotation(RelationalFullAnnotationNames.Instance.DefaultValueSql).GetConfigurationSource());
            Assert.Equal(ConfigurationSource.Convention, nestedProperty.FindAnnotation(RelationalFullAnnotationNames.Instance.DefaultValueSql).GetConfigurationSource());
        }

        [Fact]
        public void Can_set_computed_column_SQL_for_complex_property()
        {
            ComplexPropertyDefinition propDef11, propDef12, propDef21, propDef22;

            var complexUsage = BuildUsage(out propDef11, out propDef12, out propDef21, out propDef22);

            var property = complexUsage.FindProperty("StringProp");
            var nestedProperty = complexUsage.FindComplexTypeUsage("Nested").FindProperty("StringProp");

            Assert.Null(property.Relational().ComputedColumnSql);
            Assert.Null(nestedProperty.Relational().ComputedColumnSql);
            Assert.Null(propDef12.Relational().ComputedColumnSqlDefault);
            Assert.Null(propDef22.Relational().ComputedColumnSqlDefault);

            propDef12.Relational().ComputedColumnSqlDefault = "Col1";
            propDef22.SetAnnotation(RelationalFullAnnotationNames.Instance.ComputedColumnSql, "Col2", ConfigurationSource.DataAnnotation);

            Assert.Equal("Col1", propDef12.Relational().ComputedColumnSqlDefault);
            Assert.Equal("Col2", propDef22.Relational().ComputedColumnSqlDefault);

            Assert.Equal(ConfigurationSource.Explicit, propDef12.FindAnnotation(RelationalFullAnnotationNames.Instance.ComputedColumnSql).GetConfigurationSource());
            Assert.Equal(ConfigurationSource.DataAnnotation, propDef22.FindAnnotation(RelationalFullAnnotationNames.Instance.ComputedColumnSql).GetConfigurationSource());

            Assert.Equal("Col1", property.Relational().ComputedColumnSql);
            Assert.Equal("Col2", nestedProperty.Relational().ComputedColumnSql);

            Assert.Null(property.FindAnnotation(RelationalFullAnnotationNames.Instance.ComputedColumnSql));
            Assert.Null(nestedProperty.FindAnnotation(RelationalFullAnnotationNames.Instance.ComputedColumnSql));

            property.Relational().ComputedColumnSql = "Col3";
            nestedProperty.SetAnnotation(RelationalFullAnnotationNames.Instance.ComputedColumnSql, "Col4", ConfigurationSource.Convention);

            Assert.Equal("Col3", property.Relational().ComputedColumnSql);
            Assert.Equal("Col4", nestedProperty.Relational().ComputedColumnSql);

            Assert.Equal(ConfigurationSource.Explicit, property.FindAnnotation(RelationalFullAnnotationNames.Instance.ComputedColumnSql).GetConfigurationSource());
            Assert.Equal(ConfigurationSource.Convention, nestedProperty.FindAnnotation(RelationalFullAnnotationNames.Instance.ComputedColumnSql).GetConfigurationSource());
        }

        [Fact]
        public void Can_set_default_value_for_complex_property()
        {
            ComplexPropertyDefinition propDef11, propDef12, propDef21, propDef22;

            var complexUsage = BuildUsage(out propDef11, out propDef12, out propDef21, out propDef22);

            var property = complexUsage.FindProperty("StringProp");
            var nestedProperty = complexUsage.FindComplexTypeUsage("Nested").FindProperty("StringProp");

            Assert.Null(property.Relational().DefaultValue);
            Assert.Null(nestedProperty.Relational().DefaultValue);
            Assert.Null(propDef12.Relational().DefaultValueDefault);
            Assert.Null(propDef22.Relational().DefaultValueDefault);

            propDef12.Relational().DefaultValueDefault = "Col1";
            propDef22.SetAnnotation(RelationalFullAnnotationNames.Instance.DefaultValue, "Col2", ConfigurationSource.DataAnnotation);

            Assert.Equal("Col1", propDef12.Relational().DefaultValueDefault);
            Assert.Equal("Col2", propDef22.Relational().DefaultValueDefault);

            Assert.Equal(ConfigurationSource.Explicit, propDef12.FindAnnotation(RelationalFullAnnotationNames.Instance.DefaultValue).GetConfigurationSource());
            Assert.Equal(ConfigurationSource.DataAnnotation, propDef22.FindAnnotation(RelationalFullAnnotationNames.Instance.DefaultValue).GetConfigurationSource());

            Assert.Equal("Col1", property.Relational().DefaultValue);
            Assert.Equal("Col2", nestedProperty.Relational().DefaultValue);

            Assert.Null(property.FindAnnotation(RelationalFullAnnotationNames.Instance.DefaultValue));
            Assert.Null(nestedProperty.FindAnnotation(RelationalFullAnnotationNames.Instance.DefaultValue));

            property.Relational().DefaultValue = "Col3";
            nestedProperty.SetAnnotation(RelationalFullAnnotationNames.Instance.DefaultValue, "Col4", ConfigurationSource.Convention);

            Assert.Equal("Col3", property.Relational().DefaultValue);
            Assert.Equal("Col4", nestedProperty.Relational().DefaultValue);

            Assert.Equal(ConfigurationSource.Explicit, property.FindAnnotation(RelationalFullAnnotationNames.Instance.DefaultValue).GetConfigurationSource());
            Assert.Equal(ConfigurationSource.Convention, nestedProperty.FindAnnotation(RelationalFullAnnotationNames.Instance.DefaultValue).GetConfigurationSource());
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
