using ConsoleAppFramework;
using Gloam.Cli.Commands;

var app = ConsoleApp.Create();
app.Add("validate", Validate.ValidateAsync);
app.Run(args);
