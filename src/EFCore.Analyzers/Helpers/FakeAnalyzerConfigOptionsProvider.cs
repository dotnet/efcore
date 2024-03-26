// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Microsoft.EntityFrameworkCore;

public sealed class FakeAnalyzerConfigOptionsProvider(params (string, string)[] globalOptions) : AnalyzerConfigOptionsProvider
{
    public override AnalyzerConfigOptions GlobalOptions { get; } = new ConfigOptions(globalOptions);

    public override AnalyzerConfigOptions GetOptions(SyntaxTree tree)
        => GlobalOptions;

    public override AnalyzerConfigOptions GetOptions(AdditionalText textFile)
        => GlobalOptions;

    private sealed class ConfigOptions : AnalyzerConfigOptions
    {
        private readonly Dictionary<string, string> _globalOptions;

        public ConfigOptions((string, string)[] globalOptions)
            => _globalOptions = globalOptions.ToDictionary(t => t.Item1, t => t.Item2);

        public override bool TryGetValue(string key, [NotNullWhen(true)] out string? value)
            => _globalOptions.TryGetValue(key, out value);
    }
}

