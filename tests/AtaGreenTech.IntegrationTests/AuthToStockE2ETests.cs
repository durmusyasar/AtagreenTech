using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using AtaGreenTech.IntegrationTests.Infrastructure;
using FluentAssertions;

namespace AtaGreenTech.IntegrationTests;

/// <summary>
/// Login → refresh (rotation + reuse detection) → Stock permission enforcement
/// (security-test: 401 / 200 / 403) uçtan uca kontrat testleri.
/// </summary>
[Collection(AuthStockIntegrationCollection.Name)]
public sealed class AuthToStockE2ETests
{
    private readonly AuthStockIntegrationFixture _fixture;

    public AuthToStockE2ETests(AuthStockIntegrationFixture fixture)
    {
        _fixture = fixture;
    }

    private sealed record TokenPair(
        [property: JsonPropertyName("accessToken")] string AccessToken,
        [property: JsonPropertyName("refreshToken")] string RefreshToken,
        [property: JsonPropertyName("expiresAt")] DateTime ExpiresAt
    );

    [Fact]
    public async Task Full_Flow_Login_Refresh_And_Access_Protected_Stock_Endpoints_Should_Succeed()
    {
        using var userClient = _fixture.CreateUserClient();
        using var stockClient = _fixture.CreateStockClient();

        // 1. Token yok → Stock protected 401
        var unauth = await stockClient.GetAsync("/api/v1/security-test/protected");
        unauth.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        // 2. Bootstrap Admin login (ilk istek RbacCatalog + admin seed)
        var login = await userClient.PostAsJsonAsync(
            "/api/v1/auth/login",
            new { email = SharedJwtContract.AdminEmail, password = SharedJwtContract.AdminPassword }
        );
        login.StatusCode.Should().Be(HttpStatusCode.OK);
        var initial = await login.Content.ReadFromJsonAsync<TokenPair>();
        initial.Should().NotBeNull();
        initial!.AccessToken.Should().NotBeNullOrWhiteSpace();
        initial.RefreshToken.Should().NotBeNullOrWhiteSpace();

        // 3. Refresh rotation → yeni access + refresh
        var refresh = await userClient.PostAsJsonAsync(
            "/api/v1/auth/refresh",
            new { refreshToken = initial.RefreshToken }
        );
        refresh.StatusCode.Should().Be(HttpStatusCode.OK);
        var rotated = await refresh.Content.ReadFromJsonAsync<TokenPair>();
        rotated.Should().NotBeNull();
        rotated!.AccessToken.Should().NotBe(initial.AccessToken);
        rotated.RefreshToken.Should().NotBe(initial.RefreshToken);

        // 4. Rotated access token → Stock read + write policy OK
        stockClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", rotated.AccessToken);

        var read = await stockClient.GetAsync("/api/v1/security-test/protected");
        read.StatusCode.Should().Be(HttpStatusCode.OK);

        var write = await stockClient.GetAsync("/api/v1/security-test/write-check");
        write.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Viewer_Token_Should_Read_Stock_But_Get_403_On_Write_Endpoint()
    {
        using var userClient = _fixture.CreateUserClient();
        using var stockClient = _fixture.CreateStockClient();

        var adminTokens = await LoginAsync(userClient, SharedJwtContract.AdminEmail, SharedJwtContract.AdminPassword);
        await EnsureViewerUserAsync(userClient, adminTokens.AccessToken);

        var viewerTokens = await LoginAsync(
            userClient,
            SharedJwtContract.ViewerEmail,
            SharedJwtContract.ViewerPassword
        );

        stockClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", viewerTokens.AccessToken);

        var read = await stockClient.GetAsync("/api/v1/security-test/protected");
        read.StatusCode.Should().Be(HttpStatusCode.OK);

        var write = await stockClient.GetAsync("/api/v1/security-test/write-check");
        write.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Reusing_Rotated_Refresh_Token_Should_Revoke_Token_Family()
    {
        using var userClient = _fixture.CreateUserClient();

        var first = await LoginAsync(userClient, SharedJwtContract.AdminEmail, SharedJwtContract.AdminPassword);

        var secondRes = await userClient.PostAsJsonAsync(
            "/api/v1/auth/refresh",
            new { refreshToken = first.RefreshToken }
        );
        secondRes.StatusCode.Should().Be(HttpStatusCode.OK);
        var second = await secondRes.Content.ReadFromJsonAsync<TokenPair>();
        second.Should().NotBeNull();

        // Eski (rotated) refresh tekrar → reuse detection
        var reuse = await userClient.PostAsJsonAsync(
            "/api/v1/auth/refresh",
            new { refreshToken = first.RefreshToken }
        );
        reuse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        // Aile iptal → ikinci geçerli refresh de düşmeli
        var afterBreach = await userClient.PostAsJsonAsync(
            "/api/v1/auth/refresh",
            new { refreshToken = second!.RefreshToken }
        );
        afterBreach.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    private static async Task<TokenPair> LoginAsync(HttpClient userClient, string email, string password)
    {
        var response = await userClient.PostAsJsonAsync(
            "/api/v1/auth/login",
            new { email, password }
        );
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var tokens = await response.Content.ReadFromJsonAsync<TokenPair>();
        tokens.Should().NotBeNull();
        return tokens!;
    }

    private static async Task EnsureViewerUserAsync(HttpClient userClient, string adminAccessToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/users")
        {
            Content = JsonContent.Create(
                new
                {
                    email = SharedJwtContract.ViewerEmail,
                    password = SharedJwtContract.ViewerPassword,
                    firstName = "Viewer",
                    lastName = "User",
                    roles = new[] { "Viewer" },
                    isActive = true,
                }
            ),
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", adminAccessToken);

        var response = await userClient.SendAsync(request);
        // İlk oluşturma 201; tekrar koşularda e-posta çakışması 400 olabilir — login yine çalışır.
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Created, HttpStatusCode.BadRequest);
    }
}
