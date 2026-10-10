import { useEffect, useState } from "react";
import { getMyBalance, getMyRequests, createRequest } from "./requestsApi";

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
         setError(err.message); // server is the real gate; show its message
      } finally {
         setBusy(false);
      }
   }

   if (loading) return <p>Loading...</p>;

   return (
      <section>
         <h2>Request Time Off</h2>
         <p>
            Available balance: <strong>{available} hours</strong>{" "}
            <small>(Earned {balance?.earnedHours}, Taken {balance?.takenHours})</small>
         </p>

         <form onSubmit={handleSubmit} style={{ maxWidth: 360 }}>
            <label>Type<br />
               <select value={durationType} onChange={(e) => setDurationType(e.target.value)}>
                  <option value="Full">Full day (8 hours per day)</option>
                  <option value="Partial">Partial day</option>
               </select>
            </label>
            <br /><br />

            <label>{durationType === "Partial" ? "Date" : "Start date"}<br />
               <input type="date" min={today} value={startDate}
                  onChange={(e) => setStartDate(e.target.value)} required />
            </label>
            <br /><br />

            {durationType === "Full" && (
               <>
                  <label>End date<br />
                     <input type="date" min={startDate || today} value={endDate}
                        onChange={(e) => setEndDate(e.target.value)} required />
                  </label>
                  <br /><br />
               </>
            )}

            {durationType === "Partial" && (
               <>
                  <label>Hours (0.5 to 8)<br />
                     <input type="number" min="0.5" max="8" step="0.5" value={hours}
                        onChange={(e) => setHours(e.target.value)} required />
                  </label>
                  <br /><br />
               </>
            )}

            <p>Requested: <strong>{requestedHours} hours</strong></p>
            {overBalance && (
               <p style={{ color: "crimson" }}>This exceeds your available balance.</p>
            )}
            {error && <p style={{ color: "crimson" }}>{error}</p>}
            {success && <p style={{ color: "green" }}>{success}</p>}

            <button type="submit" disabled={busy || overBalance || requestedHours <= 0}>
               {busy ? "Submitting..." : "Submit request"}
            </button>
         </form>

         <h2 style={{ marginTop: 32 }}>My Requests</h2>
         {requests.length === 0 ? (
            <p>No requests yet.</p>
         ) : (
            <table border="1" cellPadding="8" style={{ borderCollapse: "collapse" }}>
               <thead>
                  <tr><th>Dates</th><th>Type</th><th>Hours</th><th>Status</th></tr>
               </thead>
               <tbody>
                  {requests.map((r) => (
                     <tr key={r.id}>
                        <td>{r.startDate === r.endDate ? r.startDate : `${r.startDate} to ${r.endDate}`}</td>
                        <td>{r.durationType}</td>
                        <td>{r.hours}</td>
                        <td>{r.status}</td>
                     </tr>
                  ))}
               </tbody>
            </table>
         )}
      </section>
   );
}