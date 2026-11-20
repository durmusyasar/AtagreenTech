using AtaGreenTech.SharedKernel.Localization;

namespace AtaGreenTech.UserService.Tests;

internal static class TestLocalizers
{
    public static IErrorLocalizer Passthrough { get; } = new PassthroughErrorLocalizer();

    private sealed class PassthroughErrorLocalizer : IErrorLocalizer
    {
        public string Localize(string errorCode, params object[] args) => errorCode;

        public string Localize(string errorCode, string? fallback, params object[] args) =>
            fallback ?? errorCode;
    }
}
