# CatalogApi

API REST para exponer un catálogo de **productos** y **categorías** a clientes y procesos internos, construida sobre **.NET 7** con arquitectura limpia por capas, PostgreSQL, autenticación JWT y carga masiva eficiente de datos. Incluye una **SPA en React** como cliente.

> Prueba técnica — Dev II. La solución busca reflejar tanto habilidades técnicas como criterios arquitectónicos.

---

## Tabla de contenido

- [Stack](#stack)
- [Arquitectura](#arquitectura)
- [Decisiones arquitectónicas](#decisiones-arquitectónicas)
- [Endpoints](#endpoints)
- [Cómo ejecutar](#cómo-ejecutar)
- [Carga masiva de 100.000 productos](#carga-masiva-de-100000-productos)
- [Seguridad](#seguridad)
- [Pruebas](#pruebas)
- [Escalabilidad horizontal en cloud](#escalabilidad-horizontal-en-cloud)
- [CI/CD](#cicd)
- [Frontend (SPA React)](#frontend-spa-react)

---

## Stack

- **Backend:** .NET 7 (C#), ASP.NET Core Web API
- **Persistencia:** PostgreSQL 16 + Entity Framework Core 7 (proveedor Npgsql)
- **Datos de prueba:** Bogus (generación de productos aleatorios)
- **Autenticación:** JWT (Bearer)
- **Pruebas:** xUnit, Moq (unitarias), Testcontainers (integración)
- **Contenedores:** Docker + docker-compose
- **CI:** GitHub Actions
- **Frontend:** React (Vite), axios, react-router-dom, react-hook-form

---

## Arquitectura

Arquitectura por capas (Onion) con la regla de dependencias apuntando siempre hacia el dominio:

    CatalogApi.Domain          -> Entidades. No depende de nada.
    CatalogApi.Application      -> DTOs, interfaces (puertos), servicios, mapeo.
    CatalogApi.Infrastructure   -> EF Core, DbContext, repositorios, JWT, carga masiva.
    CatalogApi.Api              -> Controllers, inyección de dependencias, middleware.


## Decisiones arquitectónicas

**Capas (Onion) sobre hexagonal puro.** Para un catálogo CRUD, hexagonal completo agrega complejidad que no aporta. Mantengo la regla de dependencias hacia el dominio: `Domain` no referencia nada, y la infraestructura (EF Core) queda detrás de interfaces definidas en `Application`, lo que permite mockear en pruebas sin tocar la base de datos.

**DTOs con mapeo manual.** Nunca se exponen las entidades de EF directamente. Uso métodos de extensión para el mapeo en vez de AutoMapper, para tener control explícito sobre qué se expone y evitar sobre-postear.

**PostgreSQL por su comando COPY.** Elegí PostgreSQL principalmente porque su comando `COPY` binario resuelve la carga masiva de 100.000 registros en un solo stream, en lugar de miles de INSERT. Ver la sección de carga masiva para el resultado medido.

**Foto de categoría como URL, no como binario.** La entidad `Category` guarda `PictureUrl` (un enlace) en lugar de la imagen binaria (blob). Guardar imágenes dentro de la base no escala: infla el tamaño, ralentiza las consultas y no permite usar un CDN. En producción la imagen viviría en un almacenamiento de objetos (S3) y la base solo guarda la referencia. El endpoint de detalle cumple igual con exponer la foto de la categoría, devolviendo el enlace.

**.NET 7** por requerimiento del proyecto.

---

## Endpoints

| Método | Ruta              | Descripción                                          | Protegido |
|--------|-------------------|------------------------------------------------------|-----------|
| POST   | `/auth/login`     | Devuelve un token JWT                                |    No     |
| POST   | `/Category`       | Crea una categoría                                   |    Sí     |
| POST   | `/Product?count=N`| Genera y carga N productos aleatorios (carga masiva) |    Sí     |
| POST   | `/Product/single` | Crea un producto individual                          |    Sí     |
| GET    | `/Products`       | Lista con paginación, filtros y búsqueda             |    No     |
| GET    | `/Products/{id}`  | Detalle del producto con foto de la categoría        |    No     |
| PUT    | `/Product/{id}`   | Actualiza un producto                                |    Sí     |
| DELETE | `/Product/{id}`   | Elimina un producto                                  |    Sí     |

Los endpoints de lectura son públicos; los de escritura requieren JWT (son los críticos).

**Parámetros de `GET /Products`:** `page`, `pageSize` (máx. 100), `search` (por nombre), `categoryId`, `minPrice`, `maxPrice`, `discontinued`.

La documentación interactiva está disponible en **Swagger** (`/swagger`), con soporte de autorización JWT integrado (botón *Authorize*).

---

## Cómo ejecutar

### Con Docker

Requiere Docker y Docker Compose.

    git clone https://github.com/N1c0-F4j4rd0/catalog-api.git
    cd catalog-api
    docker compose up --build

- **API + Swagger:** http://localhost:8080/swagger
- La base de datos se crea sola (migración automática al arrancar) y siembra las categorías **SERVIDORES** y **CLOUD**.

### Flujo de prueba

1. `POST /auth/login` con `{ "username": "admin", "password": "Admin123!" }` → se copia el token.
2. En Swagger, botón **Authorize**, se pega el token.
3. `POST /Product?count=100000` → se carga masivamente.
4. `GET /Products?page=1&pageSize=20&search=...&categoryId=...` → se consulta con filtros.

### Ejecución local (sin contenedor para la API)

Requiere .NET 7 SDK y un PostgreSQL accesible. Se ajusta la cadena de conexión en `src/CatalogApi.Api/appsettings.json` y:

    dotnet run --project src/CatalogApi.Api

Levanta en http://localhost:5133/swagger.

---

## Carga masiva de 100.000 productos

El endpoint `POST /Product?count=100000` genera productos aleatorios (con Bogus) y los inserta usando el comando **COPY binario** de PostgreSQL: se abre un único stream a la base y se escriben las filas una a una hacia el writer, sin materializar los 100.000 objetos en memoria.

**Resultado medido en local: 100.000 productos en ~3,5 segundos (3.531 ms).**

Esto contrasta con miles de INSERT individuales o un `AddRange` de EF, que generarían miles de round-trips a la base y tomarían órdenes de magnitud más tiempo.

---

## Seguridad

- Autenticación **JWT (Bearer)**. El `POST /auth/login` valida credenciales y emite un token firmado con expiración de 2 horas.
- Los endpoints de escritura (`POST`, `PUT`, `DELETE`) están protegidos con `[Authorize]`.
- Las llaves y secretos se inyectan por variables de entorno en Docker (no van en el código).

> El usuario de acceso es un usuario semilla para efectos de la prueba. En producción se reemplazaría por una tabla de usuarios con contraseñas hasheadas (BCrypt).

---

## Pruebas

    dotnet test

- **Unitaria** (`CatalogApi.UnitTests`): valida reglas de negocio del servicio con el repositorio mockeado vía **Moq** (por ejemplo, el recorte del `pageSize` a un máximo de 100).
- **Integración** (`CatalogApi.IntegrationTests`): **Testcontainers** levanta un PostgreSQL real en Docker y ejecuta un flujo de punta a punta (login + listado de productos).

Se usó Testcontainers en vez de una base en memoria para ejercitar el comportamiento real de PostgreSQL (COPY, ILIKE, migraciones). El test de integración requiere Docker en ejecución.

---

## Escalabilidad horizontal en cloud

La API es **stateless** (el estado vive en la base de datos y el JWT viaja en cada request), por lo que escala horizontalmente sin sesión pegajosa: N réplicas del contenedor detrás de un balanceador (ALB en AWS / Ingress en Kubernetes).

- **Base de datos:** PostgreSQL escala verticalmente para escritura y con réplicas de lectura para el `GET /Products`, apuntando las lecturas a las réplicas.
- **Cargas altas / picos:** la generación masiva de productos se sacaría del request. `POST /Product` encolaría un job (SQS / RabbitMQ) y un pool de *workers* correría el `COPY`, devolviendo `202 Accepted` con un id de trabajo para consultar el estado.
- **Cache:** Redis para las consultas paginadas más frecuentes y el detalle por id, invalidando en escrituras.
- **Despliegue en AWS:** contenedores en ECS Fargate o EKS con autoescalado por CPU/latencia, RDS PostgreSQL, ElastiCache (Redis) y las fotos de categoría en S3 + CloudFront.

---

## CI/CD

Pipeline en `.github/workflows/ci.yml`, ejecutado en cada push/PR a `main`. Tareas:

- **Restore** de dependencias
- **Format check** (`dotnet format --verify-no-changes`) — validación de estilo
- **Build** en configuración Release
- **Test** (unitarias + integración)
- **Docker build** de la imagen de la API

Verificable desde la pestaña **Actions** del repositorio.

---

## Frontend (SPA React)

Aplicación de página única en `frontend/`, construida con Vite.

    cd frontend
    npm install
    npm run dev

Queda en http://localhost:5173 (origen ya habilitado en el CORS del backend).

Funcionalidades:

- **Login** con usuario y contraseña; obtiene el token JWT desde la API.
- El token se guarda en **localStorage**.
- **Interceptor de axios** que agrega el token (`Authorization: Bearer`) a cada request, y redirige al login si la API responde 401.
- **AuthGuard** (`ProtectedRoute`) que protege las rutas de productos.
- **Listado de productos** con paginación, búsqueda y contador total.
- **Crear y editar** productos mediante un modal con validaciones, y **eliminar** con un diálogo de confirmación propio.
- **Enrutamiento centralizado** con react-router-dom.

> Nota sobre la terminología: el enunciado menciona *Reactive Forms* y *AppRoutingModule* (conceptos de Angular). Al ser una SPA en **React**, se implementaron con sus equivalentes: **react-hook-form** para los formularios con validación, y **react-router-dom** con enrutamiento centralizado para el ruteo modular.

---

## Notas

- **Puerto de PostgreSQL:** el `docker-compose.yml` mapea la base al puerto **5433** del host (para evitar choques con instalaciones locales de PostgreSQL en el 5432). La API dentro de Docker se conecta por la red interna en el 5432.
- Las URLs de las fotos de categoría apuntan a imágenes públicas de Unsplash a modo de ejemplo.
