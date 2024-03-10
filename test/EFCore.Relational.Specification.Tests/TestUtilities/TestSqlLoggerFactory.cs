// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Concurrent;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Microsoft.EntityFrameworkCore.TestUtilities;

public class TestSqlLoggerFactory : ListLoggerFactory
{
    private readonly bool _proceduralQueryGeneration = false;

    private const string FileNewLine = @"
";

    private static readonly string _eol = Environment.NewLine;

    private static readonly object _queryBaselineFileLock = new();
    private static readonly ConcurrentDictionary<string, QueryBaselineRewritingFileInfo> _queryBaselineRewritingFileInfos = new();

    public TestSqlLoggerFactory()
        : this(_ => true)
    {
    }

    public TestSqlLoggerFactory(Func<string, bool> shouldLogCategory)
        : base(c => shouldLogCategory(c) || c == DbLoggerCategory.Database.Command.Name)
    {
        Logger = new TestSqlLogger(shouldLogCategory(DbLoggerCategory.Database.Command.Name));
    }

    public IReadOnlyList<string> SqlStatements
        => ((TestSqlLogger)Logger).SqlStatements;

    public IReadOnlyList<string> Parameters
        => ((TestSqlLogger)Logger).Parameters;

    public string Sql
        => string.Join(_eol + _eol, SqlStatements);

    public void OutputSql()
    {
        Logger.TestOutputHelper?.WriteLine("SQL sent to the database:");
        Logger.TestOutputHelper?.WriteLine(Sql);
    }

    public void AssertBaseline(string[] expected, bool assertOrder = true, bool forUpdate = false)
    {
        if (_proceduralQueryGeneration)
        {
            return;
        }

        var offset = forUpdate ? 1 : 0;
        var count = SqlStatements.Count - offset - offset;
        try
        {
            if (assertOrder)
            {
                for (var i = 0; i < expected.Length; i++)
                {
                    Assert.Equal(expected[i], SqlStatements[i + offset], ignoreLineEndingDifferences: true);
                }

                Assert.Empty(SqlStatements.Skip(expected.Length + offset + offset));
            }
            else
            {
                foreach (var expectedFragment in expected)
                {
                    var normalizedExpectedFragment = NormalizeLineEndings(expectedFragment);
                    Assert.Contains(
                        normalizedExpectedFragment,
                        SqlStatements);
                }
            }
        }
        catch
        {
            var methodCallLine = Environment.StackTrace.Split(
                [_eol],
                StringSplitOptions.RemoveEmptyEntries)[3][6..];

            var indexMethodEnding = methodCallLine.IndexOf(')') + 1;
            var testName = methodCallLine.Substring(0, indexMethodEnding);
            var parts = methodCallLine[indexMethodEnding..].Split(" ", StringSplitOptions.RemoveEmptyEntries);
            var fileName = parts[1][..^5];
            var lineNumber = int.Parse(parts[2]);

            var currentDirectory = Directory.GetCurrentDirectory();
            var logFile = currentDirectory.Substring(
                    0,
                    currentDirectory.LastIndexOf(
                        $"{Path.DirectorySeparatorChar}artifacts{Path.DirectorySeparatorChar}",
                        StringComparison.Ordinal)
                    + 1)
                + "QueryBaseline.txt";

            var testInfo = testName + " : " + lineNumber + FileNewLine;
            const string indent = FileNewLine + "                ";

            if (Environment.GetEnvironmentVariable("EF_TEST_REWRITE_BASELINES")?.ToUpper() is "1" or "TRUE")
            {
                RewriteSourceWithNewBaseline(fileName, lineNumber);
            }

            var sql = string.Join(
                "," + indent + "//" + indent,
                SqlStatements.Skip(offset).Take(count).Select(sql => "\"\"\"" + FileNewLine + sql + FileNewLine + "\"\"\""));

            var newBaseLine = $@"        Assert{(forUpdate ? "ExecuteUpdate" : "")}Sql(
{sql});

";

            if (SqlStatements.Count > 20)
            {
                newBaseLine += "Output truncated.";
            }

            Logger.TestOutputHelper?.WriteLine("---- New Baseline -------------------------------------------------------------------");
            Logger.TestOutputHelper?.WriteLine(newBaseLine);

            var contents = testInfo + newBaseLine + FileNewLine + "--------------------" + FileNewLine;

            lock (_queryBaselineFileLock)
            {
                File.AppendAllText(logFile, contents);
            }

            throw;
        }

