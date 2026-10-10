import { useEffect, useState } from "react";
import { getCredits, updateEarned } from "./creditsApi";

export default function AdminCredits() {
   const [rows, setRows] = useState([]);
   const [loading, setLoading] = useState(true);
   const [error, setError] = useState("");
   const [editingId, setEditingId] = useState(null);
   const [draft, setDraft] = useState("");
   const [saving, setSaving] = useState(false);

   async function load() {
      setError("");
      try {
         setRows(await getCredits());
      } catch (err) {
         setError(err.message);
      } finally {
         setLoading(false);
      }
   }

   useEffect(() => {
      load();
   }, []);

   function startEdit(row) {
      setEditingId(row.userId);
      setDraft(String(row.earnedHours));
      setError("");
   }

   async function save(row) {
      const value = Number(draft);
      if (draft.trim() === "" || Number.isNaN(value) || value < 0) {
         setError("Enter a valid number of hours (0 or more).");
         return;
      }
      if (value < row.takenHours) {
         setError(`Earned hours cannot be less than hours already taken (${row.takenHours}).`);
         return;
      }
      setSaving(true);
      setError("");
      try {
         await updateEarned(row.userId, value);
         setEditingId(null);
         await load();
      } catch (err) {
         setError(err.message); // the server is the real gate; show its message
      } finally {
         setSaving(false);
      }
   }

   if (loading) return <p>Loading credits...</p>;

   return (
      <section>
         <h2>Agent Credit Overview</h2>
         {error && <p style={{ color: "crimson" }}>{error}</p>}
         <table border="1" cellPadding="8" style={{ borderCollapse: "collapse" }}>
            <thead>
               <tr>
                  <th>Agent</th>
                  <th>Earned</th>
                  <th>Taken</th>
                  <th>Available</th>
                  <th></th>
               </tr>
            </thead>
            <tbody>
               {rows.map((row) => (
                  <tr key={row.userId}>
                     <td>{row.name}<br /><small>{row.email}</small></td>
                     <td>
                        {editingId === row.userId ? (
                           <input
                              type="number"
                              min="0"
                              step="0.5"
                              value={draft}
                              onChange={(e) => setDraft(e.target.value)}
                              style={{ width: 80 }}
                           />
                        ) : (
                           row.earnedHours
                        )}
                     </td>
                     <td>{row.takenHours}</td>
                     <td>{row.availableHours}</td>
                     <td>
                        {editingId === row.userId ? (
                           <>
                              <button onClick={() => save(row)} disabled={saving}>
                                 {saving ? "Saving..." : "Save"}
                              </button>{" "}
                              <button onClick={() => setEditingId(null)} disabled={saving}>Cancel</button>
                           </>
                        ) : (
                           <button onClick={() => startEdit(row)}>Edit</button>
                        )}
                     </td>
                  </tr>
               ))}
            </tbody>
         </table>
      </section>
   );
}