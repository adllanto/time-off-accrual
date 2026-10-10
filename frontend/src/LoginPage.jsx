import { useState } from "react";
import { useAuth } from "./AuthContext";

export default function LoginPage() {
  const { login } = useAuth();
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [error, setError] = useState("");
  const [busy, setBusy] = useState(false);

  async function handleSubmit(e) {
    e.preventDefault();
    setError("");
    setBusy(true);
    try {
      await login(email, password);
    } catch (err) {
      setError(err.message);
    } finally {
      setBusy(false);
    }
  }

  return (
    <form onSubmit={handleSubmit} style={{ maxWidth: 320, margin: "80px auto" }}>
      <h1>Time Off Accrual</h1>
      <label>Email<br />
        <input type="email" value={email} onChange={(e) => setEmail(e.target.value)} required />
      </label>
      <br /><br />
      <label>Password<br />
        <input type="password" value={password} onChange={(e) => setPassword(e.target.value)} required />
      </label>
      <br /><br />
      {error && <p style={{ color: "crimson" }}>{error}</p>}
      <button type="submit" disabled={busy}>{busy ? "Signing in..." : "Sign in"}</button>
    </form>
  );
}