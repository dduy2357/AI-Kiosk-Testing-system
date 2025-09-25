import { isString } from "lodash";

export const errorHandler = (error: any) => {
  if (error?.message === "canceled") {
    return null;
  }

  if (isString(error)) {
    return error;
  }

  return error;
};
