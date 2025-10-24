using System.Text;

namespace PromptTemplateManager.Infrastructure.FileSystem;

/// <summary>
/// Utility for sanitizing filenames to ensure cross-platform compatibility.
/// Handles illegal characters, reserved names, and directory traversal prevention.
/// </summary>
public static class FileNameSanitizer
{
    private static readonly HashSet<char> IllegalCharacters = new()
    {
        '<', '>', ':', '"', '/', '\\', '|', '?', '*'
    };

    private static readonly Dictionary<char, char> CharacterReplacements = new()
    {
        { ':', '-' },   // "Template: Draft" → "Template - Draft"
        { '/', '_' },   // "Before/After" → "Before_After"
        { '\\', '_' },
        { '<', '_' },
        { '>', '_' },
        { '|', '_' },
        { '?', '_' },
        { '*', '_' },
        { '"', '\'' }   // Preserve quotes as single quotes
    };

    private static readonly HashSet<string> WindowsReservedNames = new(StringComparer.OrdinalIgnoreCase)
    {
        "CON", "PRN", "AUX", "NUL",
        "COM1", "COM2", "COM3", "COM4", "COM5", "COM6", "COM7", "COM8", "COM9",
        "LPT1", "LPT2", "LPT3", "LPT4", "LPT5", "LPT6", "LPT7", "LPT8", "LPT9"
    };

    private const int MaxFileNameLength = 200;

    /// <summary>
    /// Sanitizes a filename by replacing illegal characters with safe alternatives.
    /// </summary>
    /// <param name="fileName">The filename to sanitize (without extension).</param>
    /// <returns>A sanitized filename safe for all platforms.</returns>
    public static string Sanitize(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
        {
            return "Untitled";
        }

        var sanitized = new StringBuilder();

        for (int i = 0; i < fileName.Length; i++)
        {
            var c = fileName[i];

            // Skip control characters
            if (char.IsControl(c))
            {
                continue;
            }

            // Special handling for colon: ensure space before dash
            if (c == ':')
            {
                // Add space before dash if previous char wasn't a space
                if (sanitized.Length > 0 && sanitized[sanitized.Length - 1] != ' ')
                {
                    sanitized.Append(' ');
                }
                sanitized.Append('-');
                continue;
            }

            // Replace other illegal characters
            if (CharacterReplacements.TryGetValue(c, out var replacement))
            {
                sanitized.Append(replacement);
            }
            else
            {
                sanitized.Append(c);
            }
        }

        // Trim leading/trailing spaces and dots (problematic on Windows)
        var result = sanitized.ToString().Trim(' ', '.');

        // Ensure not empty after trimming
        if (string.IsNullOrWhiteSpace(result))
        {
            return "Untitled";
        }

        // Check for Windows reserved names
        var nameWithoutExtension = Path.GetFileNameWithoutExtension(result);
        if (WindowsReservedNames.Contains(nameWithoutExtension))
        {
            result = "_" + result;
        }

        // Truncate to max filename length
        if (result.Length > MaxFileNameLength)
        {
            result = result.Substring(0, MaxFileNameLength);
        }

        return result;
    }

    /// <summary>
    /// Generates a unique filename by appending a number if the file already exists.
    /// </summary>
    /// <param name="directory">The directory where the file will be created.</param>
    /// <param name="baseName">The base filename (without extension).</param>
    /// <param name="extension">The file extension (with dot, e.g., ".md").</param>
    /// <returns>A unique filename that doesn't exist in the directory.</returns>
    public static string GetUniqueFileName(string directory, string baseName, string extension)
    {
        var sanitizedBase = Sanitize(baseName);
        var fileName = sanitizedBase + extension;
        var filePath = Path.Combine(directory, fileName);

        if (!File.Exists(filePath))
        {
            return fileName;
        }

        // Append incrementing number until we find a unique name
        int counter = 2;
        while (true)
        {
            fileName = $"{sanitizedBase} ({counter}){extension}";
            filePath = Path.Combine(directory, fileName);

            if (!File.Exists(filePath))
            {
                return fileName;
            }

            counter++;

            // Prevent infinite loop
            if (counter > 10000)
            {
                throw new InvalidOperationException($"Could not generate unique filename for '{baseName}' in directory '{directory}'");
            }
        }
    }

    /// <summary>
    /// Checks if a filename contains any illegal characters.
    /// </summary>
    /// <param name="fileName">The filename to check.</param>
    /// <returns>True if the filename contains illegal characters, false otherwise.</returns>
    public static bool ContainsIllegalCharacters(string fileName)
    {
        if (string.IsNullOrEmpty(fileName))
        {
            return false;
        }

        return fileName.Any(c => IllegalCharacters.Contains(c) || char.IsControl(c));
    }
}
