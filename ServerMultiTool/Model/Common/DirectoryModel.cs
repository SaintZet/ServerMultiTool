using System;

namespace ServerMultiTool.Model.Common;

[Serializable]
public class DirectoryModel
{
    public string Name { get; set; } = string.Empty;
    public string Path { get; set; }= string.Empty;
    
    public bool IsSameDirectory(DirectoryModel directory) => 
        string.Equals(Path, directory.Path, StringComparison.OrdinalIgnoreCase);
    
    public bool IsDifferentDirectory(DirectoryModel directory) => 
        string.Equals(Path, directory.Path, StringComparison.OrdinalIgnoreCase) is false;
}