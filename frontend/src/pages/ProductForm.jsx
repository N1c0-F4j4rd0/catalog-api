import { useEffect } from "react";
import { useForm } from "react-hook-form";
import { useNavigate, useParams } from "react-router-dom";
import { api } from "../api/client";

export default function ProductForm() {
  const { id } = useParams();
  const isEdit = Boolean(id);
  const { register, handleSubmit, reset, formState: { errors } } = useForm();
  const nav = useNavigate();

  useEffect(() => {
    if (isEdit) {
      api.get(`/Products/${id}`).then(({ data }) =>
        reset({
          name: data.name,
          unitPrice: data.unitPrice,
          unitsInStock: data.unitsInStock,
          categoryId: data.categoryId,
          discontinued: data.discontinued,
        })
      );
    }
  }, [id]);

  const onSubmit = async (form) => {
    const payload = {
      ...form,
      unitPrice: Number(form.unitPrice),
      unitsInStock: Number(form.unitsInStock),
      categoryId: Number(form.categoryId),
    };
    if (isEdit) await api.put(`/Product/${id}`, payload);
    else await api.post("/Product/single", payload);
    nav("/products");
  };

  return (
    <div style={{ padding: 24, maxWidth: 440, margin: "20px auto" }}>
      <h1>{isEdit ? "Editar" : "Nuevo"} producto</h1>
      <form onSubmit={handleSubmit(onSubmit)}>
        <div style={{ marginBottom: 10 }}>
          <input
            {...register("name", { required: "Nombre requerido" })}
            placeholder="Nombre"
            style={{ width: "100%", padding: 8 }}
          />
          {errors.name && <span style={{ color: "red" }}>{errors.name.message}</span>}
        </div>
        <div style={{ marginBottom: 10 }}>
          <input
            type="number"
            step="0.01"
            {...register("unitPrice", { required: true, min: { value: 0, message: "Debe ser mayor o igual a 0" } })}
            placeholder="Precio"
            style={{ width: "100%", padding: 8 }}
          />
          {errors.unitPrice && <span style={{ color: "red" }}>{errors.unitPrice.message || "Precio inválido"}</span>}
        </div>
        <div style={{ marginBottom: 10 }}>
          <input
            type="number"
            {...register("unitsInStock", { required: true, min: { value: 0, message: "Debe ser mayor o igual a 0" } })}
            placeholder="Stock"
            style={{ width: "100%", padding: 8 }}
          />
          {errors.unitsInStock && <span style={{ color: "red" }}>{errors.unitsInStock.message || "Stock inválido"}</span>}
        </div>
        {!isEdit && (
          <div style={{ marginBottom: 10 }}>
            <input
              type="number"
              {...register("categoryId", { required: "Categoría requerida" })}
              placeholder="CategoryId (1=SERVIDORES, 2=CLOUD)"
              style={{ width: "100%", padding: 8 }}
            />
            {errors.categoryId && <span style={{ color: "red" }}>{errors.categoryId.message}</span>}
          </div>
        )}
        <label style={{ display: "block", marginBottom: 12 }}>
          <input type="checkbox" {...register("discontinued")} /> Descontinuado
        </label>
        <button type="submit" style={{ padding: "8px 16px" }}>Guardar</button>{" "}
        <button type="button" onClick={() => nav("/products")} style={{ padding: "8px 16px" }}>Cancelar</button>
      </form>
    </div>
  );
}
