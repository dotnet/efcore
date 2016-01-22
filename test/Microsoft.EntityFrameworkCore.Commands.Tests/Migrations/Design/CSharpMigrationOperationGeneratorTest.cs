using System;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Migrations.Design
{
    public class CSharpMigrationOperationGeneratorTest
    {
        private static string EOL = Environment.NewLine;

        [Fact]
        public void Generate_seperates_operations_by_a_blank_line()
        {
            var generator = new CSharpMigrationOperationGenerator(new CSharpHelper());
            var builder = new IndentedStringBuilder();

            generator.Generate(
                "mb",
                new[]
                {
                    new SqlOperation { Sql = "-- Don't stand so" },
                    new SqlOperation { Sql = "-- close to me" }
                },
                builder);

            Assert.Equal(
                "mb.Sql(\"-- Don't stand so\");" + EOL +
                EOL +
                "mb.Sql(\"-- close to me\");",
                builder.ToString());
        }
    }
}
