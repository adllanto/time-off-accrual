import { useAuth } from "./AuthContext";
import LoginPage from "./LoginPage";
import AdminCredits from "./AdminCredits";

export default function App() {
  const { user, logout } = useAuth();

  if (!user) return <LoginPage />;

  return (
    <div style={{ padding: 24 }}>
      <header style={{ display: "flex", justifyContent: "space-between" }}>
        <strong>{user.name} ({user.role})</strong>
        <button onClick={logout}>Log out</button>
      </header>

      {user.role === "Admin" ? (
        <AdminCredits />
      ) : (
        <p>Agent pages come next.</p>
      )}
    </div>
  );
}