import { createContext, useContext, useState } from "react";
import { api } from "./api";

const AuthContext = createContext(null);

export function AuthProvider({ children }) {
  const [user, setUser] = useState(() => {
    const saved = sessionStorage.getItem("user");
    return saved ? JSON.parse(saved) : null;
  });

  async function login(email, password) {
    const data = await api("/api/auth/login", {
      method: "POST",
      body: JSON.stringify({ email, password }),
    });
    sessionStorage.setItem("token", data.token);
    const info = { userId: data.userId, name: data.name, role: data.role };
    sessionStorage.setItem("user", JSON.stringify(info));
    setUser(info);
  }

  function logout() {
    sessionStorage.clear();
    setUser(null);
  }

  return (
    <AuthContext.Provider value={{ user, login, logout }}>
      {children}
    </AuthContext.Provider>
  );
}

export const useAuth = () => useContext(AuthContext);