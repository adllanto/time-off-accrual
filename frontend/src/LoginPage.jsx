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
    <div className="login-wrap">
      <form className="card login-card" onSubmit={handleSubmit}>
        <div className="brand">TIME OFF</div>
        <p className="sub">Sign in to manage your time-off credits.</p>

        <label className="field">Email
          <input type="email" value={email} onChange={(e) => setEmail(e.target.value)} required />
        </label>
        <label className="field">Password
          <input type="password" value={password} onChange={(e) => setPassword(e.target.value)} required />
        </label>

        {error && <div className="alert error">{error}</div>}
        <button className="btn primary block" type="submit" disabled={busy}>
          {busy ? "Signing in..." : "Sign in"}
        </button>
      </form>
    </div>
  );
}