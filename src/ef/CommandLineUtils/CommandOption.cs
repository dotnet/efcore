// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;

namespace Microsoft.DotNet.Cli.CommandLine;

internal class CommandOption
{
    public CommandOption(string template, CommandOptionType optionType)
    {
        Template = template;
        OptionType = optionType;
        Values = [];

        foreach (var part in Template.Split(new[] { ' ', '|' }, StringSplitOptions.RemoveEmptyEntries))
        {
            if (part.StartsWith("--", StringComparison.Ordinal))
            {
                LongName = part.Substring(2);
            }
            else if (part.StartsWith("-", StringComparison.Ordinal))
            {
                var optName = part.Substring(1);

                // If there is only one char and it is not an English letter, it is a symbol option (e.g. "-?")
                if (optName.Length == 1
                    && !IsEnglishLetter(optName[0]))
                {
                    SymbolName = optName;
                }
                else
                {
                    ShortName = optName;
                }
            }
            else if (part.StartsWith("<", StringComparison.Ordinal)
                     && part.EndsWith(">", StringComparison.Ordinal))
            {
                ValueName = part.Substring(1, part.Length - 2);
            }
            else if (optionType == CommandOptionType.MultipleValue
                     && part.StartsWith("<", StringComparison.Ordinal)
                     && part.EndsWith(">...", StringComparison.Ordinal))
            {
                ValueName = part.Substring(1, part.Length - 5);
            }
            else
            {
                throw new ArgumentException($"Invalid template pattern '{template}'", nameof(template));
            }
        }

        if (string.IsNullOrEmpty(LongName)
            && string.IsNullOrEmpty(ShortName)
            && string.IsNullOrEmpty(SymbolName))
        {
            throw new ArgumentException($"Invalid template pattern '{template}'", nameof(template));
        }
    }

    public string Template { get; set; }
    public string? ShortName { get; set; }
    public string? LongName { get; set; }
    public string? SymbolName { get; set; }
    public string? ValueName { get; set; }
    public string? Description { get; set; }
    public List<string?> Values { get; }
    public bool? BoolValue { get; private set; }
    public CommandOptionType OptionType { get; }

    public bool TryParse(string? value)
    {
        switch (OptionType)
        {
            case CommandOptionType.MultipleValue:
                Values.Add(value);
                break;
            case CommandOptionType.SingleValue:
                if (Values.Count > 0)
                {
                    return false;
                }

                Values.Add(value);
                break;
            case CommandOptionType.BoolValue:
                if (Values.Count > 0)
                {
                    return false;
                }

                if (value == null)
                {
                    // add null to indicate that the option was present, but had no value
                    Values.Add(null);
                    BoolValue = true;
                }
                else
                {
                    if (!bool.TryParse(value, out var boolValue))
                    {
                        return false;
                    }

                    Values.Add(value);
                    BoolValue = boolValue;
                }

                break;
            case CommandOptionType.NoValue:
                if (value != null)
                {
                    return false;
                }

                // Add a value to indicate that this option was specified
                Values.Add("on");
                break;
        }

        return true;
    }

    public bool HasValue()
        => Values.Count > 0;

    public string? Value()
        => HasValue() ? Values[0] : null;

    private static bool IsEnglishLetter(char c)
        => c is >= 'a' and <= 'z' or >= 'A' and <= 'Z';
}
