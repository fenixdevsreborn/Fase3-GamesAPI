using System.Net.Http;

namespace Fcg.Games.Api.Authentication;

/// <summary>Optional factory for JWT metadata backchannel. When registered, JwtBearer uses it instead of the default HttpClient. Used in tests to mock OIDC discovery/JWKS.</summary>
public interface IBackchannelHttpHandlerFactory
{
    /// <summary>Returns the HttpMessageHandler to use for metadata and JWKS requests. Called once when JwtBearerOptions are configured.</summary>
    HttpMessageHandler Create();
}
