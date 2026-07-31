import { useForm } from "react-hook-form";
import { useNavigate } from "react-router-dom";
import { useAuth } from "../auth/AuthContext";

const IconUser = () => (
  <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
    <path d="M20 21v-2a4 4 0 0 0-4-4H8a4 4 0 0 0-4 4v2" /><circle cx="12" cy="7" r="4" />
  </svg>
);
const IconLock = () => (
  <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
    <rect x="3" y="11" width="18" height="11" rx="2" /><path d="M7 11V7a5 5 0 0 1 10 0v4" />
  </svg>
);
const IconBox = () => (
  <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
    <path d="M21 16V8l-9-5-9 5v8l9 5 9-5Z" /><path d="m3 8 9 5 9-5" /><path d="M12 13v8" />
  </svg>
);

export default function Login() {
  const { register, handleSubmit, formState: { errors, isSubmitting } } = useForm();
  const { login } = useAuth();
  const nav = useNavigate();

  const onSubmit = async (data) => {
    try {
      await login(data.username, data.password);
      nav("/products");
    } catch {
      alert("Credenciales inválidas");
    }
  };

  return (
    <div className="login-wrap">
      <div className="login-card">
        <div className="login-brand">
          <div className="login-logo"><IconBox /></div>
          <h1>CatalogApi</h1>
          <p>Panel de administración de productos</p>
        </div>

        <form onSubmit={handleSubmit(onSubmit)}>
          <div className="field">
            <label>Usuario</label>
            <div className="input-icon">
              <span className="icon"><IconUser /></span>
              <input
                {...register("username", { required: "El usuario es obligatorio" })}
                placeholder="Ingresa tu usuario"
                autoFocus
              />
            </div>
            {errors.username && <span className="error">{errors.username.message}</span>}
          </div>

          <div className="field">
            <label>Contraseña</label>
            <div className="input-icon">
              <span className="icon"><IconLock /></span>
              <input
                type="password"
                {...register("password", { required: "La contraseña es obligatoria" })}
                placeholder="Ingresa tu contraseña"
              />
            </div>
            {errors.password && <span className="error">{errors.password.message}</span>}
          </div>

          <button type="submit" className="btn-primary login-btn" disabled={isSubmitting}>
            {isSubmitting ? "Ingresando..." : "Iniciar sesión"}
          </button>
        </form>

        <div className="login-hint">
          <span>Credenciales de prueba</span>
          admin&nbsp;/&nbsp;Admin123!
        </div>
      </div>
    </div>
  );
}
