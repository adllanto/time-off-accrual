import { useState } from "react";
import { useAuth } from "./AuthContext";
import LoginPage from "./LoginPage";
import AdminCredits from "./AdminCredits";
import AdminQueue from "./AdminQueue";
import AgentRequests from "./AgentRequests";

export default function App() {
  const { user, logout } = useAuth();
  const [creditsVersion, setCreditsVersion] = useState(0);

  if (!user) return <LoginPage />;

  return (
    <div style={{ padding: 24 }}>
      <header style={{ display: "flex", justifyContent: "space-between" }}>
        <strong>{user.name} ({user.role})</strong>
        <button onClick={logout}>Log out</button>
      </header>

      {user.role === "Admin" ? (
        <>
          <AdminCredits key={creditsVersion} />
          <AdminQueue onDecided={() => setCreditsVersion((v) => v + 1)} />
        </>
      ) : (
        <AgentRequests />
      )}
    </div>
  );
}