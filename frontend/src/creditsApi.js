import {
   api
} from "./api";

export const getCredits = () => api("/api/credits");

export const updateEarned = (userId, earnedHours) =>
   api(`/api/credits/${userId}`, {
      method: "PUT",
      body: JSON.stringify({
         earnedHours
      }),
   });