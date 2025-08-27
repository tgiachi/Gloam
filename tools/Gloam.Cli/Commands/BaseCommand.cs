using Spectre.Console;

namespace Gloam.Cli.Commands;

/// <summary>
///     Base class for Gloam CLI commands with common functionality
/// </summary>
public abstract class BaseCommand
{
    /// <summary>
    ///     Displays an error message and exits with error code
    /// </summary>
    protected static void ShowError(string message, int exitCode = 1)
    {
        AnsiConsole.MarkupLine($"[red]Error:[/] {message}");
        Environment.ExitCode = exitCode;
    }

    /// <summary>
    ///     Displays a success message
    /// </summary>
    protected static void ShowSuccess(string message)
    {
        AnsiConsole.MarkupLine($"[green]✓[/] {message}");
    }

    /// <summary>
    ///     Displays a warning message
    /// </summary>
    protected static void ShowWarning(string message)
    {
        AnsiConsole.MarkupLine($"[yellow]![/] {message}");
    }

    /// <summary>
    ///     Displays an info message when verbose is enabled
    /// </summary>
    protected static void ShowVerbose(string message, bool verbose)
    {
        if (verbose)
        {
            AnsiConsole.MarkupLine($"[dim]{message}[/]");
        }
    }

    /// <summary>
    ///     Validates that a file exists
    /// </summary>
    protected static bool ValidateFileExists(string filePath)
    {
        if (File.Exists(filePath))
        {
            return true;
        }

        ShowError($"File '{filePath}' not found.");
        return false;
    }

    /// <summary>
    ///     Validates that a directory exists
    /// </summary>
    protected static bool ValidateDirectoryExists(string directoryPath)
    {
        if (Directory.Exists(directoryPath))
        {
            return true;
        }

        ShowError($"Directory '{directoryPath}' not found.");
        return false;
    }
}
