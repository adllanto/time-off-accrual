import { useEffect, useState } from "react";
import { getCredits, updateEarned } from "./creditsApi";

export default function AdminCredits() {
   const [rows, setRows] = useState([]);
   const [loading, setLoading] = useState(true);
   const [error, setError] = useState("");
   const [query, setQuery] = useState("");
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
         setError(err.message); // the server is the real gate
      } finally {
         setSaving(false);
      }
   }

   const filtered = rows.filter((r) =>
      `${r.name} ${r.email}`.toLowerCase().includes(query.toLowerCase())
   );

   return (
      <section className="card">
         <div className="card-head">
            <h2>Agent Credit Overview</h2>
            <input
               className="search"
               placeholder="Search agents..."
               value={query}
               onChange={(e) => setQuery(e.target.value)}
            />
         </div>

         {error && <div className="alert error">{error}</div>}

         {loading ? (
            <div className="spinner" />
         ) : filtered.length === 0 ? (
            <p className="empty">No agents found.</p>
         ) : (
            <div className="table-wrap">
               <table>
                  <thead>
                     <tr>
                        <th>Agent</th>
                        <th>Earned</th>
                        <th>Taken</th>
                        <th>Available</th>
                        <th>Action</th>
                     </tr>
                  </thead>
                  <tbody>
                     {filtered.map((row, i) => (
                        <tr key={row.userId} style={{ "--i": i }}>
                           <td>{row.name}<br /><small>{row.email}</small></td>
                           <td className="num">
                              {editingId === row.userId ? (
                                 <input
                                    className="inline-input"
                                    type="number"
                                    min="0"
                                    step="0.5"
                                    value={draft}
                                    onChange={(e) => setDraft(e.target.value)}
                                 />
                              ) : (
                                 row.earnedHours
                              )}
                           </td>
                           <td className="num">{row.takenHours}</td>
                           <td className="num"><strong>{row.availableHours}</strong></td>
                           <td>
                              {editingId === row.userId ? (
                                 <div className="actions">
                                    <button className="btn primary" onClick={() => save(row)} disabled={saving}>
                                       {saving ? "Saving..." : "Save"}
                                    </button>
                                    <button className="btn ghost" onClick={() => setEditingId(null)} disabled={saving}>
                                       Cancel
                                    </button>
                                 </div>
                              ) : (
                                 <button className="btn" onClick={() => startEdit(row)}>Edit</button>
                              )}
                           </td>
                        </tr>
                     ))}
                  </tbody>
               </table>
            </div>
         )}
      </section>
   );
}