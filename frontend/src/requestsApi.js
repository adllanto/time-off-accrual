import {
   api
} from "./api";

export const getMyBalance = () => api("/api/requests/balance");
export const getMyRequests = () => api("/api/requests/mine");

export const createRequest = (payload) =>
   api("/api/requests", {
      method: "POST",
      body: JSON.stringify(payload)
   });