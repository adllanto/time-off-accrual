import { useEffect, useState } from "react";
import { getRequestsByStatus, approveRequest, denyRequest } from "./requestsApi";
import StatusBadge from "./StatusBadge";

const STATUSES = ["Pending", "Approved", "Denied"];

export default function AdminQueue() {
   const [status, setStatus] = useState("Pending");
   const [requests, setRequests] = useState([]);
   const [loading, setLoading] = useState(true);
   const [error, setError] = useState("");
   const [busyId, setBusyId] = useState(null);

   async function load(which = status) {
      try {
         setRequests(await getRequestsByStatus(which));
      } catch (err) {
         setError(err.message);
      } finally {
         setLoading(false);
      }
   }

   useEffect(() => {
      setLoading(true);
      setError("");
      load(status);
   }, [status]);

   async function decide(id, action) {
      setError("");
      setBusyId(id);
      try {
         await (action === "approve" ? approveRequest(id) : denyRequest(id));
      } catch (err) {
         setError(err.message); // e.g. "no longer has enough available balance"
      } finally {
         await load(); // refresh either way, the list may be stale
         setBusyId(null);
      }
   }

   return (
      <section className="card">
         <div className="card-head">
            <h2>Time-Off Requests</h2>
         </div>

         <div className="tabs">
            {STATUSES.map((s) => (
               <button
                  key={s}
                  className={`tab ${status === s ? "active" : ""}`}
                  onClick={() => setStatus(s)}
               >
                  {s}
               </button>
            ))}
         </div>

         {error && <div className="alert error">{error}</div>}

         {loading ? (
            <div className="spinner" />
         ) : requests.length === 0 ? (
            <p className="empty">No {status.toLowerCase()} requests.</p>
         ) : (
            <div className="table-wrap">
               <table>
                  <thead>
                     <tr>
                        <th>Agent</th>
                        <th>Dates</th>
                        <th>Type</th>
                        <th>Hours</th>
                        <th>Status</th>
                        {status === "Pending" && <th>Action</th>}
                     </tr>
                  </thead>
                  <tbody>
                     {requests.map((r, i) => (
                        <tr key={r.id} style={{ "--i": i }}>
                           <td>{r.userName}</td>
                           <td>{r.startDate === r.endDate ? r.startDate : `${r.startDate} to ${r.endDate}`}</td>
                           <td>{r.durationType}</td>
                           <td className="num">{r.hours}</td>
                           <td><StatusBadge status={r.status} /></td>
                           {status === "Pending" && (
                              <td>
                                 <div className="actions">
                                    <button className="btn success" disabled={busyId === r.id}
                                       onClick={() => decide(r.id, "approve")}>Approve</button>
                                    <button className="btn danger" disabled={busyId === r.id}
                                       onClick={() => decide(r.id, "deny")}>Deny</button>
                                 </div>
                              </td>
                           )}
                        </tr>
                     ))}
                  </tbody>
               </table>
            </div>
         )}
      </section>
   );
}