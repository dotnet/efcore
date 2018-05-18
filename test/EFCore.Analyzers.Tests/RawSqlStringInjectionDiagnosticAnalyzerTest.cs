// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using System.Threading.Tasks;
using EFCore.Analyzers.Test.TestUtilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace Microsoft.EntityFrameworkCore
{
    public class RawSqlStringInjectionDiagnosticAnalyzerTest : DiagnosticAnalyzerTestBase
    {
        [Fact]
        public async Task No_warning_when_string_literal_passed_to_from_sql()
        {
            var diagnostics
                = await GetDiagnosticsAsync(
                    "RelationalQueryableExtensions.FromSql<object>(null, \"select * from Customers\");");

            Assert.Empty(diagnostics);
        }
        
        [Fact]
        public async Task Error_when_sql_expression_recursively_initialized()
        {
            var diagnostics
                = await GetDiagnosticsAsync(
                    @"string q = null;
                      q = M2(q);
                      string M2(string _) { return null; }
                      RelationalDatabaseFacadeExtensions.ExecuteSqlCommand(null, q);");

            Assert.Empty(diagnostics);
        }
        
        [Fact]
        public async Task Error_when_sql_expression_recursively_initialized_multi()
        {
            var diagnostics
                = await GetDiagnosticsAsync(
                    @"string q = null;
                      q = M2(q);
                      var s = q;
                      string M2(string _) { return null; }
                      RelationalDatabaseFacadeExtensions.ExecuteSqlCommand(null, s);");

            Assert.Empty(diagnostics);
        }

        [Fact]
        public async Task No_warning_when_string_literal_passed_to_execute_sql_command()
        {
            var diagnostics
                = await GetDiagnosticsAsync(
                    "RelationalDatabaseFacadeExtensions.ExecuteSqlCommand(null, \"select * from Customers\");");

            Assert.Empty(diagnostics);
        }

        [Fact]
        public async Task No_warning_when_string_literal_passed_to_execute_sql_command_async()
        {
            var diagnostics
                = await GetDiagnosticsAsync(
                    "RelationalDatabaseFacadeExtensions.ExecuteSqlCommandAsync(null, \"select * from Customers\");");

            Assert.Empty(diagnostics);
        }

        [Fact]
        public async Task No_warning_when_interpolated_string_passed_to_from_sql()
        {
            var diagnostics
                = await GetDiagnosticsAsync(
                    "RelationalQueryableExtensions.FromSql<object>(null, $\"select {'*'} from Customers\");");

            Assert.Empty(diagnostics);
        }

        [Fact]
        public async Task No_warning_when_interpolated_string_passed_to_execute_sql_command()
        {
            var diagnostics
                = await GetDiagnosticsAsync(
                    "RelationalDatabaseFacadeExtensions.ExecuteSqlCommand(null, $\"select {'*'} from Customers\");");

            Assert.Empty(diagnostics);
        }

        [Fact]
        public async Task No_warning_when_interpolated_string_passed_to_execute_sql_command_async()
        {
            var diagnostics
                = await GetDiagnosticsAsync(
                    "RelationalDatabaseFacadeExtensions.ExecuteSqlCommandAsync(null, $\"select {'*'} from Customers\");");

            Assert.Empty(diagnostics);
        }

        [Fact]
        public async Task No_warning_when_interpolated_string_coerced_to_string_from_sql_when_no_interpolation()
        {
            var diagnostics
                = await GetDiagnosticsAsync(
                    @"var maybe = true;
                      new DbContext(null).Set<object>().FromSql(maybe ? $""bad"" : $""worse"");");

            Assert.Empty(diagnostics);
        }

        [Fact]
        public async Task No_warning_when_interpolated_string_coerced_to_string_from_sql_when_safe_interpolation()
        {
            var diagnostics
                = await GetDiagnosticsAsync(
                    @"var maybe = true;
                      new DbContext(null).Set<object>().FromSql(maybe ? $""{'a'}"" : $""{123}"");");

            Assert.Empty(diagnostics);
        }

        [Fact]
        public async Task Warning_when_interpolated_string_coerced_to_string_passed_to_execute_sql_command_async()
        {
            var diagnostics
                = await GetDiagnosticsAsync(
                    @"var danger = ""boom!"";
                      new DbContext(null).Database.ExecuteSqlCommandAsync((string)$""{danger}"");");

            var diagnostic = diagnostics.Single();

            Assert.Equal(RawSqlStringInjectionDiagnosticAnalyzer.Id, diagnostic.Id);
            Assert.Equal(DiagnosticSeverity.Warning, diagnostic.Severity);
            Assert.Equal(1, diagnostic.Location.GetLineSpan().StartLinePosition.Line);
            Assert.Equal(22, diagnostic.Location.GetLineSpan().StartLinePosition.Character);
            Assert.Equal(string.Format(
                SqlInjectionDiagnosticAnalyzerBase.MessageFormat, "ExecuteSqlCommandAsync"), diagnostic.GetMessage());
        }

        [Fact]
        public async Task Warning_when_interpolated_string_coerced_to_string_passed_to_execute_sql_command_async_deep()
        {
            var diagnostics
                = await GetDiagnosticsAsync(
                    @"var b = true;
                      var danger = ""boom!"";
                      new DbContext(null).Database.ExecuteSqlCommandAsync((string)$""{(!b ? danger : ""good"")}"");");

            var diagnostic = diagnostics.Single();

            Assert.Equal(RawSqlStringInjectionDiagnosticAnalyzer.Id, diagnostic.Id);
            Assert.Equal(DiagnosticSeverity.Warning, diagnostic.Severity);
            Assert.Equal(2, diagnostic.Location.GetLineSpan().StartLinePosition.Line);
            Assert.Equal(22, diagnostic.Location.GetLineSpan().StartLinePosition.Character);
            Assert.Equal(string.Format(
                SqlInjectionDiagnosticAnalyzerBase.MessageFormat, "ExecuteSqlCommandAsync"), diagnostic.GetMessage());
        }

        [Fact]
        public async Task Warning_when_interpolated_string_from_var_passed_to_from_sql()
        {
            var diagnostics
                = await GetDiagnosticsAsync(
                    @"var boom = 123;
                      var danger = $""boom! {boom}"";
                      new DbContext(null).Set<object>().FromSql(danger);");

            var diagnostic = diagnostics.Single();

            Assert.Equal(RawSqlStringInjectionDiagnosticAnalyzer.Id, diagnostic.Id);
            Assert.Equal(DiagnosticSeverity.Warning, diagnostic.Severity);
            Assert.Equal(2, diagnostic.Location.GetLineSpan().StartLinePosition.Line);
            Assert.Equal(22, diagnostic.Location.GetLineSpan().StartLinePosition.Character);
            Assert.Equal(string.Format(
                SqlInjectionDiagnosticAnalyzerBase.MessageFormat, "FromSql"), diagnostic.GetMessage());
        }

        [Fact]
        public async Task Warning_when_interpolated_string_from_var_passed_to_execute_sql_command_transitive()
        {
            var diagnostics
                = await GetDiagnosticsAsync(
                    @"var boom = 123;
                      var danger = $""boom! {boom}"";
                      var extreme = danger;
                      new DbContext(null).Database.ExecuteSqlCommandAsync(extreme);");

            var diagnostic = diagnostics.Single();

            Assert.Equal(RawSqlStringInjectionDiagnosticAnalyzer.Id, diagnostic.Id);
            Assert.Equal(DiagnosticSeverity.Warning, diagnostic.Severity);
            Assert.Equal(3, diagnostic.Location.GetLineSpan().StartLinePosition.Line);
            Assert.Equal(22, diagnostic.Location.GetLineSpan().StartLinePosition.Character);
            Assert.Equal(string.Format(
                SqlInjectionDiagnosticAnalyzerBase.MessageFormat, "ExecuteSqlCommandAsync"), diagnostic.GetMessage());
        }

        protected override DiagnosticAnalyzer CreateDiagnosticAnalyzer()
            => new RawSqlStringInjectionDiagnosticAnalyzer();
    }
}
