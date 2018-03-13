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
    public class CommandTextInjectionDiagnosticAnalyzerTest : DiagnosticAnalyzerTestBase
    {
        [Fact]
        public async Task No_warning_when_string_literal_passed_to_command_text()
        {
            var diagnostics
                = await GetDiagnosticsAsync(
                    "new SqlCommand().CommandText = \"select * from Customers\";");

            Assert.Empty(diagnostics);
        }

        [Fact]
        public async Task No_warning_when_interpolated_string_passed_to_command_text()
        {
            var diagnostics
                = await GetDiagnosticsAsync(
                    "new SqlCommand().CommandText = $\"select {'*'} from Customers\";");

            Assert.Empty(diagnostics);
        }

        [Fact]
        public async Task Warning_when_string_format_with_arg_passed_to_command_text()
        {
            var diagnostics
                = await GetDiagnosticsAsync(
                    @"var danger = ""boom!"";
                      new SqlCommand().CommandText = string.Format(""blah"", danger);");

            var diagnostic = diagnostics.Single();

            Assert.Equal(CommandTextInjectionDiagnosticAnalyzer.Id, diagnostic.Id);
            Assert.Equal(DiagnosticSeverity.Warning, diagnostic.Severity);
            Assert.Equal(1, diagnostic.Location.GetLineSpan().StartLinePosition.Line);
            Assert.Equal(22, diagnostic.Location.GetLineSpan().StartLinePosition.Character);
            Assert.Equal(string.Format(CommandTextInjectionDiagnosticAnalyzer.MessageFormat, "CommandText"), diagnostic.GetMessage());
        }

        [Fact]
        public async Task Warning_when_string_format_with_arg_passed_to_command_text_deep()
        {
            var diagnostics
                = await GetDiagnosticsAsync(
                    @"var b = true;
                      var danger = ""boom!"";
                      new SqlCommand().CommandText = b ? string.Format(""blah"", danger) : ""doof doof"";");

            var diagnostic = diagnostics.Single();

            Assert.Equal(CommandTextInjectionDiagnosticAnalyzer.Id, diagnostic.Id);
            Assert.Equal(DiagnosticSeverity.Warning, diagnostic.Severity);
            Assert.Equal(2, diagnostic.Location.GetLineSpan().StartLinePosition.Line);
            Assert.Equal(22, diagnostic.Location.GetLineSpan().StartLinePosition.Character);
            Assert.Equal(string.Format(CommandTextInjectionDiagnosticAnalyzer.MessageFormat, "CommandText"), diagnostic.GetMessage());
        }

        [Fact]
        public async Task Warning_when_string_concat_op_with_arg_passed_to_command_text_deep()
        {
            var diagnostics
                = await GetDiagnosticsAsync(
                    @"var b = true;
                      var danger = ""boom!"";
                      new SqlCommand().CommandText = b ? ""select"" + danger : ""doof doof"";");

            var diagnostic = diagnostics.Single();

            Assert.Equal(CommandTextInjectionDiagnosticAnalyzer.Id, diagnostic.Id);
            Assert.Equal(DiagnosticSeverity.Warning, diagnostic.Severity);
            Assert.Equal(2, diagnostic.Location.GetLineSpan().StartLinePosition.Line);
            Assert.Equal(22, diagnostic.Location.GetLineSpan().StartLinePosition.Character);
            Assert.Equal(string.Format(CommandTextInjectionDiagnosticAnalyzer.MessageFormat, "CommandText"), diagnostic.GetMessage());
        }

        [Fact]
        public async Task Warning_when_string_concat_with_arg_passed_to_command_text_deep()
        {
            var diagnostics
                = await GetDiagnosticsAsync(
                    @"var b = true;
                      var danger = ""boom!"";
                      new SqlCommand().CommandText = b ? string.Concat(""select"", danger) : ""doof doof"";");

            var diagnostic = diagnostics.Single();

            Assert.Equal(CommandTextInjectionDiagnosticAnalyzer.Id, diagnostic.Id);
            Assert.Equal(DiagnosticSeverity.Warning, diagnostic.Severity);
            Assert.Equal(2, diagnostic.Location.GetLineSpan().StartLinePosition.Line);
            Assert.Equal(22, diagnostic.Location.GetLineSpan().StartLinePosition.Character);
            Assert.Equal(string.Format(CommandTextInjectionDiagnosticAnalyzer.MessageFormat, "CommandText"), diagnostic.GetMessage());
        }

        [Fact]
        public async Task Warning_when_string_insert_with_arg_passed_to_command_text_deep()
        {
            var diagnostics
                = await GetDiagnosticsAsync(
                    @"var b = true;
                      var danger = ""boom!"";
                      new SqlCommand().CommandText = b ? ""select"".Insert(0, danger) : ""doof doof"";");

            var diagnostic = diagnostics.Single();

            Assert.Equal(CommandTextInjectionDiagnosticAnalyzer.Id, diagnostic.Id);
            Assert.Equal(DiagnosticSeverity.Warning, diagnostic.Severity);
            Assert.Equal(2, diagnostic.Location.GetLineSpan().StartLinePosition.Line);
            Assert.Equal(22, diagnostic.Location.GetLineSpan().StartLinePosition.Character);
            Assert.Equal(string.Format(CommandTextInjectionDiagnosticAnalyzer.MessageFormat, "CommandText"), diagnostic.GetMessage());
        }

        [Fact]
        public async Task Warning_when_interpolated_string_passed_to_command_text()
        {
            var diagnostics
                = await GetDiagnosticsAsync(
                    @"var boom = 123;
                      new SqlCommand().CommandText = $""boom! {boom}"";");

            var diagnostic = diagnostics.Single();

            Assert.Equal(CommandTextInjectionDiagnosticAnalyzer.Id, diagnostic.Id);
            Assert.Equal(DiagnosticSeverity.Warning, diagnostic.Severity);
            Assert.Equal(1, diagnostic.Location.GetLineSpan().StartLinePosition.Line);
            Assert.Equal(22, diagnostic.Location.GetLineSpan().StartLinePosition.Character);
            Assert.Equal(string.Format(CommandTextInjectionDiagnosticAnalyzer.MessageFormat, "CommandText"), diagnostic.GetMessage());
        }

        [Fact]
        public async Task Warning_when_interpolated_string_from_var_passed_to_command_text()
        {
            var diagnostics
                = await GetDiagnosticsAsync(
                    @"var boom = 123;
                      var danger = $""boom! {boom}"";
                      new SqlCommand().CommandText = danger;");

            var diagnostic = diagnostics.Single();

            Assert.Equal(CommandTextInjectionDiagnosticAnalyzer.Id, diagnostic.Id);
            Assert.Equal(DiagnosticSeverity.Warning, diagnostic.Severity);
            Assert.Equal(2, diagnostic.Location.GetLineSpan().StartLinePosition.Line);
            Assert.Equal(22, diagnostic.Location.GetLineSpan().StartLinePosition.Character);
            Assert.Equal(string.Format(CommandTextInjectionDiagnosticAnalyzer.MessageFormat, "CommandText"), diagnostic.GetMessage());
        }

        [Fact]
        public async Task Warning_when_interpolated_string_from_var_passed_to_command_text_transitive()
        {
            var diagnostics
                = await GetDiagnosticsAsync(
                    @"var boom = 123;
                      var danger = $""boom! {boom}"";
                      var extreme = danger;
                      new SqlCommand().CommandText = extreme;");

            var diagnostic = diagnostics.Single();

            Assert.Equal(CommandTextInjectionDiagnosticAnalyzer.Id, diagnostic.Id);
            Assert.Equal(DiagnosticSeverity.Warning, diagnostic.Severity);
            Assert.Equal(3, diagnostic.Location.GetLineSpan().StartLinePosition.Line);
            Assert.Equal(22, diagnostic.Location.GetLineSpan().StartLinePosition.Character);
            Assert.Equal(string.Format(CommandTextInjectionDiagnosticAnalyzer.MessageFormat, "CommandText"), diagnostic.GetMessage());
        }

        protected override DiagnosticAnalyzer CreateDiagnosticAnalyzer()
            => new CommandTextInjectionDiagnosticAnalyzer();
    }
}
