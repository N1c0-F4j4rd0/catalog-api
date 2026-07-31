import { useForm } from "react-hook-form";
import { useNavigate } from "react-router-dom";
import { useAuth } from "../auth/AuthContext";

export default function Login() {
  const { register, handleSubmit, formState: { errors } } = useForm();
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
    <div style={{ padding: 24, maxWidth: 340, margin: "40px auto" }}>
      <h1>Ingresar</h1>
      <form onSubmit={handleSubmit(onSubmit)}>
        <div style={{ marginBottom: 10 }}>
          <input
            {...register("username", { required: "Usuario requerido" })}
            placeholder="Usuario"
            style={{ width: "100%", padding: 8 }}
          />
          {errors.username && <span style={{ color: "red" }}>{errors.username.message}</span>}
        </div>
        <div style={{ marginBottom: 10 }}>
          <input
            type="password"
            {...register("password", { required: "Contraseña requerida" })}
            placeholder="Contraseña"
            style={{ width: "100%", padding: 8 }}
          />
          {errors.password && <span style={{ color: "red" }}>{errors.password.message}</span>}
        </div>
        <button type="submit" style={{ padding: "8px 16px" }}>Entrar</button>
      </form>
      <p style={{ marginTop: 16, fontSize: 13, color: "#666" }}>
        admin / Admin123!
      </p>
    </div>
  );
}
