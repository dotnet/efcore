// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.DotNet.Cli.CommandLine;

internal class CommandLineApplication(bool throwOnUnexpectedArg = true)
{
    private enum ParseOptionResult
    {
        Succeeded,
        ShowHelp,
        ShowVersion,
        UnexpectedArgs
    }

    // Indicates whether the parser should throw an exception when it runs into an unexpected argument.
    // If this field is set to false, the parser will stop parsing when it sees an unexpected argument, and all
    // remaining arguments, including the first unexpected argument, will be stored in RemainingArguments property.
    private readonly bool _throwOnUnexpectedArg = throwOnUnexpectedArg;

    public CommandLineApplication? Parent { get; set; }
    public string? Name { get; set; }
    public string? FullName { get; set; }
    public string? Syntax { get; set; }
    public string? Description { get; set; }
    public List<CommandOption> Options { get; } = [];
    public CommandOption? OptionHelp { get; private set; }
    public CommandOption? OptionVersion { get; private set; }
    public List<CommandArgument> Arguments { get; } = [];
    public List<string> RemainingArguments { get; } = [];
    public List<string> ApplicationArguments { get; } = [];
    public bool IsShowingInformation { get; protected set; } // Is showing help or version?
    public Func<string[], int> Invoke { get; set; } = _ => 0;
    public Func<string>? LongVersionGetter { get; set; }
    public Func<string>? ShortVersionGetter { get; set; }
    public List<CommandLineApplication> Commands { get; } = [];
    public bool HandleResponseFiles { get; set; }
    public bool AllowArgumentSeparator { get; set; }
    public bool HandleRemainingArguments { get; set; }
    public string? ArgumentSeparatorHelpText { get; set; }

    public CommandLineApplication Command(string name, bool throwOnUnexpectedArg = true)
        => Command(name, _ => { }, throwOnUnexpectedArg);

    public CommandLineApplication Command(
        string name,
        Action<CommandLineApplication> configuration,
        bool throwOnUnexpectedArg = true)
    {
        var command = new CommandLineApplication(throwOnUnexpectedArg) { Name = name, Parent = this };
        Commands.Add(command);
        configuration(command);
        return command;
    }

    public CommandOption Option(string template, string? description, CommandOptionType optionType)
        => Option(template, description, optionType, _ => { });

    public CommandOption Option(string template, string? description, CommandOptionType optionType, Action<CommandOption> configuration)
    {
        var option = new CommandOption(template, optionType) { Description = description };
        Options.Add(option);
        configuration(option);
        return option;
    }

    public CommandArgument Argument(string name, string description, bool multipleValues = false)
        => Argument(name, description, _ => { }, multipleValues);

    public CommandArgument Argument(string name, string description, Action<CommandArgument> configuration, bool multipleValues = false)
    {
        var lastArg = Arguments.LastOrDefault();
        if (lastArg?.MultipleValues == true)
        {
            var message = $"The last argument '{lastArg.Name}' accepts multiple values. No more argument can be added.";
            throw new InvalidOperationException(message);
        }

        var argument = new CommandArgument
        {
            Name = name,
            Description = description,
            MultipleValues = multipleValues
        };
        Arguments.Add(argument);
        configuration(argument);
        return argument;
    }

    public void OnExecute(Func<string[], int> invoke)
        => Invoke = invoke;

    public void OnExecute(Func<string[], Task<int>> invoke)
        => Invoke = args => invoke(args).Result;

    public int Execute(params string[] args)
    {
        var command = this;
        IEnumerator<CommandArgument>? arguments = null;

        if (HandleResponseFiles)
        {
            args = ExpandResponseFiles(args).ToArray();
        }

        try
        {
            for (var index = 0; index < args.Length; index++)
            {
                var arg = args[index];

                var isLongOption = arg.StartsWith("--", StringComparison.Ordinal);
                if (isLongOption || arg.StartsWith("-", StringComparison.Ordinal))
                {
                    var result = ParseOption(isLongOption, command, args, ref index, out _);
                    if (result == ParseOptionResult.ShowHelp)
                    {
                        command.ShowHelp();
                        return 0;
                    }

                    if (result == ParseOptionResult.ShowVersion)
                    {
                        command.ShowVersion();
                        return 0;
                    }
                }
                else
                {
                    var subcommand = ParseSubCommand(arg, command);
                    if (subcommand != null)
                    {
                        command = subcommand;
                    }
                    else
                    {
                        arguments ??= new CommandArgumentEnumerator(command.Arguments.GetEnumerator());

                        if (arguments.MoveNext())
                        {
                            arguments.Current.Values.Add(arg);
                        }
                        else
                        {
                            HandleUnexpectedArg(command, args, index, argTypeName: "command or argument");
                        }
                    }
                }
            }
        }
        finally
        {
            arguments?.Dispose();
        }

        return command.Invoke(command.ApplicationArguments.ToArray());
    }

