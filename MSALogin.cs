using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;

namespace XSTSPlayground
{
    public class MSAAuthorizeRequest
    {
        public string? client_id { get; set; }
        public string? response_type { get; set; }
        public string? scope { get; set; }
        public string? redirect_uri { get; set; }
        public string? state { get; set; }
    }

    public class MSACodeRedeemRequest
    {
        public string? client_id { get; set; }
        public string? client_secret { get; set; }
        public string? code { get; set; }
        public string? refresh_token { get; set; }
        public string? grant_type { get; set; }
        public string? redirect_uri { get; set; }
    }

    public class MSACodeRedeemResponse
    {
        public string? token_type { get; set; }
        public int expires_in { get; set; }
        public string? scope { get; set; }
        public string? access_token { get; set; }
        public string? refresh_token { get; set; }
        public string? user_id { get; set; }
        public string? foci { get; set; }
    }

    public class MSALogin
    {
        // default values
        public const string NATIVECLIENT_REDIRECT_URI = "https://login.microsoftonline.com/common/oauth2/nativeclient";
        public const string MINECRAFT_LAUNCHER_CLIENT_ID = "00000000402B5328";

        // default scopes
        public const string SCOPE_XBOXLIVE_LOGIN = "XboxLive.signin";
        public const string SCOPE_XBOXLIVE_LOGIN_AND_REFRESH = "XboxLive.signin XboxLive.offline_access";

        // URL bases
        public const string MSA_API_AUTHORIZE = "https://login.live.com/oauth20_authorize.srf";
        public const string MSA_API_TOKEN = "https://login.live.com/oauth20_token.srf";
        // required for enterprise accounts? not sure
        public const string AZURE_API_AUTHORIZE = "https://login.microsoftonline.com/common/v2.0/oauth2/authorize";
        public const string AZURE_API_TOKEN = "https://login.microsoftonline.com/common/v2.0/oauth2/token";

        // let us change what urls are used
        public static string URL_AUTHORIZE = MSA_API_AUTHORIZE;
        public static string URL_TOKEN = MSA_API_TOKEN;

        // make ourselves a HTTP client
        private static HttpClient client = new HttpClient();

        async public static Task<string?> GetAuthorizeURL(string clientID = MINECRAFT_LAUNCHER_CLIENT_ID, string scopes = SCOPE_XBOXLIVE_LOGIN, string redirectURI = NATIVECLIENT_REDIRECT_URI, string? state = null)
        {
            MSAAuthorizeRequest request = new MSAAuthorizeRequest
            {
                client_id = clientID,
                scope = scopes,
                redirect_uri = redirectURI,
                response_type = "code"
            };
            if (state != null)
                request.state = state;
            return await GetAuthorizeURL(request);
        }

        async public static Task<string?> GetAuthorizeURL(MSAAuthorizeRequest request)
        {
            string builtURL = URL_AUTHORIZE + $"?client_id={request.client_id}";
            if (request.scope != null)
                builtURL += $"&scope={HttpUtility.UrlEncode(request.scope)}";
            if (request.redirect_uri != null)
                builtURL += $"&redirect_uri={request.redirect_uri}";
            if (request.response_type != null)
                builtURL += $"&response_type={request.response_type}";
            if (request.state != null)
                builtURL += $"&state={request.state}";
            return builtURL;
        }

        async public static Task<MSACodeRedeemResponse?> RedeemOAuthCode(string oauthCode, string clientID = MINECRAFT_LAUNCHER_CLIENT_ID, string redirectURI = NATIVECLIENT_REDIRECT_URI, string? clientSecret = null, string? state = null)
        {
            MSACodeRedeemRequest request = new MSACodeRedeemRequest
            {
                client_id = clientID,
                code = oauthCode,
                grant_type = "authorization_code",
                redirect_uri = redirectURI
            };
            if (clientSecret != null)
                request.client_secret = clientSecret;
            return await GetOAuthToken(request);
        }
        async public static Task<MSACodeRedeemResponse?> RedeemRefreshToken(string refreshToken, string clientID = MINECRAFT_LAUNCHER_CLIENT_ID, string redirectURI = NATIVECLIENT_REDIRECT_URI, string? clientSecret = null)
        {
            MSACodeRedeemRequest request = new MSACodeRedeemRequest
            {
                client_id = clientID,
                refresh_token = refreshToken,
                grant_type = "refresh_token",
                redirect_uri = redirectURI
            };
            if (clientSecret != null)
                request.client_secret = clientSecret;
            return await GetOAuthToken(request);
        }

        async public static Task<MSACodeRedeemResponse?> GetOAuthToken(MSACodeRedeemRequest request)
        {
            Dictionary<string, string> args = new Dictionary<string, string>();
            if (request.client_id != null)
                args["client_id"] = request.client_id;
            if (request.client_secret != null)
                args["client_secret"] = request.client_secret;
            if (request.code != null)
                args["code"] = request.code;
            if (request.refresh_token != null)
                args["refresh_token"] = request.refresh_token;
            if (request.grant_type != null)
                args["grant_type"] = request.grant_type;
            if (request.redirect_uri != null)
                args["redirect_uri"] = request.redirect_uri;
            FormUrlEncodedContent httpContent = new FormUrlEncodedContent(args);
            HttpResponseMessage response = await client.PostAsync(URL_TOKEN, httpContent);
            if (response.StatusCode != HttpStatusCode.OK)
                return null;
            MSACodeRedeemResponse? codeResponse = await JsonSerializer.DeserializeAsync<MSACodeRedeemResponse>(response.Content.ReadAsStream());
            return codeResponse;
        }
    }
}
