using System.Reflection;

namespace BuildingBlocks.Tools.Tests.Unit;

public class EnvGeneratorTests : IDisposable
{
    private readonly string _testProjectPath;
    private readonly string _tempOutputPath;

    public EnvGeneratorTests()
    {
        // Get path to test project source folder (not bin folder)
        // Navigate from bin/Debug/net10.0 to the project root
        var testAssemblyPath = Assembly.GetExecutingAssembly().Location;
        var binFolder = Path.GetDirectoryName(testAssemblyPath)!; // bin/Debug/net10.0
        var debugFolder = Path.GetDirectoryName(binFolder)!; // bin/Debug
        var srcFolder = Path.GetDirectoryName(debugFolder)!; // bin
        var projectFolder = Path.GetDirectoryName(srcFolder)!; // project root

        _testProjectPath = Path.Combine(projectFolder, "BuildingBlocks.Tools.Tests.Unit.csproj");
        _tempOutputPath = Path.Combine(Path.GetTempPath(), $"test-env-{Guid.NewGuid()}.env");
    }

    public void Dispose()
    {
        if (File.Exists(_tempOutputPath))
        {
            File.Delete(_tempOutputPath);
        }
    }

    [Fact]
    public void Program_WithTestProject_GeneratesEnvFileWithAllOptions()
    {
        // Arrange
        var args = new[] { _testProjectPath, "--output", _tempOutputPath };

        // Capture console output
        var originalOut = Console.Out;
        var originalError = Console.Error;
        using var outWriter = new StringWriter();
        using var errorWriter = new StringWriter();
        Console.SetOut(outWriter);
        Console.SetError(errorWriter);

        try
        {
            // Act
            var exitCode = BuildingBlocks.Tools.Program.Main(args);

            // Assert
            if (exitCode != 0)
            {
                var output = outWriter.ToString();
                var error = errorWriter.ToString();
                throw new Exception($"Exit code: {exitCode}\nOutput:\n{output}\nError:\n{error}");
            }

            exitCode.Should().Be(0);
        File.Exists(_tempOutputPath).Should().BeTrue();

        var content = File.ReadAllText(_tempOutputPath);

        // SimpleOptions
        content.Should().Contain("SIMPLECONFIG__APIURL=https://api.example.com");
        content.Should().Contain("SIMPLECONFIG__APIKEY=");
        content.Should().Contain("SIMPLECONFIG__MAXRETRIES=3");
        content.Should().Contain("SIMPLECONFIG__ENABLELOGGING=True");
        content.Should().Contain("SIMPLECONFIG__TIMEOUT=30.0");

        // NestedOptions
        content.Should().Contain("NESTEDCONFIG__BASEURL=");
        content.Should().Contain("NESTEDCONFIG__USER=");
        content.Should().Contain("NESTEDCONFIG__KEYS__KEYA=default-key-a");
        content.Should().Contain("NESTEDCONFIG__KEYS__KEYB=");
        content.Should().Contain("NESTEDCONFIG__KEYS__KEYC=default-key-c");

        // ComplexNestedOptions
        content.Should().Contain("COMPLEXCONFIG__SERVICENAME=");
        content.Should().Contain("COMPLEXCONFIG__DATABASE__CONNECTIONSTRING=Server=localhost;Database=mydb");
        content.Should().Contain("COMPLEXCONFIG__DATABASE__COMMANDTIMEOUT=30");
        content.Should().Contain("COMPLEXCONFIG__DATABASE__CREDENTIALS__USERNAME=admin");
        content.Should().Contain("COMPLEXCONFIG__DATABASE__CREDENTIALS__PASSWORD=");
        content.Should().Contain("COMPLEXCONFIG__CACHE__PROVIDER=InMemory");
        content.Should().Contain("COMPLEXCONFIG__CACHE__EXPIRATIONMINUTES=60");
        content.Should().Contain("COMPLEXCONFIG__CACHE__SERVERSETTINGS__HOST=localhost");
        content.Should().Contain("COMPLEXCONFIG__CACHE__SERVERSETTINGS__PORT=6379");

        // ColonSectionOptions - verify colon is replaced with double underscore
        content.Should().Contain("AUTH__SECTIONNAME__AUDIENCE=my-app");
        content.Should().Contain("AUTH__SECTIONNAME__CLIENTID=");
        content.Should().Contain("AUTH__SECTIONNAME__AUTHORITY=https://auth.example.com");
        }
        finally
        {
            Console.SetOut(originalOut);
            Console.SetError(originalError);
        }
    }

    [Fact]
    public void Program_WithoutOutputPath_GeneratesEnvInProjectDirectory()
    {
        // Arrange
        var args = new[] { _testProjectPath };
        var projectDir = Path.GetDirectoryName(_testProjectPath)!;
        var defaultEnvPath = Path.Combine(projectDir, ".env.template");

        try
        {
            // Clean up any existing .env file
            if (File.Exists(defaultEnvPath))
            {
                File.Delete(defaultEnvPath);
            }

            // Act
            var exitCode = BuildingBlocks.Tools.Program.Main(args);

            // Assert
            exitCode.Should().Be(0);
            File.Exists(defaultEnvPath).Should().BeTrue();
        }
        finally
        {
            // Cleanup
            if (File.Exists(defaultEnvPath))
            {
                File.Delete(defaultEnvPath);
            }
        }
    }

    [Fact]
    public void Program_WithMissingFile_ReturnsErrorCode()
    {
        // Arrange
        var args = new[] { "nonexistent.csproj" };

        // Act
        var exitCode = BuildingBlocks.Tools.Program.Main(args);

        // Assert
        exitCode.Should().Be(1);
    }

    [Fact]
    public void Program_WithNoArguments_ReturnsErrorCode()
    {
        // Arrange
        var args = Array.Empty<string>();

        // Act
        var exitCode = BuildingBlocks.Tools.Program.Main(args);

        // Assert
        exitCode.Should().Be(1);
    }
}
