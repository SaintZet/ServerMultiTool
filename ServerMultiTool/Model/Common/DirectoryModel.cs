using System;

namespace ServerMultiTool.Model.Common;

[Serializable]
public class DirectoryModel
{
    public string Name { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;

    //if (string.IsNullOrEmpty(directory.Path))
    //    throw new ArgumentException("Http directory path cannot be null or empty.");

    //if (directory.Path.Equals(SolutionDirectory, StringComparison.OrdinalIgnoreCase))
    //    throw new ArgumentException("Http directory path cannot be the same as the solution directory path.");

    //if (directory.Path.Contains(" "))
    //    throw new ArgumentException("Http directory path cannot contain spaces.");

    //if (directory.Path.Contains(".."))
    //    throw new ArgumentException("Http directory path cannot contain relative paths (..) for security reasons.");

    //if (directory.Path.EndsWith("/"))
    //    directory.Path = directory.Path.TrimEnd('/');

    //if (directory.Path.EndsWith("\\"))
    //    directory.Path = directory.Path.TrimEnd('\\');

    public bool IsSameDirectory(DirectoryModel directory) =>
        string.Equals(Path, directory.Path, StringComparison.OrdinalIgnoreCase);

    public bool IsSameDirectory(string path) =>
        string.Equals(Path, path, StringComparison.OrdinalIgnoreCase);

    public bool IsDifferentDirectory(DirectoryModel directory) =>
        string.Equals(Path, directory.Path, StringComparison.OrdinalIgnoreCase) is false;
}