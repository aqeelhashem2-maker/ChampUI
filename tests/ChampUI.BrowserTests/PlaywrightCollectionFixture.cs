using Xunit;

namespace ChampUI.BrowserTests;

/// <summary>
/// Collection fixture to ensure Playwright tests run serially (avoid port conflicts).
/// </summary>
[CollectionDefinition("PlaywrightCollection")]
public class PlaywrightCollectionFixture : ICollectionFixture<PlaywrightFixture>
{
}

/// <summary>
/// Shared fixture for Playwright browser tests.
/// Note: Demo application must be running on http://localhost:5000 before tests execute.
/// </summary>
public class PlaywrightFixture
{
    // Placeholder - actual browser instances created per-test for isolation
}
