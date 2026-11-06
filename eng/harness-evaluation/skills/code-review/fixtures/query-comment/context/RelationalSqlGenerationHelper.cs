using System.IO;
using System.Text;

namespace Microsoft.EntityFrameworkCore.Storage;

public interface ISqlGenerationHelper
{
    string GenerateComment(string text);
}

public class RelationalSqlGenerationHelper : ISqlGenerationHelper
{
    public virtual string SingleLineCommentToken => "--";

    public virtual string GenerateComment(string text)
    {
        var builder = new StringBuilder();
        using (var reader = new StringReader(text))
        {
            string? line;
            while ((line = reader.ReadLine()) != null)
            {
                builder.Append(SingleLineCommentToken).Append(' ').AppendLine(line);
            }
        }

        return builder.ToString();
    }
}