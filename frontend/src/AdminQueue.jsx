import { useEffect, useState } from "react";
import { getPendingRequests, approveRequest, denyRequest } from "./requestsApi";

export default function AdminQueue({ onDecided }) {
   const [requests, setRequests] = useState([]);
   const [loading, setLoading] = useState(true);
   const [error, setError] = useState("");
   const [busyId, setBusyId] = useState(null);

   async function load() {
      try {
         setRequests(await getPendingRequests());
      } catch (err) {
         setError(err.message);
      } finally {
         setLoading(false);
      }
   }

   useEffect(() => {
      load();
   }, []);

   async function decide(id, action) {
      setError("");
      setBusyId(id);
      try {
         await (action === "approve" ? approveRequest(id) : denyRequest(id));
         await load();
         onDecided?.(); // tell the parent to refresh the credit table
      } catch (err) {
         setError(err.message); // e.g. "no longer has enough available balance"
         await load();          // the list may be stale, so refresh it
      } finally {
         setBusyId(null);
      }
   }

   if (loading) return <p>Loading requests...</p>;

   return (
      <section style={{ marginTop: 32 }}>
         <h2>Pending Requests</h2>
         {error && <p style={{ color: "crimson" }}>{error}</p>}
         {requests.length === 0 ? (
            <p>No pending requests.</p>
         ) : (
            <table border="1" cellPadding="8" style={{ borderCollapse: "collapse" }}>
               <thead>
                  <tr><th>Agent</th><th>Dates</th><th>Type</th><th>Hours</th><th></th></tr>
               </thead>
               <tbody>
                  {requests.map((r) => (
                     <tr key={r.id}>
                        <td>{r.userName}</td>
                        <td>{r.startDate === r.endDate ? r.startDate : `${r.startDate} to ${r.endDate}`}</td>
                        <td>{r.durationType}</td>
                        <td>{r.hours}</td>
                        <td>
                           <button onClick={() => decide(r.id, "approve")} disabled={busyId === r.id}>
                              Approve
                           </button>{" "}
                           <button onClick={() => decide(r.id, "deny")} disabled={busyId === r.id}>
                              Deny
                           </button>
                        </td>
                     </tr>
                  ))}
               </tbody>
            </table>
         )}
      </section>
   );
}