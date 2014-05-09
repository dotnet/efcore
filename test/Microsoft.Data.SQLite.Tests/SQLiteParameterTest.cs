// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Data;
using Xunit;

namespace Microsoft.Data.SQLite
{
    public class SQLiteParameterTest
    {
        [Fact]
        public void Ctor_validates_arguments()
        {
            var ex = Assert.Throws<ArgumentException>(() => new SQLiteParameter(null, 1));
            Assert.Equal(Strings.FormatArgumentIsNullOrWhitespace("parameterName"), ex.Message);

            ex = Assert.Throws<ArgumentNullException>(() => new SQLiteParameter("@Parameter", null));
            Assert.Equal("value", ex.ParamName);
        }

        [Fact]
        public void Ctor_sets_name_and_value()
        {
            var result = new SQLiteParameter("@Parameter", 1);

            Assert.Equal("@Parameter", result.ParameterName);
            Assert.Equal(1, result.Value);
        }

        [Fact]
        public void Direction_input_by_default()
        {
            Assert.Equal(ParameterDirection.Input, new SQLiteParameter().Direction);
        }

        [Fact]
        public void Direction_validates_value()
        {
            var ex = Assert.Throws<ArgumentException>(() => new SQLiteParameter().Direction = 0);
            Assert.Equal(Strings.FormatInvalidParameterDirection(0), ex.Message);
        }

        [Fact]
        public void ParameterName_validates_value()
        {
            var ex = Assert.Throws<ArgumentException>(() => new SQLiteParameter().ParameterName = null);
            Assert.Equal(Strings.FormatArgumentIsNullOrWhitespace("value"), ex.Message);
        }

        [Fact]
        public void ParameterName_unsets_bound_when_changed()
        {
            using (var connection = new SQLiteConnection("Filename=:memory:"))
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "SELECT @Parameter";
                var parameter = command.Parameters.AddWithValue("@Parameter", 1);
                connection.Open();
                command.ExecuteNonQuery();

                parameter.ParameterName = "Renamed";

                Assert.False(parameter.Bound);
            }
        }

        [Fact]
        public void ParameterName_doesnt_unset_bound_when_unchanged()
        {
            using (var connection = new SQLiteConnection("Filename=:memory:"))
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "SELECT @Parameter";
                var parameter = command.Parameters.AddWithValue("@Parameter", 1);
                connection.Open();
                command.ExecuteNonQuery();

                parameter.ParameterName = "@Parameter";

                Assert.True(parameter.Bound);
            }
        }

        [Fact]
        public void Value_validates_value()
        {
            var ex = Assert.Throws<ArgumentNullException>(() => new SQLiteParameter().Value = null);
            Assert.Equal("value", ex.ParamName);
        }

        [Fact]
        public void Value_unsets_bound_when_changed()
        {
            using (var connection = new SQLiteConnection("Filename=:memory:"))
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "SELECT @Parameter";
                var parameter = command.Parameters.AddWithValue("@Parameter", 1);
                connection.Open();
                command.ExecuteNonQuery();

                parameter.Value = 2;

                Assert.False(parameter.Bound);
            }
        }

        [Fact]
        public void Value_doesnt_unset_bound_when_unchanged()
        {
            using (var connection = new SQLiteConnection("Filename=:memory:"))
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "SELECT @Parameter";
                var parameter = command.Parameters.AddWithValue("@Parameter", 1);
                connection.Open();
                command.ExecuteNonQuery();

                parameter.Value = 1;

                Assert.True(parameter.Bound);
            }
        }

        [Fact]
        public void ResetDbType_not_supported()
        {
            Assert.Throws<NotSupportedException>(() => new SQLiteParameter().ResetDbType());
        }

        [Fact]
        public void Bind_requres_set_name()
        {
            using (var connection = new SQLiteConnection("Filename=:memory:"))
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "SELECT @Parameter";
                command.Parameters.Add(new SQLiteParameter { Value = 1 });
                connection.Open();

                var ex = Assert.Throws<InvalidOperationException>(() => command.ExecuteNonQuery());

                Assert.Equal(Strings.FormatRequiresSet("ParameterName"), ex.Message);
            }
        }

        [Fact]
        public void Bind_requres_set_value()
        {
            using (var connection = new SQLiteConnection("Filename=:memory:"))
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "SELECT @Parameter";
                command.Parameters.Add(new SQLiteParameter { ParameterName = "@Parameter" });
                connection.Open();

                var ex = Assert.Throws<InvalidOperationException>(() => command.ExecuteNonQuery());

                Assert.Equal(Strings.FormatRequiresSet("Value"), ex.Message);
            }
        }

        [Fact]
        public void Bind_is_noop_on_unknown_parameter()
        {
            using (var connection = new SQLiteConnection("Filename=:memory:"))
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "SELECT 1";
                command.Parameters.AddWithValue("@Unknown", 1);
                connection.Open();

                command.ExecuteNonQuery();
            }
        }

        [Fact]
        public void Bind_binds_integer_values()
        {
            using (var connection = new SQLiteConnection("Filename=:memory:"))
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "SELECT @Integer";
                command.Parameters.AddWithValue("@Integer", 1L);
                connection.Open();

                var result = command.ExecuteScalar();

                Assert.Equal(1L, result);
            }
        }

        [Fact]
        public void Bind_binds_float_values()
        {
            using (var connection = new SQLiteConnection("Filename=:memory:"))
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "SELECT @Float";
                command.Parameters.AddWithValue("@Float", 3.14);
                connection.Open();

                var result = command.ExecuteScalar();

                Assert.Equal(3.14, result);
            }
        }

        [Fact]
        public void Bind_binds_text_values()
        {
            using (var connection = new SQLiteConnection("Filename=:memory:"))
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "SELECT @Text";
                command.Parameters.AddWithValue("@Text", "test");
                connection.Open();

                var result = command.ExecuteScalar();

                Assert.Equal("test", result);
            }
        }

        [Fact]
        public void Bind_binds_blob_values()
        {
            using (var connection = new SQLiteConnection("Filename=:memory:"))
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "SELECT @Blob";
                command.Parameters.AddWithValue("@Blob", new byte[] { 0x7e, 0x57 });
                connection.Open();

                var result = command.ExecuteScalar();

                Assert.Equal(new byte[] { 0x7e, 0x57 }, result);
            }
        }

        [Fact]
        public void Bind_binds_null_value()
        {
            using (var connection = new SQLiteConnection("Filename=:memory:"))
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "SELECT @Null";
                command.Parameters.AddWithValue("@Null", DBNull.Value);
                connection.Open();

                var result = command.ExecuteScalar();

                Assert.Equal(DBNull.Value, result);
            }
        }

        [Fact]
        public void Bind_sets_bound()
        {
            using (var connection = new SQLiteConnection("Filename=:memory:"))
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "SELECT @Parameter";
                var parameter = command.Parameters.AddWithValue("@Parameter", 1);
                connection.Open();

                command.ExecuteNonQuery();

                Assert.True(parameter.Bound);
            }
        }
    }
}
