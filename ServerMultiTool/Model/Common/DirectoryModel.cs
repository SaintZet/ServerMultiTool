using System;

namespace ServerMultiTool.Model.Common;

[Serializable]
public struct DirectoryModel
{
    public string Name { get; set; }
    public string Path { get; set; }
}