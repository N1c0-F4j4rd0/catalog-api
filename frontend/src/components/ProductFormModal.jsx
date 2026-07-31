import { useEffect } from "react";
import { useForm } from "react-hook-form";
import { api } from "../api/client";
import Modal from "./Modal";

// Modal de crear/editar. Si recibe "product" -> modo edición; si no -> creación.
// Llama a onSaved() cuando guarda bien, para que la tabla se recargue.
export default function ProductFormModal({ product, onClose, onSaved }) {
  const isEdit = Boolean(product);
  const { register, handleSubmit, reset, formState: { errors, isSubmitting } } = useForm();

  useEffect(() => {
    if (isEdit) {
      reset({
        name: product.name,
        unitPrice: product.unitPrice,
        unitsInStock: product.unitsInStock,
        categoryId: product.categoryId,
        discontinued: product.discontinued,
      });
    } else {
      reset({ name: "", unitPrice: "", unitsInStock: "", categoryId: "", discontinued: false });
    }
  }, [product]);

  const onSubmit = async (form) => {
    const payload = {
      ...form,
      unitPrice: Number(form.unitPrice),
      unitsInStock: Number(form.unitsInStock),
      categoryId: Number(form.categoryId),
    };
    try {
      if (isEdit) await api.put(`/Product/${product.id}`, payload);
      else await api.post("/Product/single", payload);
      onSaved();
    } catch {
      alert("No se pudo guardar el producto");
    }
  };

  const footer = (
    <>
      <button type="button" onClick={onClose}>Cancelar</button>
      <button type="submit" form="product-form" className="btn-primary" disabled={isSubmitting}>
        {isSubmitting ? "Guardando..." : "Guardar"}
      </button>
    </>
  );

  return (
    <Modal title={isEdit ? "Editar producto" : "Nuevo producto"} onClose={onClose} footer={footer}>
      <form id="product-form" onSubmit={handleSubmit(onSubmit)}>
        <div className="field">
          <label>Nombre</label>
          <input type="text" {...register("name", { required: "El nombre es obligatorio" })} placeholder="Nombre del producto" />
          {errors.name && <span className="error">{errors.name.message}</span>}
        </div>

        <div className="field">
          <label>Precio</label>
          <input type="number" step="0.01"
            {...register("unitPrice", { required: "El precio es obligatorio", min: { value: 0, message: "Debe ser mayor o igual a 0" } })}
            placeholder="0.00" />
          {errors.unitPrice && <span className="error">{errors.unitPrice.message}</span>}
        </div>

        <div className="field">
          <label>Stock</label>
          <input type="number"
            {...register("unitsInStock", { required: "El stock es obligatorio", min: { value: 0, message: "Debe ser mayor o igual a 0" } })}
            placeholder="0" />
          {errors.unitsInStock && <span className="error">{errors.unitsInStock.message}</span>}
        </div>

        {!isEdit && (
          <div className="field">
            <label>Categoría</label>
            <select {...register("categoryId", { required: "Selecciona una categoría" })}
              style={{ width: "100%", padding: "9px 12px", border: "1px solid var(--border)", borderRadius: 6, background: "#fff" }}>
              <option value="">-- Selecciona --</option>
              <option value="1">SERVIDORES</option>
              <option value="2">CLOUD</option>
            </select>
            {errors.categoryId && <span className="error">{errors.categoryId.message}</span>}
          </div>
        )}

        <label style={{ display: "flex", alignItems: "center", gap: 8, fontWeight: 500, color: "var(--text)" }}>
          <input type="checkbox" {...register("discontinued")} style={{ width: "auto", boxShadow: "none" }} />
          Producto descontinuado
        </label>
      </form>
    </Modal>
  );
}
