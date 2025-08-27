using ConsoleAppFramework;
using Gloam.Cli.Commands;

// Create and configure the CLI application
var app = ConsoleApp.Create();

// Register validate command for JSON entity validation
app.Add("validate", Validate.ValidateAsync);

// Run the application with command line arguments
app.Run(args);
