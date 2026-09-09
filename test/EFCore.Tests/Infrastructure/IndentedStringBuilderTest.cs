// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// ReSharper disable InconsistentNaming

namespace Microsoft.EntityFrameworkCore.Infrastructure;

public class IndentedStringBuilderTest
{
    private static readonly string EOL = Environment.NewLine;

    [ConditionalFact]
    public void Append_at_start_with_indent()
    {
        var indentedStringBuilder = new IndentedStringBuilder();

        using (indentedStringBuilder.Indent())
        {
            indentedStringBuilder.Append("Foo");
        }

        Assert.Equal("    Foo", indentedStringBuilder.ToString());
    }

    [ConditionalFact]
    public void Append_in_middle_when_no_new_line()
    {
        var indentedStringBuilder = new IndentedStringBuilder();

        indentedStringBuilder.Append("Foo");

        using (indentedStringBuilder.Indent())
        {
            indentedStringBuilder.Append("Foo");
        }

        Assert.Equal("FooFoo", indentedStringBuilder.ToString());
    }

    [ConditionalFact]
    public void Append_in_middle_when_new_line()
    {
        var indentedStringBuilder = new IndentedStringBuilder();

        indentedStringBuilder.AppendLine("Foo");

        using (indentedStringBuilder.Indent())
        {
            indentedStringBuilder.Append("Foo");
            indentedStringBuilder.AppendLine();
        }

        Assert.Equal($"Foo{EOL}    Foo{EOL}", indentedStringBuilder.ToString());
    }

    [ConditionalFact]
    public void Append_value_containing_end_of_line_no_indent()
    {
        var indentedStringBuilder = new IndentedStringBuilder();

        indentedStringBuilder.Append($"Foo{EOL}Bar");

        Assert.Equal($"Foo{EOL}Bar", indentedStringBuilder.ToString());
    }

    [ConditionalFact]
    public void Append_value_containing_end_of_line_with_indent()
    {
        var indentedStringBuilder = new IndentedStringBuilder();

        using (indentedStringBuilder.Indent())
        {
            indentedStringBuilder.Append($"Foo{EOL}Bar");
        }

        // Note: EOL does not cause indent on "Bar"
        Assert.Equal($"    Foo{EOL}Bar", indentedStringBuilder.ToString());
    }

    [ConditionalFact]
    public void Append_in_middle_value_containing_end_of_line_with_indent()
    {
        var indentedStringBuilder = new IndentedStringBuilder();

        indentedStringBuilder.Append("xyz");
        using (indentedStringBuilder.Indent())
        {
            indentedStringBuilder.Append($"Foo{EOL}Bar");
        }

        // Note: EOL does not cause indent on "Bar"
        Assert.Equal($"xyzFoo{EOL}Bar", indentedStringBuilder.ToString());
    }

    [ConditionalFact]
    public void Append_line_at_start_with_indent()
    {
        var indentedStringBuilder = new IndentedStringBuilder();

        using (indentedStringBuilder.Indent())
        {
            indentedStringBuilder.AppendLine("Foo");
        }

        Assert.Equal("    Foo" + EOL, indentedStringBuilder.ToString());
    }

    [ConditionalFact]
    public void Append_line_in_middle_when_no_new_line()
    {
        var indentedStringBuilder = new IndentedStringBuilder();

        indentedStringBuilder.AppendLine("Foo");

        using (indentedStringBuilder.Indent())
        {
            indentedStringBuilder.AppendLine("Foo");
        }

        Assert.Equal($"Foo{EOL}    Foo{EOL}", indentedStringBuilder.ToString());
    }

    [ConditionalFact]
    public void Append_line_with_indent_only()
    {
        var indentedStringBuilder = new IndentedStringBuilder();

        using (indentedStringBuilder.Indent())
        {
            indentedStringBuilder.AppendLine();
        }

        Assert.Equal(Environment.NewLine, indentedStringBuilder.ToString());
    }

    [ConditionalFact]
    public void Append_line_value_containing_end_of_line_no_indent()
    {
        var indentedStringBuilder = new IndentedStringBuilder();

        indentedStringBuilder.AppendLine($"Foo{EOL}Bar");

        Assert.Equal($"Foo{EOL}Bar{EOL}", indentedStringBuilder.ToString());
    }

