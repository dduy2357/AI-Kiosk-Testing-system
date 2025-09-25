import { AUTH_URL, GEET_LINK_GOOGLE, LOGIN_GOOGLE } from "@/consts/apiUrl";
import httpService from "@/services/httpService";

class GoogleLoginService {
  // Get Google OAuth URL
  getLinkGoogle() {
    return httpService.get(`${GEET_LINK_GOOGLE}`);
  }

  // Call google-callback API
  loginGoogle(code: string, campusId: string) {
    return httpService.post(
      `${LOGIN_GOOGLE}`,
      {
        credential: code,
        campusId,
      },
      {
        headers: {
          "Content-Type": "application/json",
        },
      },
    );
  }

  loginWithAdmin(email: string, password: string, campusId: string) {
    return httpService.post(`${AUTH_URL}/Login`, {
      email,
      password,
      campusId,
    });
  }

  logOut(userid: string) {
    return httpService.get(`${AUTH_URL}/logout?userId=${userid}`);
  }
}

export default new GoogleLoginService();
