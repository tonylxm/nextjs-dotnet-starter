using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

namespace Starter.Api.Auth;

public sealed class SupabaseOptions
{
    public const string Section = "Supabase";

    [Required, Url]
    public string Url { get; init; } = "";
}

public static class SupabaseAuthExtensions
{
    private const string SupabaseAudience = "authenticated";

    public static IServiceCollection AddSupabaseAuth(this IServiceCollection services)
    {
        services.AddOptions<SupabaseOptions>()
            .BindConfiguration(SupabaseOptions.Section)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer();
        services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
            .Configure<IOptions<SupabaseOptions>>((jwt, supabase) =>
            {
                var issuer = new Uri($"{supabase.Value.Url.TrimEnd('/')}/auth/v1");
                jwt.MapInboundClaims = false;
                jwt.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidIssuer = issuer.ToString(),
                    ValidAudience = SupabaseAudience,
                };
                // Supabase's OIDC discovery document isn't on every project yet, so read the JWKS directly.
                jwt.ConfigurationManager = new ConfigurationManager<OpenIdConnectConfiguration>(
                    $"{issuer}/.well-known/jwks.json",
                    new JwksRetriever(),
                    new HttpDocumentRetriever { RequireHttps = !issuer.IsLoopback });
            });

        services.AddAuthorizationBuilder()
            .SetFallbackPolicy(new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build());

        return services;
    }

    private sealed class JwksRetriever : IConfigurationRetriever<OpenIdConnectConfiguration>
    {
        public async Task<OpenIdConnectConfiguration> GetConfigurationAsync(
            string address, IDocumentRetriever retriever, CancellationToken cancel)
        {
            var keySet = new JsonWebKeySet(await retriever.GetDocumentAsync(address, cancel));
            var configuration = new OpenIdConnectConfiguration();
            foreach (var key in keySet.GetSigningKeys())
            {
                configuration.SigningKeys.Add(key);
            }

            return configuration;
        }
    }
}
