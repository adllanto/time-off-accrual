import { useEffect } from "react";
import { createPortal } from "react-dom";

export default function ConfirmDialog({
   title,
   message,
   confirmLabel = "Confirm",
   variant = "primary",
   busy = false,
   onConfirm,
   onCancel,
}) {
   // Close on Escape
   useEffect(() => {
      function onKey(e) {
         if (e.key === "Escape" && !busy) onCancel();
      }
      window.addEventListener("keydown", onKey);
      return () => window.removeEventListener("keydown", onKey);
   }, [busy, onCancel]);

   return createPortal(
      <div className="modal-backdrop" onClick={() => !busy && onCancel()}>
         <div
            className="modal"
            role="dialog"
            aria-modal="true"
            aria-labelledby="confirm-title"
            onClick={(e) => e.stopPropagation()}
         >
            <h3 id="confirm-title">{title}</h3>
            <p>{message}</p>
            <div className="actions modal-actions">
               <button className="btn ghost" onClick={onCancel} disabled={busy}>
                  Cancel
               </button>
               <button className={`btn ${variant}`} onClick={onConfirm} disabled={busy}>
                  {busy ? "Working..." : confirmLabel}
               </button>
            </div>
         </div>
      </div>,
      document.body
   );
}