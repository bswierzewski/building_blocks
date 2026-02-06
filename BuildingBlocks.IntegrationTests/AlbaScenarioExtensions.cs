using Alba;
using System.Text.Json;
using Xunit.Abstractions;

namespace BuildingBlocks.IntegrationTests;

/// <summary>
/// Extension methods for Alba IScenarioResult to simplify test output logging.
/// </summary>
public static class AlbaScenarioExtensions
{
    /// <summary>
    /// Prints the HTTP response body as formatted JSON to the test output.
    /// Works with any response - errors, success responses, lists, etc.
    /// </summary>
    /// <param name="resultTask">The Alba scenario result task.</param>
    /// <param name="output">The xUnit test output helper.</param>
    /// <param name="header">Optional header text to display before the JSON output. Defaults to "Response Body:".</param>
    /// <returns>The same IScenarioResult for fluent chaining.</returns>
    /// <example>
    /// <code>
    /// // Print error response
    /// await AlbaHost.Scenario(s =>
    /// {
    ///     s.Get.Url("/orders/invalid");
    ///     s.StatusCodeShouldBe(HttpStatusCode.BadRequest);
    /// }).PrintBody(output);
    ///
    /// // Print success response
    /// await AlbaHost.Scenario(s =>
    /// {
    ///     s.Get.Url("/orders");
    ///     s.StatusCodeShouldBe(HttpStatusCode.OK);
    /// }).PrintBody(output, "Orders List:");
    /// </code>
    /// </example>
    public static async Task<IScenarioResult> PrintBody(
        this Task<IScenarioResult> resultTask,
        ITestOutputHelper output,
        string header = "Response Body:")
    {
        var result = await resultTask;
        var responseText = await result.ReadAsTextAsync();

        var formattedJson = JsonSerializer.Serialize(
            JsonSerializer.Deserialize<JsonElement>(responseText),
            new JsonSerializerOptions { WriteIndented = true });

        output.WriteLine(header);
        output.WriteLine(formattedJson);

        return result;
    }
}
