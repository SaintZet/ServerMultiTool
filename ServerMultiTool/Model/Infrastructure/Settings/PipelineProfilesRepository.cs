using log4net;
using Microsoft.IdentityModel.Tokens;
using ServerMultiTool.Model.Domain.Pipeline;
using ServerMultiTool.Model.Domain.Pipeline.Interfaces;
using ServerMultiTool.Model.Infrastructure.DefaultValues;
using ServerMultiTool.Model.Infrastructure.Json;
using ServerMultiTool.Model.Services.Settings;
using ServerMultiTool.ViewModels.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;

namespace ServerMultiTool.Model.Infrastructure.Settings;

/// <summary>
/// Repository implementation for PipelineProfiles
/// </summary>
public class PipelineProfilesRepository : IPipelineProfilesRepository
{
    private readonly ILog _log;
    private readonly string _profilesDirectoryPath;

    private readonly JsonSerializerOptions _readOptions;
    private readonly JsonSerializerOptions _writeOptions;

    private const string ProfileSearchPattern = "*.json";
    private const NotifyFilters WATCHER_NOTIFY_FILTERS =
        NotifyFilters.LastWrite | NotifyFilters.CreationTime;

    private FileSystemWatcher? _settingsFileWatcher;
    private CancellationTokenSource? _fileChangeDebounceCts;

    // Cache of profiles
    private List<PipelineProfile> _profiles = [];

    private bool _isInternalSave = false;
    private readonly object _fileLock = new object();

    /// <summary>
    /// Event that triggers when pipeline profiles are changed
    /// </summary>
    public event EventHandler? PipelineProfilesChanged;

    /// <summary>
    /// Initializes a new instance of the PipelineProfilesRepository class
    /// </summary>
    public PipelineProfilesRepository(string appDirectory, ILog log)
    {
        _log = log ?? throw new ArgumentNullException(nameof(log));
        _profilesDirectoryPath = Path.Combine(appDirectory, AppConstants.Folders.AppSettings, AppConstants.Folders.Profiles);

        _readOptions = GetSerializerOptions();
        _writeOptions = GetSerializerOptions();

        // Ensure directory exists
        if (!Directory.Exists(_profilesDirectoryPath))
        {
            Directory.CreateDirectory(_profilesDirectoryPath);
        }
    }

    /// <summary>
    /// Gets all pipeline profiles
    /// </summary>
    public List<PipelineProfile> GetAll()
    {
        return _profiles;
    }

    /// <summary>
    /// Gets a pipeline profile by its ID
    /// </summary>
    public PipelineProfile GetById(Guid id)
    {
        return _profiles.FirstOrDefault(p => p.Id == id)
            ?? throw new KeyNotFoundException($"Profile with ID {id} not found");
    }

    /// <summary>
    /// Gets a pipeline profile by its name
    /// </summary>
    public PipelineProfile GetByName(string name)
    {
        return _profiles.FirstOrDefault(p => p.Name == name)
            ?? throw new KeyNotFoundException($"Profile with name '{name}' not found");
    }

    /// <summary>
    /// Adds a new pipeline profile
    /// </summary>
    public void Add(PipelineProfile profile)
    {
        ArgumentNullException.ThrowIfNull(profile);

        if (_profiles.Any(p => p.Id == profile.Id))
            throw new InvalidOperationException($"Profile with ID {profile.Id} already exists");

        if (_profiles.Any(p => p.Name == profile.Name))
            throw new InvalidOperationException($"Profile with name '{profile.Name}' already exists");

        _profiles.Add(profile);

        // Save the new profile
        var path = Path.Combine(_profilesDirectoryPath, $"{profile.Name}.json");
        SaveProfileToFile(profile, path);
    }

    /// <summary>
    /// Updates an existing pipeline profile
    /// </summary>
    public void Update(PipelineProfile profile)
    {
        ArgumentNullException.ThrowIfNull(profile);

        var existingProfile = _profiles.FirstOrDefault(p => p.Id == profile.Id)
            ?? throw new KeyNotFoundException($"Profile with ID {profile.Id} not found");

        // Remove old profile
        _profiles.Remove(existingProfile);

        // Add updated profile
        _profiles.Add(profile);

        // Save the updated profile
        var path = Path.Combine(_profilesDirectoryPath, $"{profile.Name}.json");
        SaveProfileToFile(profile, path);

        // If name changed, delete the old file
        if (existingProfile.Name != profile.Name)
        {
            var oldPath = Path.Combine(_profilesDirectoryPath, $"{existingProfile.Name}.json");
            if (File.Exists(oldPath))
            {
                File.Delete(oldPath);
            }
        }
    }

