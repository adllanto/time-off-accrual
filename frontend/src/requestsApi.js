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

   export const getPendingRequests = () => api("/api/requests?status=Pending");

   export const approveRequest = (id) =>
      api(`/api/requests/${id}/approve`, {
         method: "POST"
      });

   export const denyRequest = (id) =>
      api(`/api/requests/${id}/deny`, {
         method: "POST"
      });