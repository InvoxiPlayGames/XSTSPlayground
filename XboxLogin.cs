using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace XSTSPlayground
{
    public class XSTSAuthProperties
    {
        public string? SandboxId { get; set; }
        public string[]? UserTokens { get; set; }
    }

    public class XboxAuthRequest
    {
        public dynamic? Properties { get; set; }
        public string? RelyingParty { get; set; }
        public string? TokenType { get; set; }
    }

    public class XboxAuthResponse
    {
        public string? IssueInstant { get; set; }
        public string? NotAfter { get; set; }
        public string? Token { get; set; }
        public XboxDisplayClaims? DisplayClaims { get; set; }
    }

    public class XboxAuthError
    {
        public string? Identity { get; set; }
        public int XErr { get; set; }
        public string? Message { get; set; }
        public string? Redirect { get; set; }
    }

    public class XboxDisplayClaims
    {
        public XboxXUI[]? xui { get; set; }
    }

    public class XboxXUI
    {
        public string? uhs { get; set; }
    }

    public class XboxToken
    {
        public string? Token { get; set; }
        public string? UserHash { get; set; }
    }

    public class XboxLogin
    {
        // URL bases
        public const string XBOX_USER_AUTH_API = "https://user.auth.xboxlive.com/user/authenticate";
        public const string XBOX_XSTS_AUTH_API = "https://xsts.auth.xboxlive.com/xsts/authorize";

        // xbox relying parties
        public const string XBOX_AUTH_RELYINGPARTY = "http://auth.xboxlive.com";
        public const string XBOX_AUTH_SITENAME = "user.auth.xboxlive.com";
        // xsts relying parties
        public const string MINECRAFTSERVICES_RELYINGPARTY = "rp://api.minecraftservices.com/";
        public const string MINECRAFTREALMS_RELYINGPARTY = "https://pocket.realms.minecraft.net/";

        // make ourselves a HTTP client
        private static HttpClient client = new HttpClient();

        // give ourselves some json option
        private static JsonSerializerOptions opts = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

    async public static Task<XboxToken?> GetXboxToken(string msaToken, string relyingParty = XBOX_AUTH_RELYINGPARTY, string siteName = XBOX_AUTH_SITENAME)
        {
            XboxAuthRequest request = new XboxAuthRequest
            {
                Properties = new Dictionary<string, string> {
                    { "AuthMethod", "RPS" },
                    { "SiteName", siteName },
                    { "RpsTicket", $"d={msaToken}" },
                },
                RelyingParty = relyingParty,
                TokenType = "JWT"
            };
            JsonContent httpContent = JsonContent.Create(request, request.GetType(), null, opts);
            HttpResponseMessage response = await client.PostAsync(XBOX_USER_AUTH_API, httpContent);
            if (response.StatusCode != HttpStatusCode.OK)
                return null;
            XboxAuthResponse? codeResponse = await JsonSerializer.DeserializeAsync<XboxAuthResponse>(response.Content.ReadAsStream());
            if (codeResponse == null)
                return null;
            return new XboxToken { Token = codeResponse.Token, UserHash = codeResponse.DisplayClaims.xui[0].uhs };
        }
        async public static Task<XboxToken?> GetXSTSToken(string xboxToken, string relyingParty = MINECRAFTSERVICES_RELYINGPARTY)
        {
            XboxAuthRequest request = new XboxAuthRequest
            {
                Properties = new XSTSAuthProperties
                {
                    SandboxId = "RETAIL",
                    UserTokens = new string[] { xboxToken }
                },
                RelyingParty = relyingParty,
                TokenType = "JWT"
            };
            JsonContent httpContent = JsonContent.Create(request, request.GetType(), null, opts);
            HttpResponseMessage response = await client.PostAsync(XBOX_XSTS_AUTH_API, httpContent);
            if (response.StatusCode != HttpStatusCode.OK)
                return null;
            XboxAuthResponse? codeResponse = await JsonSerializer.DeserializeAsync<XboxAuthResponse>(response.Content.ReadAsStream());
            if (codeResponse == null)
                return null;
            return new XboxToken { Token = codeResponse.Token, UserHash = codeResponse.DisplayClaims.xui[0].uhs };
        }
    }
}
