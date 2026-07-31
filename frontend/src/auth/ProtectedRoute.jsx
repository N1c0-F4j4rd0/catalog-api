import { Navigate } from "react-router-dom";
import { useAuth } from "./AuthContext";

// El "AuthGuard": si no hay token, redirige al login
export default function ProtectedRoute({ children }) {
  const { token } = useAuth();
  return token ? children : <Navigate to="/login" replace />;
}
