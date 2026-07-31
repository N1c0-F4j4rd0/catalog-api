import { useEffect, useState } from "react";
import { api } from "../api/client";
import { useAuth } from "../auth/AuthContext";
import ProductFormModal from "../components/ProductFormModal";
import ConfirmDialog from "../components/ConfirmDialog";

const IconPlus = () => (
  <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2.2" strokeLinecap="round">
    <path d="M12 5v14M5 12h14" />
  </svg>
);
const IconEdit = () => (
  <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
    <path d="M12 20h9" /><path d="M16.5 3.5a2.1 2.1 0 0 1 3 3L7 19l-4 1 1-4Z" />
  </svg>
);
const IconTrash = () => (
  <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
    <path d="M3 6h18M8 6V4h8v2M19 6l-1 14H6L5 6" />
  </svg>
);
const IconSearch = () => (
  <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round">
    <circle cx="11" cy="11" r="7" /><path d="m21 21-4.3-4.3" />
  </svg>
);
const IconLogout = () => (
  <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
    <path d="M9 21H5a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h4M16 17l5-5-5-5M21 12H9" />
  </svg>
);
const IconBox = () => (
  <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
    <path d="M21 16V8l-9-5-9 5v8l9 5 9-5Z" /><path d="m3 8 9 5 9-5" /><path d="M12 13v8" />
  </svg>
);

export default function Products() {
  const [data, setData] = useState({ items: [], total: 0, page: 1, pageSize: 10 });
  const [search, setSearch] = useState("");
  const [page, setPage] = useState(1);
  const { logout } = useAuth();

  const [formModal, setFormModal] = useState({ open: false, product: null });
  const [confirmDelete, setConfirmDelete] = useState(null); 

  const load = async () => {
    const { data } = await api.get("/Products", {
      params: { page, pageSize: 10, search: search || undefined },
    });
    setData(data);
  };

  useEffect(() => { load(); }, [page]);

  const openNew = () => setFormModal({ open: true, product: null });
  const openEdit = (product) => setFormModal({ open: true, product });
  const closeForm = () => setFormModal({ open: false, product: null });

  const onSaved = () => {
    closeForm();
    load();
  };

  const doDelete = async () => {
    await api.delete(`/Product/${confirmDelete.id}`);
    setConfirmDelete(null);
    load();
  };

  const totalPages = Math.ceil(data.total / data.pageSize) || 1;

  const badgeClass = (cat) =>
    cat === "SERVIDORES" ? "badge badge-servidores"
    : cat === "CLOUD" ? "badge badge-cloud"
    : "badge";

  return (
    <div style={{ padding: "24px 40px" }}>
      <div className="page-header">
        <div className="header-title">
          <span className="bar"></span>
          <h1>Productos</h1>
        </div>
        <div className="header-controls">
          <div className="search-box">
            <input
              value={search}
              onChange={(e) => setSearch(e.target.value)}
              onKeyDown={(e) => { if (e.key === "Enter") { setPage(1); load(); } }}
              placeholder="Buscar por nombre..."
            />
            <button className="btn-search header-btn" onClick={() => { setPage(1); load(); }}><IconSearch /> Buscar</button>
          </div>
          <div className="actions">
            <span className="btn btn-counter header-btn"><IconBox /> Total:&nbsp;<span className="count">{data.total.toLocaleString()}</span></span>
            <button className="btn-primary header-btn" onClick={openNew}><IconPlus /> Nuevo producto</button>
            <button className="header-btn" onClick={logout}><IconLogout /> Cerrar sesión</button>
          </div>
        </div>
      </div>

      <div className="card">
        <table>
          <thead>
            <tr>
              <th>ID</th><th>Nombre</th><th>Precio</th><th>Stock</th>
              <th>Categoría</th><th>Acciones</th>
            </tr>
          </thead>
          <tbody>
            {data.items.map((p) => (
              <tr key={p.id}>
                <td>{p.id}</td>
                <td>{p.name}</td>
                <td>${p.unitPrice}</td>
                <td>{p.unitsInStock}</td>
                <td><span className={badgeClass(p.categoryName)}>{p.categoryName}</span></td>
                <td>
                  <div className="cell-actions">
                    <button className="btn-sm btn-edit" onClick={() => openEdit(p)}><IconEdit /> Editar</button>
                    <button className="btn-sm btn-danger" onClick={() => setConfirmDelete(p)}><IconTrash /> Borrar</button>
                  </div>
                </td>
              </tr>
            ))}
          </tbody>
        </table>

        <div className="pagination">
          <span>Página {data.page} de {totalPages}</span>
          <button disabled={page <= 1} onClick={() => setPage((p) => p - 1)}>← Anterior</button>
          <button
            disabled={data.page * data.pageSize >= data.total}
            onClick={() => setPage((p) => p + 1)}
          >
            Siguiente →
          </button>
        </div>
      </div>

      {formModal.open && (
        <ProductFormModal
          product={formModal.product}
          onClose={closeForm}
          onSaved={onSaved}
        />
      )}

      {confirmDelete && (
        <ConfirmDialog
          title="Eliminar producto"
          message={`¿Seguro que deseas eliminar "${confirmDelete.name}"? Esta acción no se puede deshacer.`}
          confirmText="Sí, eliminar"
          onConfirm={doDelete}
          onCancel={() => setConfirmDelete(null)}
        />
      )}
    </div>
  );
}
