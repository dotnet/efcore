// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.Data.Entity.Internal
{
    public class MakeGenericMethodAnalyzerTest : CodeFixVerifier
    {
        public MakeGenericMethodAnalyzerTest(ITestOutputHelper output)
        {
            // uncomment to see output
            // _output = output;
        }

        [Fact]
        public void Blank()
        {
            var test = @"";

            //No diagnostics expected to show up
            VerifyCSharpDiagnostic(test);
        }

        [Fact]
        public void NonReflectionMakeGenericMethod()
        {
            var test = @"    
    namespace ConsoleApplication1
    {
        class TypeName
        {
            public void MakeGenericMethod() {}
            public void Test()
            {
                this.MakeGenericMethod();
            }
        }
    }";

            VerifyCSharpDiagnostic(test);
        }

        [Fact]
        public void CommentSuppressesDiagnostic()
        {
            var test = @"
    using System;
    using System.Reflection;

    namespace ConsoleApplication1
    {
        class TypeName
        {
            // disable MakeGenericMethod inspection
            public void Test()
            {
                typeof(string).GetTypeInfo().GetDeclaredMethod(""ToString"").MakeGenericMethod(typeof(int));
            }
        }
    }";
            VerifyCSharpDiagnostic(test);
        }

        [Fact]
        public void AttributeSuppressesDiagnostic()
        {
            var test = @"
    using System;
    using System.Reflection;

    namespace ConsoleApplication1
    {
        class TypeName
        {
            [GenericMethodFactory]
            public void Test()
            {
                typeof(string).GetTypeInfo().GetDeclaredMethod(""ToString"").MakeGenericMethod(typeof(int));
            }
        }

        public class GenericMethodUsageAttribute : Attribute { } 
    }";
            VerifyCSharpDiagnostic(test);
        }

        [Fact]
        public void CodeFixAddsIgnoreComment()
        {
            var test = @"
using System;
using System.Reflection;

namespace ConsoleApplication1
{
    class TypeName
    {

        public void Test()
        {
            typeof(string).GetTypeInfo().GetDeclaredMethod(""ToString"").MakeGenericMethod(typeof(int));
        }
    }
}";
            var expected = new DiagnosticResult
            {
                Id = MakeGenericMethodAnalyzer.DiagnosticId,
                Message = AnalyzerStrings.UnsafeMakeGenericMethod,
                Severity = DiagnosticSeverity.Info,
                Locations =
                    new[]
                    {
                        new DiagnosticResultLocation("Test0.cs", 12, 13)
                    }
            };

            VerifyCSharpDiagnostic(test, expected);

            var fixtest = @"
using System;
using System.Reflection;

namespace ConsoleApplication1
{
    class TypeName
    {

        // disable MakeGenericMethod inspection
        public void Test()
        {
            typeof(string).GetTypeInfo().GetDeclaredMethod(""ToString"").MakeGenericMethod(typeof(int));
        }
    }
}";
            VerifyCSharpFix(test, fixtest, 1);
        }

        [Fact]
        public void CodeFixStartsAttribute()
        {
            var test = @"
using System;
using System.Reflection;

namespace ConsoleApplication1
{
    class TypeName
    {

        public void Test()
        {
            typeof(string).GetTypeInfo().GetDeclaredMethod(""ToString"").MakeGenericMethod(typeof(int));
        }
    }
}";
            var expected = new DiagnosticResult
            {
                Id = MakeGenericMethodAnalyzer.DiagnosticId,
                Message = AnalyzerStrings.UnsafeMakeGenericMethod,
                Severity = DiagnosticSeverity.Info,
                Locations =
                    new[]
                    {
                        new DiagnosticResultLocation("Test0.cs", 12, 13)
                    }
            };

            VerifyCSharpDiagnostic(test, expected);

            var fixtest = @"
using System;
using System.Reflection;
using Microsoft.Data.Entity.Internal;

namespace ConsoleApplication1
{
    class TypeName
    {
        [GenericMethodFactory(MethodName = nameof(), TypeArguments = new [] { }, TargetType = typeof())]
        public void Test()
        {
            typeof(string).GetTypeInfo().GetDeclaredMethod(""ToString"").MakeGenericMethod(typeof(int));
        }
    }
}";
            VerifyCSharpFix(test, fixtest, 0);
        }

        protected override CodeFixProvider GetCSharpCodeFixProvider() => new MakeGenericMethodCodeFixProvider();

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() => new MakeGenericMethodAnalyzer();
    }
}
