// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;

namespace Microsoft.Data.Entity.Relational.Design.ReverseEngineering
{
    /// <summary>
    ///     Abstraction for outputting a 'file'. Normally this will be outputting a file to disk
    ///     but this allows for other implementations which e.g. just output to memory.
    /// </summary>
    public interface IFileService
    {
        bool DirectoryExists([NotNull] string directoryName);

        /// <summary>
        ///     Checks whether 'file' is read-only. Absence of the 'file' is interpreted as readable.
        /// </summary>
        bool IsFileReadOnly([NotNull] string directoryName, [NotNull] string fileName);

        /// <summary>
        ///     Creates, if necessary, a 'file' located within the given directory and with the given name.
        ///     Ensures that the contents of the 'file' contain the given contents (overwriting if necessary).
        /// </summary>
        /// <returns>the full path of the output 'file'</returns>
        string OutputFile([NotNull] string directoryName, [NotNull] string fileName, [NotNull] string contents);

        string RetrieveFileContents([NotNull] string directoryName, [NotNull] string fileName);
    }
}
