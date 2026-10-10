import { useAuth } from "./AuthContext";
import LoginPage from "./LoginPage";

export default function App() {
  const { user, logout } = useAuth();

  if (!user) return <LoginPage />;

  return (
    <div style={{ padding: 24 }}>
      <header style={{ display: "flex", justifyContent: "space-between" }}>
        <strong>{user.name} ({user.role})</strong>
        <button onClick={logout}>Log out</button>
      </header>
      <p>Logged in. Pages for {user.role}s come next.</p>
    </div>
  );
}