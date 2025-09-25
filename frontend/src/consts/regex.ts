export const regexCommon = {
  regexStrongPassword: /(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}/,
  regexPhone: /^\d{10}$/,
};

export const dobRegex =
  /^(?:(?:31\/(?:0[13578]|1[02]))|(?:29|30\/(?:0[13-9]|1[0-2]))|(?:0[1-9]|1\d|2[0-8])\/(?:0[1-9]|1[0-2]))\/(?:19|20)\d{2}$|^29\/02\/(?:19|20)(?:[02468][048]|[13579][26])$/;
export const emailRegex = /^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$/;
export const phoneRegex = /^0\d{9}$/;