    /// <summary>
    /// Updates a specific field in a pipeline profile
    /// </summary>
    public void UpdateField<TValue>(Guid profileId, string fieldPath, TValue value)
    {
        var profile = GetById(profileId);
        var filePath = Path.Combine(_profilesDirectoryPath, $"{profile.Name}.json");

        try
        {
            // Get the current JSON document
            JsonNode jsonDocument;
            if (File.Exists(filePath))
            {
                var json = File.ReadAllText(filePath);
                jsonDocument = JsonNode.Parse(json)
                    ?? throw new InvalidOperationException("Failed to parse JSON document");
            }
            else
            {
                throw new FileNotFoundException("Profile file not found", filePath);
            }

            // Update the field using the provided path (e.g., "Description")
            UpdateJsonNodeField(jsonDocument, fieldPath.Split('.'), value);

            // Create a temporary file path
            var tempFilePath = Path.GetTempFileName();

            // Write to temporary file first
            File.WriteAllText(tempFilePath, jsonDocument.ToJsonString(_writeOptions));

            // Atomically replace the original file
            File.Copy(tempFilePath, filePath, true);
            File.Delete(tempFilePath);

            _log.Info($"Field '{fieldPath}' successfully updated in profile {profile.Name}");

            // Reload the profile to update the cache
            ReloadProfiles();
        }
        catch (Exception ex)
        {
            _log.Error($"Failed to update field '{fieldPath}' in profile {profile.Name}: {ex.Message}", ex);
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
            if (node is JsonObject jsonObject)
            {
                jsonObject[pathParts[0]] = ConvertToJsonNode(value);
            }
            return;
        }

        // We need to navigate deeper
        var currentPart = pathParts[0];
        var remainingPath = new string[pathParts.Length - 1];
        Array.Copy(pathParts, 1, remainingPath, 0, remainingPath.Length);

        // If current node is an object
        if (node is JsonObject currentObject)
        {
            // If property doesn't exist or is null, create it
            if (!currentObject.ContainsKey(currentPart) || currentObject[currentPart] == null)
            {
                currentObject[currentPart] = new JsonObject();
            }

            // Navigate to the next level
            var nextNode = currentObject[currentPart];
            if (nextNode != null)
            {
                UpdateJsonNodeField(nextNode, remainingPath, value);
            }
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

    /// <summary>
    /// Deletes a pipeline profile
    /// </summary>
    public void Delete(Guid profileId)
    {
        var profile = _profiles.FirstOrDefault(p => p.Id == profileId)
            ?? throw new KeyNotFoundException($"Profile with ID {profileId} not found");

        var path = Path.Combine(_profilesDirectoryPath, $"{profile.Name}.json");
        if (File.Exists(path))
        {
            File.Delete(path);
        }

        _profiles.Remove(profile);
        _log.Info($"Profile '{profile.Name}' has been deleted");
    }

    /// <summary>
    /// Saves all profiles
    /// </summary>
    public void SaveAll(IEnumerable<PipelineProfile> profiles)
    {
        ArgumentNullException.ThrowIfNull(profiles);

        try
        {
            _isInternalSave = true;

            if (!Directory.Exists(_profilesDirectoryPath))
                Directory.CreateDirectory(_profilesDirectoryPath);

            var profilesList = profiles.ToList();
            var profileNames = profilesList.Select(p => $"{p.Name}.json").ToHashSet();

            var existingFiles = Directory.GetFiles(_profilesDirectoryPath, ProfileSearchPattern);
            foreach (var file in existingFiles)
            {
                if (!profileNames.Contains(Path.GetFileName(file)))
                {
                    File.Delete(file);
                    _log.Info($"Obsolete profile file {Path.GetFileName(file)} has been deleted.");
                }
            }

            foreach (var profile in profilesList)
            {
                var path = Path.Combine(_profilesDirectoryPath, $"{profile.Name}.json");
                SaveProfileToFile(profile, path);
            }

            _profiles = profilesList;
            _log.Info("All pipeline profiles have been successfully saved.");
        }
        finally
        {
            _isInternalSave = false;
        }
    }

    /// <summary>
    /// Load or initialize pipeline profiles
    /// </summary>
    public List<PipelineProfile> LoadOrInitialize(string appDirectory)
    {
        var path = Path.Combine(appDirectory, AppConstants.Folders.AppSettings, AppConstants.Folders.Profiles);
        _profiles = TryLoadProfilesFrom(path);

        if (_profiles.IsNullOrEmpty())
        {
            _profiles = InitializeDefaultProfiles(path);
            _log.Info($"Pipeline profiles have been successfully initialized.");
        }
        else
        {
            _log.Info($"Pipeline profiles have been successfully loaded.");
        }

        SetupFileWatcher(path);
        return _profiles;
    }

    /// <summary>
    /// Setup file watcher to monitor changes to profile files
    /// </summary>
    private void SetupFileWatcher(string path)
    {
        try
        {
            if (string.IsNullOrEmpty(path))
                return;

            _settingsFileWatcher = new FileSystemWatcher(path)
            {
                Filter = ProfileSearchPattern,
                NotifyFilter = WATCHER_NOTIFY_FILTERS
            };

            _settingsFileWatcher.Changed += OnSettingsFileChanged;
            _settingsFileWatcher.EnableRaisingEvents = true;
        }
        catch (Exception ex)
        {
            _log.Error($"Failed to set up file watcher for {path}: {ex.Message}");
        }
    }

    /// <summary>
    /// Handle file change events
    /// </summary>
    private volatile bool _isProcessingFileChange = false;

    private void OnSettingsFileChanged(object sender, FileSystemEventArgs e)
    {
        if (_isProcessingFileChange) return;

        _fileChangeDebounceCts?.Cancel();
        _fileChangeDebounceCts = new CancellationTokenSource();
        var token = _fileChangeDebounceCts.Token;

        Task.Delay(1000, token).ContinueWith(t =>
        {
            if (t.IsCanceled) return;

            try
            {
                _isProcessingFileChange = true;
                ReloadProfiles();
            }
            finally
            {
                _isProcessingFileChange = false;
            }
        }, TaskScheduler.Default);
    }

    /// <summary>
    /// Reload profiles from disk
    /// </summary>
    private void ReloadProfiles()
    {
        lock (_fileLock)
        {
            _profiles = TryLoadProfilesFrom(_profilesDirectoryPath);

            if (_profiles.IsNullOrEmpty())
            {
                _profiles = InitializeDefaultProfiles(_profilesDirectoryPath);
                _log.Info($"Pipeline profiles have been successfully re-initialized.");
            }
            else
            {
                _log.Info($"Pipeline profiles have been successfully reloaded.");
            }

            PipelineProfilesChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    /// <summary>
    /// Initialize default profiles
    /// </summary>
    private List<PipelineProfile> InitializeDefaultProfiles(string pathToFolder)
    {
        try
        {
            var appSettings = AppSettingsService.AppSettings;
            var solutionDir = appSettings.SolutionDirectories.FirstOrDefault();
            var httpDir = appSettings.HttpDirectories.FirstOrDefault();

            var devProfile = DefaultProfiles.GetIisResetProfile();
            SaveProfileToFile(devProfile, Path.Combine(pathToFolder, $"{devProfile.Name}.json"));

            var standardProfile = DefaultProfiles.GetStandardProfile();
            SaveProfileToFile(standardProfile, Path.Combine(pathToFolder, $"{standardProfile.Name}.json"));

            if (solutionDir == null || httpDir == null)
            {
                throw new InvalidOperationException("Solution or HTTP directory is not configured properly.");
            }

            var extendedProfile = DefaultProfiles.GetExtendedProfile(solutionDir, httpDir);
            SaveProfileToFile(extendedProfile, Path.Combine(pathToFolder, $"{extendedProfile.Name}.json"));

            return [devProfile, standardProfile, extendedProfile];
        }
        catch (Exception ex)
        {
            _log.Error($"Failed to initialize default profiles: {ex.Message}", ex);
            throw;
        }
    }

    /// <summary>
    /// Load profiles from disk
    /// </summary>
    private List<PipelineProfile> TryLoadProfilesFrom(string pathToFolder)
    {
        if (!Directory.Exists(pathToFolder))
        {
            _log.Info($"Profiles directory {pathToFolder} does not exist.");
            return [];
        }

        var files = Directory.GetFiles(pathToFolder, ProfileSearchPattern);
        _log.Info($"Found {files.Length} profile files in {pathToFolder}.");

        var profiles = new List<PipelineProfile>();

        foreach (var filePath in files)
        {
            try
            {
                var json = File.ReadAllText(filePath);

                var profile = JsonSerializer.Deserialize<PipelineProfile>(json, _readOptions)
                    ?? throw new Exception("Failed to deserialize profile.");

                profiles.Add(profile);

                _log.Info($"Successfully loaded profile from {Path.GetFileName(filePath)}.");
            }
            catch (Exception ex)
            {
                _log.Error($"Failed to load profile from {Path.GetFileName(filePath)}: {ex.Message}");
                throw;
            }
        }

        return profiles;
    }

    /// <summary>
    /// Save a profile to a file
    /// </summary>
    private void SaveProfileToFile(PipelineProfile profile, string path)
    {
        try
        {
            var directoryPath = Path.GetDirectoryName(path)
                ?? throw new Exception("Directory path is null.");

            Directory.CreateDirectory(directoryPath);

            // Create a temporary file path
            var tempFilePath = Path.GetTempFileName();

            // Write to temporary file first
            var json = JsonSerializer.Serialize(profile, _writeOptions);
            File.WriteAllText(tempFilePath, json);

            // Atomically replace the original file
            File.Copy(tempFilePath, path, true);
            File.Delete(tempFilePath);

            _log.Info($"Profile '{profile.Name}' has been successfully saved to {Path.GetFileName(path)}.");
        }
        catch (Exception ex)
        {
            _log.Error($"Failed to save profile '{profile.Name}' to {Path.GetFileName(path)}: {ex.Message}");
            throw;
        }
    }

    private static JsonSerializerOptions GetSerializerOptions()
    {
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            IncludeFields = true,
            WriteIndented = true
        };
        options.Converters.Add(new PipelineOperationJsonConverter());
        return options;
    }
}