    [ConditionalFact]
    public void Append_line_value_containing_end_of_line_with_indent()
    {
        var indentedStringBuilder = new IndentedStringBuilder();

        using (indentedStringBuilder.Indent())
        {
            indentedStringBuilder.AppendLine($"Foo{EOL}Bar");
        }

        // Note: EOL does not cause indent on "Bar"
        Assert.Equal($"    Foo{EOL}Bar{EOL}", indentedStringBuilder.ToString());
    }

    [ConditionalFact]
    public void Append_line_in_middle_value_containing_end_of_line_with_indent_when_no_new_line()
    {
        var indentedStringBuilder = new IndentedStringBuilder();

        indentedStringBuilder.Append("xyz");
        using (indentedStringBuilder.Indent())
        {
            indentedStringBuilder.AppendLine($"Foo{EOL}Bar");
        }

        // Note: EOL does not cause indent on "Bar"
        Assert.Equal($"xyzFoo{EOL}Bar{EOL}", indentedStringBuilder.ToString());
    }

    [ConditionalFact]
    public void Append_line_in_middle_value_containing_end_of_line_with_indent_when_new_line()
    {
        var indentedStringBuilder = new IndentedStringBuilder();

        indentedStringBuilder.AppendLine("xyz");
        using (indentedStringBuilder.Indent())
        {
            indentedStringBuilder.AppendLine($"Foo{EOL}Bar");
        }

        // Note: EOL does not cause indent on "Bar"
        Assert.Equal($"xyz{EOL}    Foo{EOL}Bar{EOL}", indentedStringBuilder.ToString());
    }

    [ConditionalFact]
    public void Append_lines_at_start_with_indent()
    {
        var indentedStringBuilder = new IndentedStringBuilder();

        using (indentedStringBuilder.Indent())
        {
            indentedStringBuilder.AppendLines("Foo");
        }

        Assert.Equal("    Foo" + EOL, indentedStringBuilder.ToString());
    }

    [ConditionalFact]
    public void Append_lines_no_indent()
    {
        var indentedStringBuilder = new IndentedStringBuilder();

        indentedStringBuilder.AppendLines($"Foo{EOL}Bar");

        Assert.Equal($"Foo{EOL}Bar{EOL}", indentedStringBuilder.ToString());
    }

    [ConditionalFact]
    public void Append_lines_with_indent()
    {
        var indentedStringBuilder = new IndentedStringBuilder();

        using (indentedStringBuilder.Indent())
        {
            indentedStringBuilder.AppendLines($"Foo{EOL}Bar");
        }

        // Note: EOL _does_ cause indent on "Bar"
        Assert.Equal($"    Foo{EOL}    Bar{EOL}", indentedStringBuilder.ToString());
    }

    [ConditionalFact]
    public void Append_lines_with_indent_with_skip_final_new_line()
    {
        var indentedStringBuilder = new IndentedStringBuilder();

        using (indentedStringBuilder.Indent())
        {
            indentedStringBuilder.AppendLines($"Foo{EOL}Bar", skipFinalNewline: true);
        }

        // Note: EOL _does_ cause indent on "Bar", plus final EOL is skipped
        Assert.Equal($"    Foo{EOL}    Bar", indentedStringBuilder.ToString());
    }

    [ConditionalFact]
    public void Append_lines_in_middle_with_indent_when_no_new_line()
    {
        var indentedStringBuilder = new IndentedStringBuilder();

        indentedStringBuilder.Append("xyz");
        using (indentedStringBuilder.Indent())
        {
            indentedStringBuilder.AppendLines($"Foo{EOL}Bar");
        }

        // Note: EOL _does_ cause indent on "Bar"
        Assert.Equal($"xyzFoo{EOL}    Bar{EOL}", indentedStringBuilder.ToString());
    }

    [ConditionalFact]
    public void Append_lines_in_middle_with_indent_when_new_line()
    {
        var indentedStringBuilder = new IndentedStringBuilder();

        indentedStringBuilder.AppendLine("xyz");
        using (indentedStringBuilder.Indent())
        {
            indentedStringBuilder.AppendLines($"Foo{EOL}Bar");
        }

        // Note: EOL _does_ cause indent on "Bar"
        Assert.Equal($"xyz{EOL}    Foo{EOL}    Bar{EOL}", indentedStringBuilder.ToString());
    }
}