    private static ParseOptionResult ParseOption(
        bool isLongOption,
        CommandLineApplication command,
        string[] args,
        ref int index,
        out CommandOption? option)
    {
        option = null;
        var result = ParseOptionResult.Succeeded;
        var arg = args[index];

        var optionPrefixLength = isLongOption ? 2 : 1;
        var optionComponents = arg.Substring(optionPrefixLength).Split([':', '='], 2);
        var optionName = optionComponents[0];

        if (isLongOption)
        {
            option = command.Options.SingleOrDefault(
                opt => string.Equals(opt.LongName, optionName, StringComparison.Ordinal));
        }
        else
        {
            option = command.Options.SingleOrDefault(
                    opt => string.Equals(opt.ShortName, optionName, StringComparison.Ordinal))
                ?? command.Options.SingleOrDefault(
                    opt => string.Equals(opt.SymbolName, optionName, StringComparison.Ordinal));
        }

        if (option == null)
        {
            if (isLongOption
                && string.IsNullOrEmpty(optionName)
                && command.AllowArgumentSeparator)
            {
                // a stand-alone "--" is the argument separator, so skip it and
                // handle the rest of the args as application args
                for (index++; index < args.Length; index++)
                {
                    command.ApplicationArguments.Add(args[index]);
                }
            }
            else
            {
                HandleUnexpectedArg(command, args, index, argTypeName: "option");
            }

            result = ParseOptionResult.UnexpectedArgs;
        }
        else if (command.OptionHelp == option)
        {
            result = ParseOptionResult.ShowHelp;
        }
        else if (command.OptionVersion == option)
        {
            result = ParseOptionResult.ShowVersion;
        }
        else
        {
            if (optionComponents.Length == 2)
            {
                if (!option.TryParse(optionComponents[1]))
                {
                    command.ShowHint();
                    throw new CommandParsingException(
                        command,
                        $"Unexpected value '{optionComponents[1]}' for option '{optionName}'");
                }
            }
            else
            {
                if (option.OptionType is CommandOptionType.NoValue or CommandOptionType.BoolValue)
                {
                    // No value is needed for this option
                    option.TryParse(null);
                }
                else
                {
                    index++;
                    arg = args[index];
                    if (!option.TryParse(arg))
                    {
                        command.ShowHint();
                        throw new CommandParsingException(command, $"Unexpected value '{arg}' for option '{optionName}'");
                    }
                }
            }
        }

        return result;
    }

    private static CommandLineApplication? ParseSubCommand(string arg, CommandLineApplication command)
    {
        foreach (var subcommand in command.Commands)
        {
            if (string.Equals(subcommand.Name, arg, StringComparison.OrdinalIgnoreCase))
            {
                return subcommand;
            }
        }

        return null;
    }

    // Helper method that adds a help option
    public CommandOption HelpOption(string template)
    {
        // Help option is special because we stop parsing once we see it
        // So we store it separately for further use
        OptionHelp = Option(template, "Show help information", CommandOptionType.NoValue);

        return OptionHelp;
    }

    public CommandOption VersionOption(
        string template,
        string shortFormVersion,
        string? longFormVersion = null)
        => longFormVersion == null
            ? VersionOption(template, () => shortFormVersion)
            : VersionOption(template, () => shortFormVersion, () => longFormVersion);

    // Helper method that adds a version option
    public CommandOption VersionOption(
        string template,
        Func<string> shortFormVersionGetter,
        Func<string>? longFormVersionGetter = null)
    {
        // Version option is special because we stop parsing once we see it
        // So we store it separately for further use
        OptionVersion = Option(template, "Show version information", CommandOptionType.NoValue);
        ShortVersionGetter = shortFormVersionGetter;
        LongVersionGetter = longFormVersionGetter ?? shortFormVersionGetter;

        return OptionVersion;
    }

