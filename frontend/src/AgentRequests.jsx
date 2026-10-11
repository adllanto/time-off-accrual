import { useEffect, useState } from "react";
import { getMyBalance, getMyRequests, createRequest } from "./requestsApi";
import StatusBadge from "./StatusBadge";

const HOURS_PER_DAY = 8;

function daysBetween(start, end) {
   const ms = new Date(end) - new Date(start);
   return Math.round(ms / 86400000) + 1;
}

export default function AgentRequests() {
   const [balance, setBalance] = useState(null);
   const [requests, setRequests] = useState([]);
   const [loading, setLoading] = useState(true);
   const [error, setError] = useState("");
   const [success, setSuccess] = useState("");
   const [busy, setBusy] = useState(false);

   const [startDate, setStartDate] = useState("");
   const [endDate, setEndDate] = useState("");
   const [durationType, setDurationType] = useState("Full");
   const [hours, setHours] = useState("");

   const today = new Date().toISOString().slice(0, 10);

   async function load() {
      try {
         const [b, r] = await Promise.all([getMyBalance(), getMyRequests()]);
         setBalance(b);
         setRequests(r);
      } catch (err) {
         setError(err.message);
      } finally {
         setLoading(false);
      }
   }

   useEffect(() => {
      load();
   }, []);

   // Partial requests are a single date, so keep end = start
   const effectiveEnd = durationType === "Partial" ? startDate : endDate;

   let requestedHours = 0;
   if (startDate && effectiveEnd && effectiveEnd >= startDate) {
      requestedHours =
         durationType === "Full"
            ? HOURS_PER_DAY * daysBetween(startDate, effectiveEnd)
            : Number(hours) || 0;
   }

   const available = balance?.availableHours ?? 0;
   const overBalance = requestedHours > available;

   async function handleSubmit(e) {
      e.preventDefault();
      setError("");
      setSuccess("");
      setBusy(true);
      try {
         await createRequest({
            startDate,
            endDate: effectiveEnd,
            durationType,
            hours: durationType === "Partial" ? Number(hours) : null,
         });
         setSuccess("Request submitted. It is now pending approval.");
         setStartDate("");
         setEndDate("");
         setHours("");
         await load();
      } catch (err) {
         setError(err.message); // the server is the real gate
      } finally {
         setBusy(false);
      }
   }

   if (loading) return <div className="spinner" />;

   return (
      <>
         <div className="stats">
            <div className="stat" style={{ "--i": 0 }}>
               <div className="label">Earned</div>
               <div className="value">{balance?.earnedHours}<small> hrs</small></div>
            </div>
            <div className="stat accent" style={{ "--i": 1 }}>
               <div className="label">Taken</div>
               <div className="value">{balance?.takenHours}<small> hrs</small></div>
            </div>
            <div className="stat good" style={{ "--i": 2 }}>
               <div className="label">Available</div>
               <div className="value">{available}<small> hrs</small></div>
            </div>
         </div>

         <div className="grid-2">
            <form className="card" onSubmit={handleSubmit}>
               <div className="card-head"><h2>Request Time Off</h2></div>

               <label className="field">Type
                  <select value={durationType} onChange={(e) => setDurationType(e.target.value)}>
                     <option value="Full">Full day (8 hours per day)</option>
                     <option value="Partial">Partial day</option>
                  </select>
               </label>

               <label className="field">{durationType === "Partial" ? "Date" : "Start date"}
                  <input type="date" min={today} value={startDate}
                     onChange={(e) => setStartDate(e.target.value)} required />
               </label>

               {durationType === "Full" && (
                  <label className="field">End date
                     <input type="date" min={startDate || today} value={endDate}
                        onChange={(e) => setEndDate(e.target.value)} required />
                  </label>
               )}

               {durationType === "Partial" && (
                  <label className="field">Hours (0.5 to 8)
                     <input type="number" min="0.5" max="8" step="0.5" value={hours}
                        onChange={(e) => setHours(e.target.value)} required />
                  </label>
               )}

               <p>Requested: <strong>{requestedHours} hours</strong></p>
               {overBalance && <div className="alert warn">This exceeds your available balance.</div>}
               {error && <div className="alert error">{error}</div>}
               {success && <div className="alert success">{success}</div>}

               <button className="btn primary block" type="submit"
                  disabled={busy || overBalance || requestedHours <= 0}>
                  {busy ? "Submitting..." : "Submit request"}
               </button>
            </form>

            <section className="card">
               <div className="card-head"><h2>My Requests</h2></div>
               {requests.length === 0 ? (
                  <p className="empty">No requests yet.</p>
               ) : (
                  <div className="table-wrap">
                     <table>
                        <thead>
                           <tr><th>Dates</th><th>Type</th><th>Hours</th><th>Status</th></tr>
                        </thead>
                        <tbody>
                           {requests.map((r, i) => (
                              <tr key={r.id} style={{ "--i": i }}>
                                 <td>{r.startDate === r.endDate ? r.startDate : `${r.startDate} to ${r.endDate}`}</td>
                                 <td>{r.durationType}</td>
                                 <td className="num">{r.hours}</td>
                                 <td><StatusBadge status={r.status} /></td>
                              </tr>
                           ))}
                        </tbody>
                     </table>
                  </div>
               )}
            </section>
         </div>
      </>
   );
}