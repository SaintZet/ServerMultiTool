using log4net;
using ServerMultiTool.Model.Infrastructure.Interfaces;
using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace ServerMultiTool.Model.Infrastructure.Json;

public abstract class JsonRepository<T> : IRepository<T> where T : class
{
    private static readonly object FileLock = new();

    protected readonly ILog Log;
    protected readonly string FilePath;
    protected readonly JsonSerializerOptions ReadOptions;
    protected readonly JsonSerializerOptions WriteOptions;

    protected JsonRepository(string filePath, ILog log)
    {
        FilePath = filePath ?? throw new ArgumentNullException(nameof(filePath));
        Log = log ?? throw new ArgumentNullException(nameof(log));

        ReadOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            IncludeFields = true
        };

        WriteOptions = new JsonSerializerOptions
        {
            WriteIndented = true
        };

        // Ensure directory exists
        var directoryPath = Path.GetDirectoryName(FilePath);
        if (!string.IsNullOrEmpty(directoryPath) && !Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }
    }

    /// <summary>
    /// Gets the entire entity
    /// </summary>
    public virtual T Get()
    {
        try
        {
            if (!File.Exists(FilePath))
                throw new FileNotFoundException("File not found", FilePath);

            var json = File.ReadAllText(FilePath);
            return JsonSerializer.Deserialize<T>(json, ReadOptions)
                ?? throw new InvalidOperationException("Failed to deserialize entity");
        }
        catch (Exception ex)
        {
            Log.Error($"Failed to get entity from {FilePath}: {ex.Message}", ex);
            throw;
        }
    }

    /// <summary>
    /// Updates the entire entity
    /// </summary>
    public virtual void Update(T entity)
    {
        try
        {
            ArgumentNullException.ThrowIfNull(entity);

            var tempFilePath = Path.GetTempFileName();
            var json = JsonSerializer.Serialize(entity, WriteOptions);
            File.WriteAllText(tempFilePath, json);

            lock (FileLock)
            {
                File.Copy(tempFilePath, FilePath, true);
                File.Delete(tempFilePath);
            }

            Log.Info($"Entity successfully updated in {FilePath}");
        }
        catch (Exception ex)
        {
            Log.Error($"Failed to update entity in {FilePath}: {ex.Message}", ex);
            throw;
        }
    }

    /// <summary>
    /// Updates a specific field of the entity
    /// </summary>
    public virtual void UpdateField<TValue>(string fieldPath, TValue value)
    {
        try
        {
            JsonNode jsonDocument;
            if (File.Exists(FilePath))
            {
                var json = File.ReadAllText(FilePath);
                jsonDocument = JsonNode.Parse(json)
                    ?? throw new InvalidOperationException("Failed to parse JSON document");
            }
            else
            {
                jsonDocument = new JsonObject();
            }

            UpdateJsonNodeField(jsonDocument, fieldPath.Split('.'), value);

            var tempFilePath = Path.GetTempFileName();
            File.WriteAllText(tempFilePath, jsonDocument.ToJsonString(WriteOptions));

            lock (FileLock)
            {
                File.Copy(tempFilePath, FilePath, true);
                File.Delete(tempFilePath);
            }

            Log.Info($"Field '{fieldPath}' successfully updated in {FilePath}");
        }
        catch (Exception ex)
        {
            Log.Error($"Failed to update field '{fieldPath}' in {FilePath}: {ex.Message}", ex);
            throw;
        }
    }

    /// <summary>
    /// Asynchronously gets the entire entity
    /// </summary>
    public virtual async Task<T> GetAsync()
    {
        try
        {
            if (!File.Exists(FilePath))
                throw new FileNotFoundException("File not found", FilePath);

            var json = await File.ReadAllTextAsync(FilePath);
            return JsonSerializer.Deserialize<T>(json, ReadOptions)
                ?? throw new InvalidOperationException("Failed to deserialize entity");
        }
        catch (Exception ex)
        {
            Log.Error($"Failed to get entity from {FilePath}: {ex.Message}", ex);
            throw;
        }
    }

    /// <summary>
    /// Asynchronously updates the entire entity
    /// </summary>
    public virtual async Task UpdateAsync(T entity)
    {
        try
        {
            ArgumentNullException.ThrowIfNull(entity);

            var tempFilePath = Path.GetTempFileName();
            var json = JsonSerializer.Serialize(entity, WriteOptions);
            await File.WriteAllTextAsync(tempFilePath, json);

            lock (FileLock)
            {
                File.Copy(tempFilePath, FilePath, true);
                File.Delete(tempFilePath);
            }

            Log.Info($"Entity successfully updated in {FilePath}");
        }
        catch (Exception ex)
        {
            Log.Error($"Failed to update entity in {FilePath}: {ex.Message}", ex);
            throw;
        }
    }

    /// <summary>
    /// Asynchronously updates a specific field of the entity
    /// </summary>
    public virtual async Task UpdateFieldAsync<TValue>(string fieldPath, TValue value)
    {
        try
        {
            JsonNode jsonDocument;
            if (File.Exists(FilePath))
            {
                var json = await File.ReadAllTextAsync(FilePath);
                jsonDocument = JsonNode.Parse(json)
                    ?? throw new InvalidOperationException("Failed to parse JSON document");
            }
            else
            {
                jsonDocument = new JsonObject();
            }

            UpdateJsonNodeField(jsonDocument, fieldPath.Split('.'), value);

            var tempFilePath = Path.GetTempFileName();
            await File.WriteAllTextAsync(tempFilePath, jsonDocument.ToJsonString(WriteOptions));

            lock (FileLock)
            {
                File.Copy(tempFilePath, FilePath, true);
                File.Delete(tempFilePath);
            }

            Log.Info($"Field '{fieldPath}' successfully updated in {FilePath}");
        }
        catch (Exception ex)
        {
            Log.Error($"Failed to update field '{fieldPath}' in {FilePath}: {ex.Message}", ex);
            throw;
        }
    }

    /// <summary>
    /// Updates a field in a JSON node
    /// </summary>
    private void UpdateJsonNodeField<TValue>(JsonNode node, string[] pathParts, TValue value)
    {
        if (pathParts.Length == 0)
            return;

        if (pathParts.Length == 1)
        {
            // We're at the final level, set the value
            if (node is JsonObject finalJsonObject)
            {
                finalJsonObject[pathParts[0]] = ConvertToJsonNode(value);
            }
            return;
        }

        // We need to navigate deeper
        var currentPart = pathParts[0];
        var remainingPath = new string[pathParts.Length - 1];
        Array.Copy(pathParts, 1, remainingPath, 0, remainingPath.Length);

        // If current node is an object
        if (node is JsonObject nestedJsonObject)
        {
            // If property doesn't exist or is null, create it
            if (!nestedJsonObject.ContainsKey(currentPart) || nestedJsonObject[currentPart] == null)
            {
                nestedJsonObject[currentPart] = new JsonObject();
            }

            // Navigate to the next level
            UpdateJsonNodeField(nestedJsonObject[currentPart], remainingPath, value);
        }
    }

    /// <summary>
    /// Converts a .NET value to a JsonNode
    /// </summary>
    private JsonNode ConvertToJsonNode<TValue>(TValue value)
    {
        if (value == null)
            return JsonValue.Create<object?>(null);

        // Use JsonSerializer to properly convert the value
        var json = JsonSerializer.Serialize(value);
        return JsonNode.Parse(json) ?? throw new InvalidOperationException("Failed to convert value to JsonNode");
    }
}