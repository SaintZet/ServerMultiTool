using log4net;
using ServerMultiTool.Model.Infrastructure.Interfaces;
using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace ServerMultiTool.Model.Infrastructure;

public abstract class JsonRepository<T> : IRepository<T> where T : class
{
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

            // Create a temporary file path
            var tempFilePath = Path.GetTempFileName();

            // Write to temporary file first
            var json = JsonSerializer.Serialize(entity, WriteOptions);
            File.WriteAllText(tempFilePath, json);

            // Atomically replace the original file
            File.Copy(tempFilePath, FilePath, true);
            File.Delete(tempFilePath);

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
            // Get the current JSON document
            JsonNode jsonDocument;
            if (File.Exists(FilePath))
            {
                var json = File.ReadAllText(FilePath);
                jsonDocument = JsonNode.Parse(json)
                    ?? throw new InvalidOperationException("Failed to parse JSON document");
            }
            else
            {
                // If file doesn't exist, create a new JSON document
                jsonDocument = new JsonObject();
            }

            // Update the field using the provided path (e.g., "Property1.NestedProperty")
            UpdateJsonNodeField(jsonDocument, fieldPath.Split('.'), value);

            // Create a temporary file path
            var tempFilePath = Path.GetTempFileName();

            // Write to temporary file first
            File.WriteAllText(tempFilePath, jsonDocument.ToJsonString(WriteOptions));

            // Atomically replace the original file
            File.Copy(tempFilePath, FilePath, true);
            File.Delete(tempFilePath);

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

            // Create a temporary file path
            var tempFilePath = Path.GetTempFileName();

            // Write to temporary file first
            var json = JsonSerializer.Serialize(entity, WriteOptions);
            await File.WriteAllTextAsync(tempFilePath, json);

            // Atomically replace the original file
            File.Copy(tempFilePath, FilePath, true);
            File.Delete(tempFilePath);

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
            // Get the current JSON document
            JsonNode jsonDocument;
            if (File.Exists(FilePath))
            {
                var json = await File.ReadAllTextAsync(FilePath);
                jsonDocument = JsonNode.Parse(json)
                    ?? throw new InvalidOperationException("Failed to parse JSON document");
            }
            else
            {
                // If file doesn't exist, create a new JSON document
                jsonDocument = new JsonObject();
            }

            // Update the field using the provided path (e.g., "Property1.NestedProperty")
            UpdateJsonNodeField(jsonDocument, fieldPath.Split('.'), value);

            // Create a temporary file path
            var tempFilePath = Path.GetTempFileName();

            // Write to temporary file first
            await File.WriteAllTextAsync(tempFilePath, jsonDocument.ToJsonString(WriteOptions));

            // Atomically replace the original file
            File.Copy(tempFilePath, FilePath, true);
            File.Delete(tempFilePath);

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