        void RewriteSourceWithNewBaseline(string fileName, int lineNumber)
        {
            var fileInfo = _queryBaselineRewritingFileInfos.GetOrAdd(fileName, _ => new QueryBaselineRewritingFileInfo());
            lock (fileInfo.Lock)
            {
                // First, adjust our lineNumber to take into account any baseline rewriting that already occurred in this file
                var origLineNumber = lineNumber;
                foreach (var displacement in fileInfo.LineDisplacements)
                {
                    if (displacement.Key < origLineNumber)
                    {
                        lineNumber += displacement.Value;
                    }
                    else
                    {
                        break;
                    }
                }

                // Parse the file to find the line where the relevant AssertSql is
                try
                {
                    // First have Roslyn parse the file
                    SyntaxTree syntaxTree;
                    using (var stream = File.OpenRead(fileName))
                    using (var bufferedStream = new BufferedStream(stream))
                    {
                        syntaxTree = CSharpSyntaxTree.ParseText(SourceText.From(bufferedStream));
                    }

                    // Read through the source file, copying contents to a temp file (with the baseline change)
                    using (var inputFileStream = File.OpenRead(fileName))
                    using (var inputStream = new BufferedStream(inputFileStream))
                    using (var outputFileStream = File.Open(fileName + ".tmp", FileMode.Create, FileAccess.Write))
                    using (var outputStream = new BufferedStream(outputFileStream))
                    {
                        // Detect whether a byte-order mark (BOM) exists, to write out the same
                        var buffer = new byte[3];
                        inputStream.Read(buffer, 0, 3);
                        inputStream.Position = 0;

                        var hasUtf8ByteOrderMark = (buffer[0] == 0xEF && buffer[1] == 0xBB && buffer[2] == 0xBF);

                        using var reader = new StreamReader(inputStream);
                        using var writer = new StreamWriter(outputStream, new UTF8Encoding(hasUtf8ByteOrderMark));

                        // First find the char position where our line starts.

                        // Note that we skip over lines manually (without using reader.ReadLine) since the Roslyn API below expects
                        // absolute character positions; because StreamReader buffers internally, we can't know the precise character offset
                        // in the file etc.
                        var pos = 0;
                        for (var i = 0; i < lineNumber - 1; i++)
                        {
                            while (true)
                            {
                                if (reader.Peek() == -1)
                                {
                                    return;
                                }

                                pos++;
                                var ch = (char)reader.Read();
                                writer.Write(ch);
                                if (ch == '\n') // Unix
                                {
                                    break;
                                }

                                if (ch == '\r')
                                {
                                    // Mac (just \r) or Windows (\r\n)
                                    if (reader.Peek() >= 0 && (char)reader.Peek() == '\n')
                                    {
                                        _ = reader.Read();
                                        writer.Write('\n');
                                        pos++;
                                    }

                                    break;
                                }
                            }
                        }

                        // We have the character position of the line start. Skip over whitespace (that's the indent) to find the invocation
                        var indentBuilder = new StringBuilder();
                        while (true)
                        {
                            var i = reader.Peek();
                            if (i == -1)
                            {
                                return;
                            }

                            var ch = (char)i;

                            if (ch == ' ')
                            {
                                pos++;
                                indentBuilder.Append(' ');
                                reader.Read();
                                writer.Write(ch);
                            }
                            else
                            {
                                break;
                            }
                        }

                        // We are now at the start of the invocation.
                        var node = syntaxTree.GetRoot().FindNode(TextSpan.FromBounds(pos, pos));

                        // Node should be pointing at the AssertSql identifier. Go up and find the text span for the entire method invocation.
                        if (node is not IdentifierNameSyntax { Parent: InvocationExpressionSyntax invocation })
                        {
                            return;
                        }

                        // Skip over the invocation on the read side, and write the new baseline invocation
                        var tempBuf = new char[Math.Max(1024, invocation.Span.Length)];
                        reader.ReadBlock(tempBuf, 0, invocation.Span.Length);
                        var numNewlinesInOrigin = tempBuf.Count(c => c is '\n' or '\r');

                        indentBuilder.Append("    ");
                        var indent = indentBuilder.ToString();
                        var newBaseLine = $@"Assert{(forUpdate ? "ExecuteUpdate" : "")}Sql(
{string.Join("," + Environment.NewLine + indent + "//" + Environment.NewLine, SqlStatements.Skip(offset).Take(count).Select(sql => indent + "\"\"\"" + Environment.NewLine + sql + Environment.NewLine + "\"\"\""))})";
                        var numNewlinesInRewritten = newBaseLine.Count(c => c is '\n' or '\r');

                        writer.Write(newBaseLine);

                        // If we've added or removed any lines, record that in the line displacements data structure for later rewritings
                        // in the same file
                        var lineDiff = numNewlinesInRewritten - numNewlinesInOrigin;
                        if (lineDiff != 0)
                        {
                            fileInfo.LineDisplacements[origLineNumber] = lineDiff;
                        }

                        // Copy the rest of the file contents as-is
                        int c;
                        while ((c = reader.ReadBlock(tempBuf, 0, 1024)) > 0)
                        {
                            writer.Write(tempBuf, 0, c);
                        }
                    }
                }
                catch
                {
                    File.Delete(fileName + ".tmp");
                    throw;
                }

                File.Move(fileName + ".tmp", fileName, overwrite: true);
            }
        }
    }

    protected class TestSqlLogger(bool shouldLogCommands) : ListLogger
    {
        private readonly bool _shouldLogCommands = shouldLogCommands;

        public List<string> SqlStatements { get; } = [];
        public List<string> Parameters { get; } = [];

        private readonly StringBuilder _stringBuilder = new();

        protected override void UnsafeClear()
        {
            base.UnsafeClear();

            SqlStatements.Clear();
            Parameters.Clear();
        }

        protected override void UnsafeLog<TState>(
            LogLevel logLevel,
            EventId eventId,
            string? message,
            TState state,
            Exception? exception)
        {
            if ((eventId.Id == RelationalEventId.CommandExecuted.Id
                    || eventId.Id == RelationalEventId.CommandError.Id
                    || eventId.Id == RelationalEventId.CommandExecuting.Id))
            {
                if (_shouldLogCommands)
                {
                    base.UnsafeLog(logLevel, eventId, message, state, exception);
                }

                if (!IsRecordingSuspended
                    && message != null
                    && eventId.Id != RelationalEventId.CommandExecuting.Id)
                {
                    var structure = (IReadOnlyList<KeyValuePair<string, object>>)state!;

                    var parameters = structure.Where(i => i.Key == "parameters").Select(i => (string)i.Value).First();
                    var commandText = structure.Where(i => i.Key == "commandText").Select(i => (string)i.Value).First();

                    if (!string.IsNullOrWhiteSpace(parameters))
                    {
                        Parameters.Add(parameters);

                        _stringBuilder.Clear();

                        var inQuotes = false;
                        var inCurlies = false;
                        for (var i = 0; i < parameters.Length; i++)
                        {
                            var c = parameters[i];
                            switch (c)
                            {
                                case '\'':
                                    inQuotes = !inQuotes;
                                    goto default;
                                case '{':
                                    inCurlies = true;
                                    goto default;
                                case '}':
                                    inCurlies = false;
                                    goto default;
                                case ',' when parameters[i + 1] == ' ' && !inQuotes && !inCurlies:
                                    _stringBuilder.Append(_eol);
                                    i++;
                                    continue;
                                default:
                                    _stringBuilder.Append(c);
                                    continue;
                            }
                        }

                        _stringBuilder.Append(_eol).Append(_eol);
                        parameters = _stringBuilder.ToString();
                    }

                    SqlStatements.Add(parameters + commandText);
                }
            }
            else
            {
                base.UnsafeLog(logLevel, eventId, message, state, exception);
            }
        }
    }

    private struct QueryBaselineRewritingFileInfo
    {
        public QueryBaselineRewritingFileInfo() { }

        public object Lock { get; } = new();

        /// <summary>
        ///     Contains information on where previous baseline rewriting caused line numbers to shift; this is used in adjusting line
        ///     numbers for later errors. The keys are (pre-rewriting) line numbers, and the values are offsets that have been applied to
        ///     them.
        /// </summary>
        public readonly SortedDictionary<int, int> LineDisplacements = new();
    }
}
