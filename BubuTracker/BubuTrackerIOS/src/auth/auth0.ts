import * as AuthSession from "expo-auth-session";
import * as WebBrowser from "expo-web-browser";
import { AUTH0_AUDIENCE, AUTH0_CLIENT_ID, AUTH0_DOMAIN } from "../config";
import { clearSession, getRefreshToken, saveAccessToken, saveCredentials } from "./session";

WebBrowser.maybeCompleteAuthSession();

const discovery: AuthSession.DiscoveryDocument = {
  authorizationEndpoint: `https://${AUTH0_DOMAIN}/authorize`,
  tokenEndpoint: `https://${AUTH0_DOMAIN}/oauth/token`,
  revocationEndpoint: `https://${AUTH0_DOMAIN}/oauth/revoke`,
  userInfoEndpoint: `https://${AUTH0_DOMAIN}/userinfo`,
};

// Must be listed in Auth0 Allowed Callback / Logout URLs (LoginScreen prints it).
export function getRedirectUri(): string {
  return AuthSession.makeRedirectUri();
}

export interface LoginResult {
  accessToken: string;
  refreshToken: string | null;
}

export async function login(): Promise<LoginResult | null> {
  const redirectUri = getRedirectUri();

  const request = new AuthSession.AuthRequest({
    clientId: AUTH0_CLIENT_ID,
    redirectUri,
    // offline_access required for Auth0 to issue a refresh token.
    scopes: ["openid", "profile", "email", "offline_access"],
    responseType: AuthSession.ResponseType.Code,
    extraParams: { audience: AUTH0_AUDIENCE },
    usePKCE: true,
  });

  const result = await request.promptAsync(discovery);
  if (result.type !== "success" || !result.params.code) {
    return null;
  }

  const tokenResponse = await AuthSession.exchangeCodeAsync(
    {
      clientId: AUTH0_CLIENT_ID,
      code: result.params.code,
      redirectUri,
      extraParams: {
        code_verifier: request.codeVerifier ?? "",
      },
    },
    discovery
  );

  const accessToken = tokenResponse.accessToken;
  const refreshToken = tokenResponse.refreshToken ?? null;
  await saveCredentials(accessToken, refreshToken);
  return { accessToken, refreshToken };
}

export async function logout(): Promise<void> {
  await clearSession();
  const redirectUri = getRedirectUri();
  const logoutUrl = `https://${AUTH0_DOMAIN}/v2/logout?client_id=${AUTH0_CLIENT_ID}&returnTo=${encodeURIComponent(
    redirectUri
  )}`;
  try {
    await WebBrowser.openAuthSessionAsync(logoutUrl, redirectUri);
  } catch {
  }
}

export async function refreshAccessToken(): Promise<string | null> {
  const refreshToken = await getRefreshToken();
  if (!refreshToken) {
    return null;
  }
  try {
    const result = await AuthSession.refreshAsync(
      { clientId: AUTH0_CLIENT_ID, refreshToken },
      discovery
    );
    await saveAccessToken(result.accessToken);
    return result.accessToken;
  } catch {
    return null;
  }
}
