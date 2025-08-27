using DryIoc;
using Gloam.Data.Entities.Base;
using Gloam.Data.Interfaces.Content;
using Gloam.Data.Interfaces.Loader;
using Gloam.Data.Interfaces.Validation;
using Gloam.Runtime;
using Gloam.Runtime.Config;
using Gloam.Runtime.Types;
using Spectre.Console;

namespace Gloam.Cli.Commands;

/// <summary>
///     CLI commands for Gloam
/// </summary>
public class Validate
{
    /// <summary>
    ///     Validates a JSON entity file
    /// </summary>
    /// <param name="file">Path to the JSON file to validate</param>
    /// <param name="rootDir">Root directory for content loading (defaults to current directory)</param>
    /// <param name="verbose">Enable verbose output</param>
    public static async Task ValidateAsync(
        string file,
        string rootDir = ".",
        bool verbose = false
    )
    {
        if (verbose)
        {
            AnsiConsole.MarkupLine($"[dim]Validating file: {file}[/]");
            AnsiConsole.MarkupLine($"[dim]Root directory: {Path.GetFullPath(rootDir)}[/]");
        }

        // Check if file exists
        if (!File.Exists(file))
        {
            AnsiConsole.MarkupLine($"[red]Error:[/] File '{file}' not found.");
            Environment.ExitCode = 1;
            return;
        }

        // Check if root directory exists
        if (!Directory.Exists(rootDir))
        {
            AnsiConsole.MarkupLine($"[red]Error:[/] Root directory '{rootDir}' not found.");
            Environment.ExitCode = 1;
            return;
        }

        try
        {
            await AnsiConsole.Progress()
                .StartAsync(async ctx =>
                    {
                        var task = ctx.AddTask("[green]Validating JSON entities...[/]");

                        // Configure Gloam host
                        var config = new GloamHostConfig
                        {
                            RootDirectory = Path.GetFullPath(rootDir),
                            LoaderType = ContentLoaderType.FileSystem,
                            LogLevel = verbose ? LogLevelType.Debug : LogLevelType.Warning,
                            EnableConsoleLogging = verbose,
                            EnableFileLogging = false
                        };

                        task.Increment(20);

                        using var host = new GloamHost(config);
                        var entityLoader = host.Container.Resolve<IEntityDataLoader>();

                        task.Increment(30);

                        // Read the file content directly
                        var contentLoader = host.Container.Resolve<IContentLoader>();
                        var fileContent = await contentLoader.ReadTextAsync(file);

                        // Validate against schema
                        var schemaValidator = host.Container.Resolve<IEntitySchemaValidator>();
                        var schema = await schemaValidator.GetSchemaAsync<BaseGloamEntity>();
                        var validationResult = schemaValidator.Validate(fileContent, schema);

                        task.Increment(20);

                        if (!validationResult.Ok)
                        {
                            task.StopTask();
                            AnsiConsole.MarkupLine("[red]✗[/] Validation failed:");
                            foreach (var error in validationResult.Errors)
                            {
                                AnsiConsole.MarkupLine($"  [red]•[/] {error.Replace("[", "[[").Replace("]", "]]")}");
                            }

                            Environment.ExitCode = 1;
                            return;
                        }

                        task.Increment(20);
                        task.StopTask();

                        AnsiConsole.MarkupLine($"[green]✓[/] Successfully validated JSON file '{file}'");
                    }
                );
        }
        catch (FileNotFoundException ex)
        {
            AnsiConsole.MarkupLine($"[red]Error:[/] {ex.Message}");
            Environment.ExitCode = 1;
        }
        catch (InvalidOperationException ex)
        {
            AnsiConsole.MarkupLine($"[red]Validation Error:[/] {ex.Message}");
            Environment.ExitCode = 1;
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Unexpected Error:[/] {ex.Message}");
            if (verbose)
            {
                AnsiConsole.WriteException(ex);
            }

            Environment.ExitCode = 1;
        }
    }
}
