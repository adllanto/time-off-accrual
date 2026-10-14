import { useState } from "react";
import { useAuth } from "./AuthContext";
import LoginPage from "./LoginPage";
import AdminCredits from "./AdminCredits";
import AdminQueue from "./AdminQueue";
import AgentRequests from "./AgentRequests";
import Icon from "./Icon";

const NAV = {
  Admin: [
    { id: "credits", label: "Credits", icon: "people" },
    { id: "approvals", label: "Approvals", icon: "check" },
  ],
  Agent: [{ id: "request", label: "Time Off", icon: "calendar" }],
};



export default function App() {
  const { user, logout } = useAuth();
  const [page, setPage] = useState(null);

  function handleLogout() {
    setPage(null);
    logout();
  }

  if (!user) return <LoginPage />;

  const items = NAV[user.role] ?? [];
  const current = items.some((i) => i.id === page) ? page : items[0]?.id;

  return (
    <div className="shell">
      <aside className="sidebar">
        <div className="brand">TIME OFF</div>
          {
            items.map((item) => (
              <button
                key={item.id}
                className={`nav-item ${current === item.id ? "active" : ""}`}
                onClick={() => setPage(item.id)}
              >
                <Icon name={item.icon} />
                {item.label}
              </button>
            ))
          }
        <div className="sidebar-bottom">
          <button className="nav-item" onClick={handleLogout}>
            <Icon name="logout" />
            Logout
          </button>
        </div>
      </aside>

      <main className="main">
        <div className="topbar">
          <span className="user-chip">{user.name} · {user.role}</span>
        </div>

        <div className="page" key={current}>
          {current === "credits" && <AdminCredits />}
          {current === "approvals" && <AdminQueue />}
          {current === "request" && <AgentRequests />}
        </div>
      </main>
    </div>
  );
}