    // Show short hint that reminds users to use help option
    public void ShowHint()
    {
        if (OptionHelp != null)
        {
            Console.WriteLine("Specify --{0} for a list of available options and commands.", OptionHelp.LongName);
        }
    }

    // Show full help
    public void ShowHelp(string? commandName = null)
    {
        var headerBuilder = new StringBuilder("Usage:");
        var usagePrefixLength = headerBuilder.Length;
        for (var cmd = this; cmd != null; cmd = cmd.Parent)
        {
            cmd.IsShowingInformation = true;
            if (cmd != this
                && cmd.Arguments.Count > 0)
            {
                var args = string.Join(" ", cmd.Arguments.Select(arg => arg.Name));
                headerBuilder.Insert(usagePrefixLength, $" {cmd.Name} {args}");
            }
            else
            {
                headerBuilder.Insert(usagePrefixLength, $" {cmd.Name}");
            }
        }

        CommandLineApplication? target;

        if (commandName == null
            || string.Equals(Name, commandName, StringComparison.OrdinalIgnoreCase))
        {
            target = this;
        }
        else
        {
            target = Commands.SingleOrDefault(cmd => string.Equals(cmd.Name, commandName, StringComparison.OrdinalIgnoreCase));

            if (target != null)
            {
                headerBuilder.AppendFormat(" {0}", commandName);
            }
            else
            {
                // The command name is invalid so don't try to show help for something that doesn't exist
                target = this;
            }
        }

        var optionsBuilder = new StringBuilder();
        var commandsBuilder = new StringBuilder();
        var argumentsBuilder = new StringBuilder();
        var argumentSeparatorBuilder = new StringBuilder();

        var maxArgLen = 0;
        for (var cmd = target; cmd != null; cmd = cmd.Parent)
        {
            if (cmd.Arguments.Count > 0)
            {
                if (cmd == target)
                {
                    headerBuilder.Append(" [arguments]");
                }

                if (argumentsBuilder.Length == 0)
                {
                    argumentsBuilder.AppendLine();
                    argumentsBuilder.AppendLine("Arguments:");
                }

                maxArgLen = Math.Max(maxArgLen, MaxArgumentLength(cmd.Arguments));
            }
        }

        for (var cmd = target; cmd != null; cmd = cmd.Parent)
        {
            if (cmd.Arguments.Count > 0)
            {
                const string outputFormat = "  {0}{1}";
                foreach (var arg in cmd.Arguments)
                {
                    argumentsBuilder.AppendFormat(
                        outputFormat,
                        arg.Name!.PadRight(maxArgLen + 2),
                        arg.Description);
                    argumentsBuilder.AppendLine();
                }
            }
        }

        if (target.Options.Count > 0)
        {
            headerBuilder.Append(" [options]");

            optionsBuilder.AppendLine();
            optionsBuilder.AppendLine("Options:");
            var maxOptLen = MaxOptionTemplateLength(target.Options);
            var outputFormat = $"  {{0, -{maxOptLen + 2}}}{{1}}";
            foreach (var opt in target.Options)
            {
                optionsBuilder.AppendFormat(outputFormat, opt.Template, opt.Description);
                optionsBuilder.AppendLine();
            }
        }

        if (target.Commands.Count > 0)
        {
            headerBuilder.Append(" [command]");

            commandsBuilder.AppendLine();
            commandsBuilder.AppendLine("Commands:");
            var maxCmdLen = MaxCommandLength(target.Commands);
            var outputFormat = $"  {{0, -{maxCmdLen + 2}}}{{1}}";
            foreach (var cmd in target.Commands.OrderBy(c => c.Name))
            {
                commandsBuilder.AppendFormat(outputFormat, cmd.Name, cmd.Description);
                commandsBuilder.AppendLine();
            }

            if (OptionHelp != null)
            {
                commandsBuilder.AppendLine();
                commandsBuilder.AppendFormat("Use \"{0} [command] --help\" for more information about a command.", Name);
                commandsBuilder.AppendLine();
            }
        }

        if (target.AllowArgumentSeparator
            || target.HandleRemainingArguments)
        {
            headerBuilder.Append(target.AllowArgumentSeparator ? " [[--] <arg>...]]" : " [args]");

            if (!string.IsNullOrEmpty(target.ArgumentSeparatorHelpText))
            {
                argumentSeparatorBuilder.AppendLine();
                argumentSeparatorBuilder.AppendLine("Args:");
                argumentSeparatorBuilder.Append("  ").AppendLine(target.ArgumentSeparatorHelpText);
                argumentSeparatorBuilder.AppendLine();
            }
        }

        headerBuilder.AppendLine();

        var nameAndVersion = new StringBuilder();
        nameAndVersion.AppendLine(GetFullNameAndVersion());
        nameAndVersion.AppendLine();

        Console.Write(
            "{0}{1}{2}{3}{4}{5}", nameAndVersion, headerBuilder, argumentsBuilder, optionsBuilder, commandsBuilder,
            argumentSeparatorBuilder);
    }

