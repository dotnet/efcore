using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Testing;
using Xunit;

using VerifyWithSuppressor = Microsoft.EntityFrameworkCore.TestUtilities.Verifiers.CSharpCodeFixVerifier<
    Microsoft.EntityFrameworkCore.NullableDiagnosticSuppressor,
    Microsoft.CodeAnalysis.Testing.EmptyCodeFixProvider>;

using VerifyWithoutSuppressor = Microsoft.EntityFrameworkCore.TestUtilities.Verifiers.CSharpCodeFixVerifier<
    Microsoft.CodeAnalysis.Testing.EmptyDiagnosticAnalyzer,
    Microsoft.CodeAnalysis.Testing.EmptyCodeFixProvider>;

namespace Microsoft.EntityFrameworkCore
{
    public class NullableDiagnosticSuppressorTest
    {
        /// <summary>
        /// Assert that given code have warnings specified by markup that are suppressed by <see cref="NullableDiagnosticSuppressor"/>
        /// </summary>
        private static async Task AssertWarningIsSuppressedAsync(string code)
        {
            await VerifyWithoutSuppressor.VerifyAnalyzerAsync(code);

            await new VerifyWithSuppressor.Test()
            {
                TestState = { Sources = { code }, MarkupHandling = MarkupMode.Ignore }
            }.RunAsync();
        }

        /// <summary>
        /// Assert that given code have warnings specified by markup that are not suppressed by <see cref="NullableDiagnosticSuppressor"/>
        /// </summary>
        private static async Task AssertWarningIsNotSuppressedAsync(string code)
        {
            await VerifyWithSuppressor.VerifyAnalyzerAsync(code);
            await VerifyWithoutSuppressor.VerifyAnalyzerAsync(code);
        }

        [Fact]
        public async Task TestCompilerWarningAsync()
        {
            var code = @"
#nullable enable

class C
{
    public void M(C? c)
    {
        _ = {|CS8602:c|}.ToString();
    }
}";
            await AssertWarningIsNotSuppressedAsync(code);
        }

        [Fact]
        public async Task TestEFCoreIncludeIsSuppressed()
        {
            var code = @"
#nullable enable

using System.Linq;
using Microsoft.EntityFrameworkCore;

class SomeModel
{
    public NavigationModel? Navigation { get; set; }
}

class NavigationModel
{
    public string DeeperNavigation { get; set; } = null!;
}

static class C
{
    public static void M(IQueryable<SomeModel> q)
    {
        _ = q.Include(m => m.Navigation).ThenInclude(n => {|CS8602:n|}.DeeperNavigation);
    }
}
";
            await AssertWarningIsSuppressedAsync(code);
        }
    }
}
