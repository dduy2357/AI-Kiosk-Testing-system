import BaseUrl from '@/consts/baseUrl';
import cachedKeys from '@/consts/cachedKeys';
import { ROLE_ENUM } from '@/consts/role';
import { showError, showSuccess } from '@/helpers/toast';
import { KEY_LANG } from '@/i18n/config';
import { UserInfo } from '@/interfaces/user';
import httpService from '@/services/httpService';
import googleloginService from '@/services/modules/googlelogin/googlelogin.service';
import { useSave } from '@/stores/useStores';
import axios from 'axios';
import { createContext, useCallback, useContext, useEffect, useMemo, useState } from 'react';
import { useNavigate } from 'react-router-dom';

interface AuthenticationContextI {
  loading: boolean;
  isLogged: boolean;
  user: UserInfo | null;
  login: ({ data }: { data: any }) => void;
  loginWithAccount: ({
    username,
    password,
  }: {
    username: string;
    password: string;
  }) => Promise<void>;
  logout: (userID: string) => Promise<void>;
  isAdmin: boolean;
  isUser: boolean;
  setIsLogging: (isLogging: boolean) => void;
}

const AuthenticationContext = createContext<AuthenticationContextI>({
  loading: false,
  isLogged: false,
  user: {} as any,
  login: () => {},
  loginWithAccount: async () => {},
  logout: async () => {},
  isAdmin: false,
  isUser: false,
  setIsLogging: () => {},
});

export const useAuth = () => useContext(AuthenticationContext);

const AuthenticationProvider = ({ children }: { children: any }) => {
  //! State
  const [token, setToken] = useState<string | null>(httpService.getTokenStorage());
  const [user, setUser] = useState<UserInfo | null>(httpService.getUserStorage());
  const [isLogging, setIsLogging] = useState(false);
  const save = useSave();
  const navigate = useNavigate();

  //! BroadcastChannel - listen to logout events
  useEffect(() => {
    const channel = new BroadcastChannel('auth');

    channel.onmessage = (event) => {
      if (event.data.type === 'logout') {
        httpService.clearStorage();
        window.sessionStorage.clear();
        setUser(null);
        setToken(null);
        navigate(BaseUrl.Login, { replace: true });
      }
    };

    return () => {
      channel.close();
    };
  }, [navigate]);

  //! Function
  const login = useCallback(({ data }: { data: any }) => {
    setIsLogging(true);
    if (data && data.accessToken) {
      const token = data.accessToken;
      const user = data.data;

      setToken(token);
      setUser(user);

      httpService.attachTokenToHeader(token);
      httpService.saveTokenStorage(token);
      httpService.saveUserStorage(user);
      showSuccess('Login successfully!');

      // Send message to webview
      if (window.chrome && window.chrome.webview) {
        const message = {
          event: 'token',
          token: token,
        };
        window.chrome.webview.postMessage(message);
      }

      setIsLogging(false);
    }
  }, []);

  const loginWithAccount = useCallback(
    async ({ username, password }: { username: string; password: string }) => {
      setIsLogging(true);
      try {
        const res = await googleloginService.loginWithAdmin(
          username,
          password,
          '11111111-aaaa-bbbb-cccc-111111111111',
        );
        if (res.status === 200) {
          const token = res.data.data.accessToken;
          const user = res.data.data.data;
          setToken(token);
          save(cachedKeys.token, token);
          setUser(user);
          httpService.attachTokenToHeader(token);
          httpService.saveTokenStorage(token);
          httpService.saveUserStorage(user);
          localStorage.setItem(KEY_LANG, 'en');

          // Send message to webview
          if (window.chrome && window.chrome.webview) {
            const message = {
              event: 'token',
              token: token,
            };
            window.chrome.webview.postMessage(message);
          }

          showSuccess('Login successfully!');
          setIsLogging(false);
          return user;
        }
      } catch (error) {
        if (axios.isAxiosError(error)) {
          showError(error.response?.data?.message || 'Login failed. Please try again.');
        } else {
          showError('Login failed. Please try again.');
        }
      } finally {
        setIsLogging(false);
      }
      return null;
    },
    [save],
  );

  const logout = useCallback(
    async (userID: string) => {
      try {
        await googleloginService.logOut(userID || '');
        httpService.clearStorage();
        window.sessionStorage.clear();
        setUser(null);
        setToken(null);

        // Send logout signal to other tabs
        const channel = new BroadcastChannel('auth');
        channel.postMessage({ type: 'logout' });
        channel.close();

        if (user?.roleId?.includes(ROLE_ENUM.ADMIN)) {
          navigate(BaseUrl.AdminLogin, { replace: true });
        } else {
          navigate(BaseUrl.Login, { replace: true });
        }
        showSuccess('You have been successfully logged out.');
      } catch (error) {
        showError(error);
        navigate(BaseUrl.Login, { replace: true });
      }
    },
    [navigate, user?.roleId],
  );

  //! Return
  const value = useMemo(() => {
    return {
      loading: isLogging,
      isLogged: !!user && !!token,
      user,
      logout,
      login,
      loginWithAccount,
      isAdmin: !!user?.roleId?.includes(ROLE_ENUM.ADMIN),
      isUser: !!user?.roleId?.includes(ROLE_ENUM.Student),
      setIsLogging,
    };
  }, [login, loginWithAccount, logout, user, token, isLogging]);

  return <AuthenticationContext.Provider value={value}>{children}</AuthenticationContext.Provider>;
};

export default AuthenticationProvider;
