// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Microsoft.EntityFrameworkCore.XuGu.Query.Internal
{
    public class XGCommandParser
    {
        public virtual string SqlFragment { get; }
        public virtual char[] States { get; }

        public XGCommandParser(string sqlFragment)
        {
            SqlFragment = sqlFragment ?? throw new ArgumentNullException(nameof(sqlFragment));
            States = new char[sqlFragment.Length];

            Parse();
        }

        public virtual IReadOnlyList<int> GetStateIndices(char state, int start = 0, int length = -1)
        {
            if (start < 0 ||
                start >= States.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(start));
            }

            if (length < 0 ||
                length > 0 && start + length > States.Length)
            {
                length = States.Length - start;
            }

            var stateIndices = new List<int>();
            char? lastState = null;

            for (var i = start; i < length; i++)
            {
                var currentState = States[i];

                if (currentState == state &&
                    currentState != lastState)
                {
                    stateIndices.Add(i);
                }

                lastState = currentState;
            }

            return stateIndices.AsReadOnly();
        }

        public virtual IReadOnlyList<int> GetStateIndices(char[] states, int start = 0, int length = -1)
        {
            if (states == null)
            {
                throw new ArgumentNullException(nameof(states));
            }

            if (states.Length <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(states));
            }

            if (start < 0 ||
                start >= States.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(start));
            }

            if (length < 0 ||
                length > 0 && start + length > States.Length)
            {
                length = States.Length - start;
            }

            var stateIndices = new List<int>();
            char? lastState = null;

            for (var i = start; i < length; i++)
            {
                var currentState = States[i];

                if (currentState != lastState &&
                    states.Contains(currentState))
                {
                    stateIndices.Add(i);
                }

                lastState = currentState;
            }

            return stateIndices.AsReadOnly();
        }

        protected virtual void Parse()
        {
            // We use '\0' as the default state and char.
            var state = '\0';
            var lastChar = '\0';
            var secondTolastChar = '\0';

            // State machine to parse MySQL SQL.
            for (var i = 0; i < SqlFragment.Length; i++)
            {
                var c = SqlFragment[i];
                var skipProcessing = false;

                if (state == '\'')
                {
                    // We are currently inside a string, or closed the string in the last iteration but didn't
                    // know that at the time, because it still could have been the beginning of an escape sequence.

                    if (c == '\'')
                    {
                        // We either end the string, begin an escape sequence or end an escape sequence.
                        if (lastChar == '\'')
                        {
                            // This is the end of an escape sequence.
                            // We continue being in a string.
                            lastChar = '\0';
                        }
                        else
                        {
                            // This is either the beginning of an escape sequence, or the end of the string.
                            // We will know in the next iteration.
                            lastChar = '\'';
                        }
                    }
                    else if (lastChar == '\'')
                    {
                        // The last iteration was the end of a string.
                        // Reset the current state and continue processing the current char.
                        state = '\0';
                        lastChar = '\0';
                        States[i - 1] = state;
                    }
                }

                if (state == '"')
                {
                    // We are currently inside a string, or closed the string in the last iteration but didn't
                    // know that at the time, because it still could have been the beginning of an escape sequence.

                    if (c == '"')
                    {
                        // We either end the string, begin an escape sequence or end an escape sequence.
                        if (lastChar == '"')
                        {
                            // This is the end of an escape sequence.
                            // We continue being in a string.
                            lastChar = '\0';
                        }
                        else
                        {
                            // This is either the beginning of an escape sequence, or the end of the string.
                            // We will know the in the next iteration.
                            lastChar = '"';
                        }
                    }
                    else if (lastChar == '"')
                    {
                        // The last iteration was the end of a string.
                        // Reset the current state and continue processing the current char.
                        state = '\0';
                        lastChar = '\0';
                        States[i - 1] = state;
                    }
                }

                if (state == '`')
                {
                    // We are currently inside an identifier, or closed the identifier in the last iteration but didn't
                    // know that at the time, because it still could have been the beginning of an escape sequence.

                    if (c == '`')
                    {
                        // We either end the identifier, begin an escape sequence or end an escape sequence.
                        if (lastChar == '`')
                        {
                            // This is the end of an escape sequence.
                            // We continue being in an identifier.
                            lastChar = '\0';
                        }
                        else
                        {
                            // This is either the beginning of an escape sequence, or the end of the identifier.
                            // We will know the in the next iteration.
                            lastChar = '`';
                        }
                    }
                    else if (lastChar == '`')
                    {
                        // The last iteration was the end of an identifier.
                        // Reset the current state and continue processing the current char.
                        state = '\0';
                        lastChar = '\0';
                        States[i - 1] = state;
                    }
                }

                if (state == '/')
                {
                    // We could be at the beginning or end of a comment, or this is just a slash.
                    if (lastChar == '/')
                    {
                        // This is the beginning of a comment.
                        Debug.Assert(c == '*');
                        lastChar = '\0';
                    }
                    else if (lastChar == '*')
                    {
                        if (c == '/')
                        {
                            // This is the end of a comment.
                            lastChar = '\0';
                            state = '\0';
                            skipProcessing = true;
                        }
                        else
                        {
                            // This was just an ordanary asterisk inside of a comment character all along.
                            lastChar = '\0';
                        }
                    }
                    else if (c == '*')
                    {
                        // This could be the beginning of the end of a comment, or it could just be an ordenary asterisk inside of a
                        // comment.
                        Debug.Assert(lastChar == '\0');
                        lastChar = '*';
                    }
                    else
                    {
                        // This is just an ordenary character, either insider or outside of a comment.
                        lastChar = '\0';
                    }
                }

                if (state == '-')
                {
                    if (lastChar == '-')
                    {
                        if (c == '-' &&
                            secondTolastChar == '\0')
                        {
                            // This could still be the beginning of a line comment.
                            // In MySQL, a line comment starts with two dashes and a whitespace.
                            secondTolastChar = '-';
                        }
                        else if (secondTolastChar == '-' &&
                                 (c == ' ' || c == '\t'))
                        {
                            // A line comment has been started.
                            lastChar = '\0';
                            secondTolastChar = '\0';
                        }
                        else
                        {
                            // The previous character(s) was/were just a dash and not the beginning of a line comment.
                            state = '\0';
                            lastChar = '\0';
                            States[i - 1] = state;

                            if (secondTolastChar == '-')
                            {
                                secondTolastChar = '\0';
                                States[i - 2] = state;
                            }
                        }
                    }
                    else
                    {
                        // We are in an established line comment.
                        Debug.Assert(lastChar == '\0');
                        Debug.Assert(secondTolastChar == '\0');

                        if (c == '\r' ||
                            c == '\n')
                        {
                            // The line comment ends here.
                            state = '\0';
                            skipProcessing = true;
                        }
                    }
                }

                if (state == '#')
                {
                    // We are inside of a line comment.
                    if (c == '\r' ||
                        c == '\n')
                    {
                        // The line comment ends here.
                        state = '\0';
                        skipProcessing = true;
                    }
                }

                if (state == '\0' &&
                    !skipProcessing)
                {
                    if (c == '"')
                    {
                        state = '"';
                    }
                    else if (c == '\'')
                    {
                        state = '\'';
                    }
                    else if (c == '`')
                    {
                        state = '`';
                    }
                    else if (c == '/')
                    {
                        state = '/';
                        lastChar = '/';
                    }
                    else if (c == '-')
                    {
                        state = '-';
                        lastChar = '-';
                    }
                    else if (c == '#')
                    {
                        state = '#';
                    }
                    else if (c == '@')
                    {
                        // This is either the beginning of a named parameter, or a global variable like @@version.
                        state = '@';
                    }
                    else if (c == ';')
                    {
                        States[i] = ';';
                    }
                }
                else if (state == '@')
                {
                    if (c == '@')
                    {
                        // This has not been a named parameter, but a global variable.
                        // We use '$' to signal a global variable like @@version.
                        States[i - 1] = '$';
                    }

                    state = '\0';
                }

                if (state != '\0')
                {
                    States[i] = state;
                }
            }

            //
            // Handle still pending states:
            //

            if (state == '\'' && lastChar == '\'' ||
                state == '"' && lastChar == '"' ||
                state == '`' && lastChar == '`' ||
                state == '/' && lastChar == '/' ||
                state == '-' && lastChar == '-')
            {
                // The last iteration was the end of a string or identifier,
                // or not the start of a comment.
                state = '\0';
                lastChar = '\0';
                States[States.Length - 1] = state;
            }
        }
    }
}
