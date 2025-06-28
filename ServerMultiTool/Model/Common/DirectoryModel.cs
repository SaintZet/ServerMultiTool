using System;

namespace ServerMultiTool.Model.Common;

[Serializable]
public record struct DirectoryModel
{
    public string Name { get; set; }
    public string Path { get; set; }
    
    public bool IsSameDirectory(DirectoryModel directory) => 
        string.Equals(Path, directory.Path, StringComparison.OrdinalIgnoreCase);
    
    public bool IsDifferentDirectory(DirectoryModel directory) => 
        string.Equals(Path, directory.Path, StringComparison.OrdinalIgnoreCase) is false;
}