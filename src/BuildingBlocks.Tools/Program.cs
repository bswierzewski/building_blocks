using System.CommandLine;
using BuildingBlocks.Tools.Commands;

var rootCommand = new RootCommand("BuildingBlocks CLI - Tools for managing project configurations");

var envCommand = new Command("env", "Manage environment variables and .env files")
{
    EnvGenerateCommand.Create(),
    EnvListCommand.Create(),
    EnvUpdateCommand.Create()
};

rootCommand.Add(envCommand);

return rootCommand.Parse(args).Invoke();
