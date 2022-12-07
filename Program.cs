using System;

namespace XSTSPlayground
{
    internal class Program
    {
        static int Main(string[] args) => MainAsync(args).GetAwaiter().GetResult();
        static async Task<int> MainAsync(string[] args)
        {
            // variables
            string? refreshTokenFile = null;
            string? clientID = MSALogin.MINECRAFT_LAUNCHER_CLIENT_ID;
            string? clientSecret = null;
            string? redirectURI = MSALogin.NATIVECLIENT_REDIRECT_URI;
            string? scopes = MSALogin.SCOPE_XBOXLIVE_LOGIN;
            string? xboxRelyingParty = XboxLogin.XBOX_AUTH_RELYINGPARTY;
            string? xboxSiteName = XboxLogin.XBOX_AUTH_SITENAME;
            string? xstsRelyingParty = XboxLogin.MINECRAFTSERVICES_RELYINGPARTY;
            bool printAllTokens = false;

            // parse command line arguments
            for(int i = 0; i < args.Length; i++)
            {
                if (args[i] == "--refresh_file")
                    refreshTokenFile = args[++i];
                if (args[i] == "--client_id")
                    clientID = args[++i];
                if (args[i] == "--client_secret")
                    clientSecret = args[++i];
                if (args[i] == "--redirect_uri")
                    redirectURI = args[++i];
                if (args[i] == "--scopes")
                    scopes = args[++i];
                if (args[i] == "--xbox_rp")
                    xboxRelyingParty = args[++i];
                if (args[i] == "--xbox_site")
                    xboxSiteName = args[++i];
                if (args[i] == "--xsts_rp")
                    xstsRelyingParty = args[++i];
                if (args[i] == "--print_all")
                    printAllTokens = true;
                if (args[i] == "--use_azure_urls")
                {
                    MSALogin.URL_AUTHORIZE = MSALogin.AZURE_API_AUTHORIZE;
                    MSALogin.URL_TOKEN = MSALogin.AZURE_API_TOKEN;
                }
            }

            // if a refresh token file is stored then make sure we're getting a refresh token
            if (refreshTokenFile != null && scopes == MSALogin.SCOPE_XBOXLIVE_LOGIN)
                scopes = MSALogin.SCOPE_XBOXLIVE_LOGIN_AND_REFRESH;

            MSACodeRedeemResponse? response = null;
            // signing into Microsoft
            if (refreshTokenFile != null && File.Exists(refreshTokenFile))
            {
                // try to use a refresh token from the file
                string refreshtoken = File.ReadAllText(refreshTokenFile);
                response = await MSALogin.RedeemRefreshToken(refreshtoken, clientID, redirectURI, clientSecret);
            } else
            {
                // get a URL for the user to type into
                string? authurl = await MSALogin.GetAuthorizeURL(clientID, scopes, redirectURI);
                Console.WriteLine($"Visit this URL in your browser: {authurl}");
                Console.Write($"Paste the 'code' value from the resulting URL here: ");
                string? authcode = Console.ReadLine();
                if (authcode == null)
                {
                    Console.WriteLine("Please specify an auth code.");
                    return -1;
                }
                response = await MSALogin.RedeemOAuthCode(authcode, clientID, redirectURI, clientSecret);
            }

            if (response == null)
            {
                Console.WriteLine("Authentication failed.");
                return -1;
            }
            if (printAllTokens)
                Console.WriteLine($"MSA Access Token: {response.access_token}");
            if (refreshTokenFile != null && response.refresh_token != null)
                File.WriteAllText(refreshTokenFile, response.refresh_token);

            // signing into Xbox Live
            XboxToken? xboxToken = await XboxLogin.GetXboxToken(response.access_token, xboxRelyingParty, xboxSiteName);
            if (xboxToken == null)
            {
                Console.WriteLine("Xbox token login failed.");
                return -1;
            }
            if (printAllTokens)
                Console.WriteLine($"Xbox Live Token: {response.access_token}");

            // signing into XSTS
            XboxToken? xstsToken = await XboxLogin.GetXSTSToken(xboxToken.Token, xstsRelyingParty);
            if (xstsToken == null)
            {
                Console.WriteLine("XSTS login failed.");
                return -1;
            }
            if (printAllTokens)
                Console.Write("XSTS Token: ");

            Console.WriteLine($"XBL3.0 x={xstsToken.UserHash};{xstsToken.Token}");

            return 0;
        }
    }
}
