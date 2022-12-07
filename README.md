# XSTSPlayground

Simple .NET 6.0 tool (and library, i guess?) for fetching XSTS tokens from Xbox Live (for usage with signing into Minecraft Java, and more)

By default the tool will fetch an XSTS token valid for Minecraft Java Edition using the official launcher's client ID to log into Microsoft. This is not intended usage, it is recommended to use your own client ID for anything in production.

*(This was written as a quick and dirty testing tool for research purposes. It's likely broken in some places - if it doesn't work for you, sorry!)*

## Tool Usage

Run the tool and it will ask you to visit a link. After signing in with Microsoft you'll be brought to a blank page - copy everything after "code=" in that URL and paste it into the console window. You'll get an XSTS token valid for use with api.minecraftservices.com.

To use a different client ID for the login process, use the client_id parameter, e.g. `./XSTSPlayground --client_id "YOUR-CLIENT-ID-HERE"` (You must also specify redirect_uri, or allow `https://login.microsoftonline.com/common/oauth2/nativeclient` as a redirect URI in the Azure portal)

To save and use refresh tokens, use the refresh_file parameter, e.g. `./XSTSPlayground --refresh_file "refresh.txt"`.

To specify another XSTS relying party, use the xsts_rp parameter, e.g. `./XSTSPlayground --xsts_rp "https://pocket.realms.minecraft.net/"`

More parameters and behaviours can be found in the source code.