    public void ShowVersion()
    {
        for (var cmd = this; cmd != null; cmd = cmd.Parent)
        {
            cmd.IsShowingInformation = true;
        }

        Console.WriteLine(FullName);
        Console.WriteLine(LongVersionGetter!());
    }

    public string? GetFullNameAndVersion()
        => ShortVersionGetter == null ? FullName : $"{FullName} {ShortVersionGetter()}";

    public void ShowRootCommandFullNameAndVersion()
    {
        var rootCmd = this;
        while (rootCmd.Parent != null)
        {
            rootCmd = rootCmd.Parent;
        }

        Console.WriteLine(rootCmd.GetFullNameAndVersion());
        Console.WriteLine();
    }

    private static int MaxOptionTemplateLength(IEnumerable<CommandOption> options)
    {
        var maxLen = 0;
        foreach (var opt in options)
        {
            maxLen = opt.Template.Length > maxLen ? opt.Template.Length : maxLen;
        }

        return maxLen;
    }

    private static int MaxCommandLength(IEnumerable<CommandLineApplication> commands)
    {
        var maxLen = 0;
        foreach (var cmd in commands)
        {
            maxLen = cmd.Name!.Length > maxLen ? cmd.Name.Length : maxLen;
        }

        return maxLen;
    }

    private static int MaxArgumentLength(IEnumerable<CommandArgument> arguments)
    {
        var maxLen = 0;
        foreach (var arg in arguments)
        {
            maxLen = arg.Name!.Length > maxLen ? arg.Name.Length : maxLen;
        }

        return maxLen;
    }

    private static void HandleUnexpectedArg(CommandLineApplication command, string[] args, int index, string argTypeName)
    {
        if (command._throwOnUnexpectedArg)
        {
            command.ShowHint();
            throw new CommandParsingException(command, $"Unrecognized {argTypeName} '{args[index]}'");
        }

        command.RemainingArguments.Add(args[index]);
    }

    private IEnumerable<string> ExpandResponseFiles(IEnumerable<string> args)
    {
        foreach (var arg in args)
        {
            if (!arg.StartsWith("@", StringComparison.Ordinal))
            {
                yield return arg;
            }
            else
            {
                var fileName = arg.Substring(1);

                var responseFileArguments = ParseResponseFile(fileName);

                // ParseResponseFile can suppress expanding this response file by
                // returning null. In that case, we'll treat the response
                // file token as a regular argument.

                if (responseFileArguments == null)
                {
                    yield return arg;
                }
                else
                {
                    foreach (var responseFileArgument in responseFileArguments)
                    {
                        yield return responseFileArgument.Trim();
                    }
                }
            }
        }
    }

    private IEnumerable<string>? ParseResponseFile(string fileName)
    {
        if (!HandleResponseFiles)
        {
            return null;
        }

        if (!File.Exists(fileName))
        {
            throw new InvalidOperationException($"Response file '{fileName}' doesn't exist.");
        }

        return File.ReadLines(fileName);
    }

    private sealed class CommandArgumentEnumerator : IEnumerator<CommandArgument>
    {
        private readonly IEnumerator<CommandArgument> _enumerator;

        public CommandArgumentEnumerator(IEnumerator<CommandArgument> enumerator)
        {
            _enumerator = enumerator;
        }

        public CommandArgument Current
            => _enumerator.Current;

        object IEnumerator.Current
            => Current;

        public void Dispose()
            => _enumerator.Dispose();

        public bool MoveNext()
            // If current argument allows multiple values, we don't move forward and
            // all later values will be added to current CommandArgument.Values
            => Current?.MultipleValues == true
                || _enumerator.MoveNext();

        public void Reset()
            => _enumerator.Reset();
    }
}
