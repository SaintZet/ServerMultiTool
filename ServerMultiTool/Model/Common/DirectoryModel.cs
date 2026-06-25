using System;

namespace ServerMultiTool.Model.Common;

[Serializable]
public class DirectoryModel
{
    private string _path = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string Path
    {
        get => _path;
        set
        {
            ValidatePath(value);
            _path = NormalizePath(value);
        }
    }

    public DirectoryModel()
    {
    }

    public DirectoryModel(string path)
    {
        Path = path;
    }

    private static void ValidatePath(string path)
    {
        if (path.Contains(".."))
            throw new ArgumentException("Directory path cannot contain relative paths (..) for security reasons.");
    }

    private static string NormalizePath(string path)
    {
        if (string.IsNullOrEmpty(path))
            return path;

        if (path.EndsWith("/"))
            path = path.TrimEnd('/');

        if (path.EndsWith("\\"))
            path = path.TrimEnd('\\');

        return path;
    }

    public bool IsSameDirectory(DirectoryModel directory) =>
        string.Equals(Path, directory.Path, StringComparison.OrdinalIgnoreCase);

    public bool IsSameDirectory(string path) =>
        string.Equals(Path, path, StringComparison.OrdinalIgnoreCase);

    public bool IsDifferentDirectory(DirectoryModel directory) =>
        string.Equals(Path, directory.Path, StringComparison.OrdinalIgnoreCase) is false;
}
