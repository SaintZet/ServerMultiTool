using System;

namespace ServerMultiTool.Model.Settings;

[Serializable]
public struct DirectoryModel
{
    public long Id { get; set; }
    public string Name { get; set; }
    public string Path { get; set; }
}