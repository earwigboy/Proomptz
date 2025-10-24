using System.Text;
using System.Text.RegularExpressions;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace PromptTemplateManager.Infrastructure.FileSystem;

/// <summary>
/// Manages file I/O operations for templates, including YAML frontmatter parsing and file locking.
/// </summary>
public class TemplateFileManager
{
    private static readonly ISerializer YamlSerializer = new SerializerBuilder()
        .WithNamingConvention(CamelCaseNamingConvention.Instance)
        .Build();

    private static readonly IDeserializer YamlDeserializer = new DeserializerBuilder()
        .WithNamingConvention(CamelCaseNamingConvention.Instance)
        .Build();

    private const int MaxRetries = 3;
    private const int RetryDelayMs = 100;

    /// <summary>
    /// Parses a template file with YAML frontmatter.
    /// </summary>
    /// <param name="fileContents">The raw file contents.</param>
    /// <returns>A tuple containing the metadata and content.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the frontmatter is missing or invalid.</exception>
    public static (TemplateMetadata Metadata, string Content) ParseTemplateFile(string fileContents)
    {
        var frontmatterPattern = @"^---\s*\n(.*?)\n---\s*\n(.*)$";
        var match = Regex.Match(fileContents, frontmatterPattern, RegexOptions.Singleline);

        if (!match.Success)
        {
            throw new InvalidOperationException("Missing or invalid YAML frontmatter");
        }

        var yamlText = match.Groups[1].Value;
        var content = match.Groups[2].Value;

        try
        {
            var metadata = YamlDeserializer.Deserialize<TemplateMetadata>(yamlText);
            return (metadata, content);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to parse YAML frontmatter: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Serializes template metadata and content to a file format with YAML frontmatter.
    /// </summary>
    /// <param name="metadata">The template metadata.</param>
    /// <param name="content">The template content.</param>
    /// <returns>The complete file contents with frontmatter.</returns>
    public static string SerializeTemplateFile(TemplateMetadata metadata, string content)
    {
        var yaml = YamlSerializer.Serialize(metadata).Trim();
        return $"---\n{yaml}\n---\n{content}";
    }

    /// <summary>
    /// Reads a template file from disk with exclusive read lock and retry logic.
    /// </summary>
    /// <param name="filePath">The path to the template file.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The file contents.</returns>
    public static async Task<string> ReadFileAsync(string filePath, CancellationToken cancellationToken = default)
    {
        return await RetryOnLockAsync(async () =>
        {
            await using var stream = new FileStream(
                filePath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read, // Allow other readers
                bufferSize: 4096,
                useAsync: true
            );

            using var reader = new StreamReader(stream, Encoding.UTF8);
            return await reader.ReadToEndAsync(cancellationToken);
        });
    }

    /// <summary>
    /// Writes a template file to disk with exclusive write lock and retry logic.
    /// </summary>
    /// <param name="filePath">The path to the template file.</param>
    /// <param name="contents">The file contents to write.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public static async Task WriteFileAsync(string filePath, string contents, CancellationToken cancellationToken = default)
    {
        await RetryOnLockAsync(async () =>
        {
            // Ensure directory exists
            var directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            await using var stream = new FileStream(
                filePath,
                FileMode.Create,
                FileAccess.Write,
                FileShare.None, // Exclusive access during write
                bufferSize: 4096,
                useAsync: true
            );

            var buffer = Encoding.UTF8.GetBytes(contents);
            await stream.WriteAsync(buffer, cancellationToken);
            return 0; // Return value for RetryOnLockAsync
        });
    }

    /// <summary>
    /// Reads and parses a folder metadata file.
    /// </summary>
    /// <param name="folderMetaPath">The path to the .folder-meta file.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The folder metadata.</returns>
    public static async Task<FolderMetadata> ReadFolderMetadataAsync(string folderMetaPath, CancellationToken cancellationToken = default)
    {
        var contents = await ReadFileAsync(folderMetaPath, cancellationToken);

        try
        {
            return YamlDeserializer.Deserialize<FolderMetadata>(contents);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to parse folder metadata: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Writes folder metadata to a .folder-meta file.
    /// </summary>
    /// <param name="folderMetaPath">The path to the .folder-meta file.</param>
    /// <param name="metadata">The folder metadata to write.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public static async Task WriteFolderMetadataAsync(string folderMetaPath, FolderMetadata metadata, CancellationToken cancellationToken = default)
    {
        var yaml = YamlSerializer.Serialize(metadata).Trim();
        await WriteFileAsync(folderMetaPath, yaml, cancellationToken);
    }

    /// <summary>
    /// Retries an operation with exponential backoff if it fails due to file locking.
    /// </summary>
    private static async Task<T> RetryOnLockAsync<T>(Func<Task<T>> operation)
    {
        for (int i = 0; i < MaxRetries; i++)
        {
            try
            {
                return await operation();
            }
            catch (IOException ex) when (IsFileLocked(ex))
            {
                if (i == MaxRetries - 1)
                {
                    throw;
                }

                await Task.Delay(RetryDelayMs * (int)Math.Pow(2, i));
            }
        }

        throw new InvalidOperationException("Retry logic failed unexpectedly");
    }

    /// <summary>
    /// Checks if an IOException is caused by file locking.
    /// </summary>
    private static bool IsFileLocked(IOException exception)
    {
        const int ErrorSharingViolation = 0x20;
        const int ErrorLockViolation = 0x21;

        var errorCode = exception.HResult & 0xFFFF;
        return errorCode == ErrorSharingViolation || errorCode == ErrorLockViolation;
    }
}
