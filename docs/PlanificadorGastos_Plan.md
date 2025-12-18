# Plan de Diseño y Desarrollo: Planificador de Gastos Personales

## Información del Proyecto

| Campo | Valor |
|-------|-------|
| **Nombre** | Planificador de Gastos Personales |
| **Fecha de Inicio** | Diciembre 2024 |
| **Responsable** | Martin |
| **Estado** | En desarrollo (Backend completado) |

---

## 1. Visión General

Una aplicación web que permita registrar, categorizar y analizar gastos mensuales para tener visibilidad y control de las finanzas personales.

---

## 2. Stack Tecnológico

### 2.1 Frontend

| Categoría | Tecnología | Versión |
|-----------|------------|---------|
| Framework | Vue 3 (Composition API `<script setup>`) | v3.4.34 |
| Build Tool | Vite | v5.3.1 |
| UI Components | PrimeVue (tema Aura) | v4.3.4 |
| Estilos | Tailwind CSS | v3.4.6 |
| Preprocesador | SCSS | - |
| State Management | Pinia | - |
| Data Fetching | TanStack Query (Vue Query) + Axios | - |

### 2.2 Backend

| Categoría | Tecnología | Versión/Detalle |
|-----------|------------|-----------------|
| Lenguaje | C# | 13 |
| Framework | .NET / ASP.NET Core Web API | 9.0 |
| ORM Principal | Entity Framework Core | 9.0 |
| Micro-ORM | Dapper | Para queries complejos y SPs |
| Base de Datos | SQL Server | - |
| Autenticación | JWT Bearer Tokens + Microsoft Identity | - |
| Logging | Serilog (Console + File JSON rolling) | - |
| Documentación | Swagger/OpenAPI (`/swagger`) | - |
| Profiling | MiniProfiler | - |
| Compresión | Brotli/Gzip | - |

---

## 3. Funcionalidades

### 3.1 MVP (Mínimo Producto Viable)

#### Gestión de Gastos
- [x] Registro de gastos con fecha, monto, categoría y descripción
- [x] Edición y eliminación de registros
- [x] Visualización en lista con filtros por período y categoría

#### Categorías
- [x] Categorías predefinidas (Alimentación, Transporte, Servicios, Entretenimiento, Salud, etc.)
- [x] Creación de categorías personalizadas
- [x] Iconos y colores por categoría

#### Dashboard
- [x] Resumen del mes actual vs. mes anterior
- [x] Gráfico de gastos por categoría (torta/dona)
- [x] Evolución mensual (gráfico de líneas/barras)
- [x] Indicadores: total gastado, promedio diario, categoría con mayor gasto

### 3.2 Segunda Iteración

#### Presupuestos
- [ ] Definir límites por categoría
- [ ] Alertas cuando se acerca al límite
- [ ] Porcentaje de consumo visual

### 3.3 Futuras Mejoras (Backlog)
- [ ] Gastos recurrentes (suscripciones, servicios fijos)
- [ ] Exportación a Excel/PDF
- [ ] Múltiples monedas
- [ ] Modo oscuro
- [ ] PWA (Progressive Web App)
- [ ] Notificaciones push

---

## 4. Arquitectura

```
┌─────────────────────────────────────────────────────────┐
│                    FRONTEND (Vue 3)                      │
│  ┌─────────┐  ┌─────────┐  ┌─────────┐  ┌─────────┐    │
│  │Dashboard│  │ Gastos  │  │Categorías│  │Reportes │    │
│  └────┬────┘  └────┬────┘  └────┬────┘  └────┬────┘    │
│       └────────────┴────────────┴────────────┘          │
│                         │                                │
│              ┌──────────┴──────────┐                    │
│              │   Pinia + TanStack  │                    │
│              └──────────┬──────────┘                    │
└─────────────────────────┼───────────────────────────────┘
                          │ Axios (HTTP/JWT)
                          ▼
┌─────────────────────────────────────────────────────────┐
│                 BACKEND (.NET 9 API)                     │
│  ┌──────────────────────────────────────────────────┐   │
│  │              Controllers (REST)                   │   │
│  │  /api/auth  /api/gastos  /api/categorias         │   │
│  │             /api/presupuestos  /api/reportes     │   │
│  └──────────────────────┬───────────────────────────┘   │
│                         │                                │
│  ┌──────────────────────┴───────────────────────────┐   │
│  │                 Services Layer                    │   │
│  └──────────────────────┬───────────────────────────┘   │
│                         │                                │
│  ┌─────────────┐  ┌─────┴─────┐                         │
│  │  EF Core 9  │  │  Dapper   │                         │
│  │  (CRUD)     │  │ (Reportes)│                         │
│  └──────┬──────┘  └─────┬─────┘                         │
└─────────┼───────────────┼───────────────────────────────┘
          └───────┬───────┘
                  ▼
         ┌───────────────┐
         │   SQL Server  │
         └───────────────┘
```

### 4.1 Estructura de Carpetas Backend (.NET 9)

```
PlanificadorGastos.API/
├── Controllers/
│   ├── AuthController.cs
│   ├── GastosController.cs
│   ├── CategoriasController.cs
│   ├── PresupuestosController.cs
│   └── ReportesController.cs
├── Services/
│   ├── Interfaces/
│   │   ├── IAuthService.cs
│   │   ├── IGastosService.cs
│   │   ├── ICategoriasService.cs
│   │   └── IReportesService.cs
│   └── Implementations/
│       ├── AuthService.cs
│       ├── GastosService.cs
│       ├── CategoriasService.cs
│       └── ReportesService.cs
├── Models/
│   ├── Entities/
│   │   ├── Usuario.cs
│   │   ├── Gasto.cs
│   │   ├── Categoria.cs
│   │   └── Presupuesto.cs
│   ├── DTOs/
│   │   ├── Auth/
│   │   ├── Gastos/
│   │   ├── Categorias/
│   │   └── Reportes/
│   └── Common/
│       ├── ApiResponse.cs
│       └── PaginatedResult.cs
├── Data/
│   ├── ApplicationDbContext.cs
│   ├── Configurations/
│   │   ├── UsuarioConfiguration.cs
│   │   ├── GastoConfiguration.cs
│   │   └── CategoriaConfiguration.cs
│   └── Repositories/
│       ├── Interfaces/
│       └── Implementations/
├── Infrastructure/
│   ├── Authentication/
│   │   ├── JwtSettings.cs
│   │   └── JwtService.cs
│   └── Extensions/
│       ├── ServiceCollectionExtensions.cs
│       └── ApplicationBuilderExtensions.cs
├── Middleware/
│   └── ExceptionHandlingMiddleware.cs
├── appsettings.json
├── appsettings.Development.json
└── Program.cs
```

### 4.2 Estructura de Carpetas Frontend (Vue 3)

```
planificador-gastos-web/
├── public/
├── src/
│   ├── assets/
│   │   ├── images/
│   │   └── styles/
│   │       ├── main.scss
│   │       └── _variables.scss
│   ├── components/
│   │   ├── common/
│   │   │   ├── AppHeader.vue
│   │   │   ├── AppSidebar.vue
│   │   │   ├── LoadingSpinner.vue
│   │   │   └── ConfirmDialog.vue
│   │   ├── gastos/
│   │   │   ├── GastoForm.vue
│   │   │   ├── GastoList.vue
│   │   │   └── GastoItem.vue
│   │   ├── categorias/
│   │   │   ├── CategoriaForm.vue
│   │   │   ├── CategoriaList.vue
│   │   │   └── CategoriaSelector.vue
│   │   └── dashboard/
│   │       ├── ResumenMensual.vue
│   │       ├── GraficoCategorias.vue
│   │       └── GraficoEvolucion.vue
│   ├── composables/
│   │   ├── useAuth.js
│   │   ├── useGastos.js
│   │   ├── useCategorias.js
│   │   └── useNotification.js
│   ├── layouts/
│   │   ├── DefaultLayout.vue
│   │   └── AuthLayout.vue
│   ├── pages/
│   │   ├── auth/
│   │   │   ├── LoginPage.vue
│   │   │   └── RegisterPage.vue
│   │   ├── DashboardPage.vue
│   │   ├── GastosPage.vue
│   │   ├── CategoriasPage.vue
│   │   └── ReportesPage.vue
│   ├── router/
│   │   └── index.js
│   ├── services/
│   │   ├── api.js
│   │   ├── authService.js
│   │   ├── gastosService.js
│   │   └── categoriasService.js
│   ├── stores/
│   │   ├── authStore.js
│   │   └── uiStore.js
│   ├── utils/
│   │   ├── formatters.js
│   │   ├── validators.js
│   │   └── constants.js
│   ├── App.vue
│   └── main.js
├── index.html
├── vite.config.js
├── tailwind.config.js
├── postcss.config.js
└── package.json
```

---

## 5. Modelo de Datos

### 5.1 Diagrama Entidad-Relación

```
┌─────────────┐       ┌─────────────┐       ┌─────────────┐
│   Usuarios  │       │  Categorias │       │   Gastos    │
├─────────────┤       ├─────────────┤       ├─────────────┤
│ Id (PK)     │──┐    │ Id (PK)     │──┐    │ Id (PK)     │
│ Email       │  │    │ UserId (FK) │  │    │ UserId (FK) │
│ PasswordHash│  └───>│ Nombre      │  └───>│ CategoriaId │
│ Nombre      │       │ Icono       │       │ Monto       │
│ CreatedAt   │       │ Color       │       │ Fecha       │
│ LastLogin   │       │ EsPredef.   │       │ Descripcion │
└─────────────┘       │ Activa      │       │ CreatedAt   │
       │              └─────────────┘       │ UpdatedAt   │
       │                                    └─────────────┘
       │              ┌─────────────┐
       │              │ Presupuestos│
       │              ├─────────────┤
       └─────────────>│ Id (PK)     │
                      │ UserId (FK) │
                      │ CategoriaId │
                      │ MontoLimite │
                      │ Año         │
                      │ Mes         │
                      └─────────────┘
```

### 5.2 Script SQL Inicial

```sql
-- =============================================
-- Base de Datos: PlanificadorGastos
-- =============================================

-- Tabla: Usuarios
CREATE TABLE Usuarios (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Email NVARCHAR(256) NOT NULL UNIQUE,
    PasswordHash NVARCHAR(MAX) NOT NULL,
    Nombre NVARCHAR(100) NOT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    LastLogin DATETIME2 NULL
);

-- Tabla: Categorias
CREATE TABLE Categorias (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    UserId INT NULL, -- NULL para categorías predefinidas del sistema
    Nombre NVARCHAR(50) NOT NULL,
    Icono NVARCHAR(50) NOT NULL DEFAULT 'pi-tag',
    Color NVARCHAR(7) NOT NULL DEFAULT '#6366f1',
    EsPredefinida BIT NOT NULL DEFAULT 0,
    Activa BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT FK_Categorias_Usuarios FOREIGN KEY (UserId) REFERENCES Usuarios(Id)
);

-- Tabla: Gastos
CREATE TABLE Gastos (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    UserId INT NOT NULL,
    CategoriaId INT NOT NULL,
    Monto DECIMAL(18,2) NOT NULL,
    Fecha DATE NOT NULL,
    Descripcion NVARCHAR(500) NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NULL,
    CONSTRAINT FK_Gastos_Usuarios FOREIGN KEY (UserId) REFERENCES Usuarios(Id),
    CONSTRAINT FK_Gastos_Categorias FOREIGN KEY (CategoriaId) REFERENCES Categorias(Id)
);

-- Tabla: Presupuestos
CREATE TABLE Presupuestos (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    UserId INT NOT NULL,
    CategoriaId INT NULL, -- NULL = presupuesto general
    MontoLimite DECIMAL(18,2) NOT NULL,
    Anio INT NOT NULL,
    Mes INT NOT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT FK_Presupuestos_Usuarios FOREIGN KEY (UserId) REFERENCES Usuarios(Id),
    CONSTRAINT FK_Presupuestos_Categorias FOREIGN KEY (CategoriaId) REFERENCES Categorias(Id),
    CONSTRAINT UQ_Presupuesto_Usuario_Categoria_Periodo UNIQUE (UserId, CategoriaId, Anio, Mes)
);

-- Índices
CREATE INDEX IX_Gastos_UserId_Fecha ON Gastos(UserId, Fecha DESC);
CREATE INDEX IX_Gastos_CategoriaId ON Gastos(CategoriaId);
CREATE INDEX IX_Categorias_UserId ON Categorias(UserId);

-- Seed: Categorías predefinidas del sistema
INSERT INTO Categorias (UserId, Nombre, Icono, Color, EsPredefinida, Activa) VALUES
(NULL, 'Alimentación', 'pi-shopping-cart', '#22c55e', 1, 1),
(NULL, 'Transporte', 'pi-car', '#3b82f6', 1, 1),
(NULL, 'Servicios', 'pi-bolt', '#f59e0b', 1, 1),
(NULL, 'Entretenimiento', 'pi-ticket', '#ec4899', 1, 1),
(NULL, 'Salud', 'pi-heart', '#ef4444', 1, 1),
(NULL, 'Educación', 'pi-book', '#8b5cf6', 1, 1),
(NULL, 'Hogar', 'pi-home', '#06b6d4', 1, 1),
(NULL, 'Ropa', 'pi-tag', '#f97316', 1, 1),
(NULL, 'Otros', 'pi-ellipsis-h', '#6b7280', 1, 1);

-- Vista: Resumen de gastos mensuales
CREATE VIEW vw_GastosMensuales AS
SELECT 
    g.UserId,
    YEAR(g.Fecha) AS Anio,
    MONTH(g.Fecha) AS Mes,
    g.CategoriaId,
    c.Nombre AS CategoriaNombre,
    c.Color AS CategoriaColor,
    c.Icono AS CategoriaIcono,
    SUM(g.Monto) AS TotalMonto,
    COUNT(*) AS CantidadGastos
FROM Gastos g
INNER JOIN Categorias c ON g.CategoriaId = c.Id
GROUP BY g.UserId, YEAR(g.Fecha), MONTH(g.Fecha), g.CategoriaId, c.Nombre, c.Color, c.Icono;
```

---

## 6. API Endpoints

### 6.1 Autenticación (`/api/auth`)

| Método | Endpoint | Descripción |
|--------|----------|-------------|
| POST | `/api/auth/register` | Registro de nuevo usuario |
| POST | `/api/auth/login` | Inicio de sesión |
| POST | `/api/auth/refresh-token` | Renovar token JWT |
| POST | `/api/auth/logout` | Cerrar sesión |
| GET | `/api/auth/me` | Obtener usuario actual |

### 6.2 Categorías (`/api/categorias`)

| Método | Endpoint | Descripción |
|--------|----------|-------------|
| GET | `/api/categorias` | Listar categorías del usuario |
| GET | `/api/categorias/{id}` | Obtener categoría por ID |
| POST | `/api/categorias` | Crear categoría |
| PUT | `/api/categorias/{id}` | Actualizar categoría |
| DELETE | `/api/categorias/{id}` | Eliminar categoría |

### 6.3 Gastos (`/api/gastos`)

| Método | Endpoint | Descripción |
|--------|----------|-------------|
| GET | `/api/gastos` | Listar gastos (con paginación y filtros) |
| GET | `/api/gastos/{id}` | Obtener gasto por ID |
| POST | `/api/gastos` | Crear gasto |
| PUT | `/api/gastos/{id}` | Actualizar gasto |
| DELETE | `/api/gastos/{id}` | Eliminar gasto |

**Query params para GET `/api/gastos`:**
- `page` (int): Página actual
- `pageSize` (int): Cantidad por página
- `fechaDesde` (date): Filtrar desde fecha
- `fechaHasta` (date): Filtrar hasta fecha
- `categoriaId` (int): Filtrar por categoría
- `ordenarPor` (string): Campo de ordenamiento
- `ordenDesc` (bool): Orden descendente

### 6.4 Presupuestos (`/api/presupuestos`)

| Método | Endpoint | Descripción |
|--------|----------|-------------|
| GET | `/api/presupuestos` | Listar presupuestos |
| GET | `/api/presupuestos/{anio}/{mes}` | Obtener presupuestos de un mes |
| POST | `/api/presupuestos` | Crear/actualizar presupuesto |
| DELETE | `/api/presupuestos/{id}` | Eliminar presupuesto |

### 6.5 Reportes (`/api/reportes`)

| Método | Endpoint | Descripción |
|--------|----------|-------------|
| GET | `/api/reportes/resumen-mensual` | Resumen del mes actual |
| GET | `/api/reportes/por-categoria` | Gastos agrupados por categoría |
| GET | `/api/reportes/evolucion` | Evolución mensual (últimos N meses) |
| GET | `/api/reportes/comparativo` | Comparativo mes actual vs anterior |

---

## 7. Plan de Fases

### Fase 0: Setup y Estructura
**Duración estimada:** 1-2 días

- [x] Documentación del proyecto (este archivo)
- [x] Crear solución .NET 9 con estructura de carpetas
- [x] Configurar EF Core + Dapper + Serilog + Swagger
- [ ] Crear proyecto Vue 3 con Vite
- [ ] Configurar PrimeVue + Tailwind + Pinia + TanStack Query
- [x] Script inicial de base de datos
- [x] Configurar repositorio Git

### Fase 1: Autenticación
**Duración estimada:** 3-4 días

- [x] Backend: Configurar Identity + JWT
- [x] Backend: Endpoints de registro, login, refresh token
- [ ] Frontend: Páginas de Login y Registro
- [ ] Frontend: Composable useAuth
- [ ] Frontend: Guards de rutas protegidas
- [ ] Frontend: Manejo de token en Axios interceptors

### Fase 2: CRUD de Categorías
**Duración estimada:** 2-3 días

- [x] Backend: Controller y Service de Categorías
- [x] Backend: Seed de categorías predefinidas
- [ ] Frontend: Página de gestión de categorías
- [ ] Frontend: Componente selector de categorías
- [ ] Frontend: Selector de iconos y colores

### Fase 3: CRUD de Gastos
**Duración estimada:** 4-5 días

- [x] Backend: Controller y Service de Gastos
- [x] Backend: Paginación y filtros
- [ ] Frontend: Página de gastos con listado
- [ ] Frontend: Formulario de carga rápida
- [ ] Frontend: Filtros por fecha y categoría
- [ ] Frontend: Edición y eliminación
- [x] Validaciones frontend y backend

### Fase 4: Dashboard y Reportes
**Duración estimada:** 4-5 días

- [x] Backend: Stored procedures para agregaciones
- [x] Backend: Endpoints de reportes
- [ ] Frontend: Dashboard con KPIs
- [ ] Frontend: Gráfico de torta por categoría
- [ ] Frontend: Gráfico de evolución mensual
- [ ] Frontend: Comparativo mes actual vs anterior

### Fase 5: Presupuestos
**Duración estimada:** 3-4 días

- [x] Backend: CRUD de presupuestos
- [x] Backend: Lógica de cálculo de consumo
- [ ] Frontend: Configuración de presupuestos
- [ ] Frontend: Indicadores visuales de consumo
- [ ] Frontend: Alertas de límite

### Fase 6: Polish y Deploy
**Duración estimada:** 2-3 días

- [ ] Responsive design completo
- [ ] Loading states y skeleton loaders
- [x] Error handling global (Backend: ExceptionHandlingMiddleware)
- [ ] Toasts y notificaciones
- [ ] Dockerización
- [ ] Configuración de CI/CD
- [ ] Deploy a producción

---

## 8. Wireframes

### 8.1 Login
```
┌─────────────────────────────────────────┐
│                                         │
│         💰 Planificador de Gastos       │
│                                         │
│    ┌─────────────────────────────┐      │
│    │ Email                       │      │
│    └─────────────────────────────┘      │
│    ┌─────────────────────────────┐      │
│    │ Contraseña            👁    │      │
│    └─────────────────────────────┘      │
│                                         │
│    ┌─────────────────────────────┐      │
│    │       Iniciar Sesión        │      │
│    └─────────────────────────────┘      │
│                                         │
│    ¿No tenés cuenta? Registrate         │
│                                         │
└─────────────────────────────────────────┘
```

### 8.2 Dashboard
```
┌────────────────────────────────────────────────────────────────┐
│ ☰  Planificador de Gastos                        👤 Martin ▼  │
├────────┬───────────────────────────────────────────────────────┤
│        │                                                       │
│ 🏠 Home│  Diciembre 2024                                      │
│        │  ┌──────────┐ ┌──────────┐ ┌──────────┐ ┌──────────┐ │
│ 💰 Gas │  │Total Mes │ │ vs Mes   │ │ Promedio │ │ Mayor    │ │
│        │  │$125.430  │ │ Anterior │ │ Diario   │ │ Gasto    │ │
│ 📁 Cat │  │          │ │ +12%  ▲  │ │ $4.181   │ │Aliment.  │ │
│        │  └──────────┘ └──────────┘ └──────────┘ └──────────┘ │
│ 📊 Rep │                                                       │
│        │  ┌─────────────────────┐ ┌─────────────────────────┐ │
│ ⚙️ Conf│  │                     │ │                         │ │
│        │  │   [Gráfico Torta]   │ │  [Gráfico Evolución]    │ │
│        │  │   Por Categoría     │ │  Últimos 6 meses        │ │
│        │  │                     │ │                         │ │
│        │  └─────────────────────┘ └─────────────────────────┘ │
│        │                                                       │
│        │  Últimos Gastos                          [Ver todos] │
│        │  ┌─────────────────────────────────────────────────┐ │
│        │  │ 🛒 Supermercado Día    -$15.230    Hoy          │ │
│        │  │ 🚗 Nafta               -$8.500     Ayer         │ │
│        │  │ 💡 Edenor              -$12.350    15/12        │ │
│        │  └─────────────────────────────────────────────────┘ │
└────────┴───────────────────────────────────────────────────────┘
```

### 8.3 Lista de Gastos
```
┌────────────────────────────────────────────────────────────────┐
│ ☰  Planificador de Gastos                        👤 Martin ▼  │
├────────┬───────────────────────────────────────────────────────┤
│        │                                                       │
│ 🏠 Home│  Gastos                              [+ Nuevo Gasto]  │
│        │                                                       │
│ 💰 Gas │  ┌─────────────────────────────────────────────────┐ │
│   ●    │  │ 📅 Desde: [01/12/2024] Hasta: [31/12/2024]     │ │
│ 📁 Cat │  │ 📁 Categoría: [Todas           ▼]    [Filtrar] │ │
│        │  └─────────────────────────────────────────────────┘ │
│ 📊 Rep │                                                       │
│        │  ┌────┬────────────┬───────────┬────────┬──────────┐ │
│ ⚙️ Conf│  │Cat │Descripción │  Fecha    │ Monto  │ Acciones │ │
│        │  ├────┼────────────┼───────────┼────────┼──────────┤ │
│        │  │ 🛒 │Supermerc..│ 18/12/24  │-15.230 │ ✏️  🗑️  │ │
│        │  │ 🚗 │Nafta YPF  │ 17/12/24  │ -8.500 │ ✏️  🗑️  │ │
│        │  │ 💡 │Edenor     │ 15/12/24  │-12.350 │ ✏️  🗑️  │ │
│        │  │ 🎬 │Netflix    │ 10/12/24  │ -2.999 │ ✏️  🗑️  │ │
│        │  │ 🏥 │Farmacia   │ 08/12/24  │ -4.500 │ ✏️  🗑️  │ │
│        │  └────┴────────────┴───────────┴────────┴──────────┘ │
│        │                                                       │
│        │  Mostrando 1-10 de 45           [<] [1] [2] [3] [>]  │
└────────┴───────────────────────────────────────────────────────┘
```

---

## 9. Notas y Decisiones Técnicas

### 9.1 ¿Por qué EF Core + Dapper?
- **EF Core** para operaciones CRUD simples: aprovecha el tracking de cambios, migraciones y facilidad de uso
- **Dapper** para consultas complejas y reportes: mejor rendimiento en queries con múltiples joins y agregaciones

### 9.2 ¿Por qué TanStack Query?
- Cache automático de datos
- Revalidación inteligente
- Estados de loading/error manejados
- Optimistic updates para mejor UX

### 9.3 Consideraciones de Seguridad
- Passwords hasheados con Identity (bcrypt/PBKDF2)
- JWT con refresh tokens para sesiones largas
- Validación de ownership en cada request (el usuario solo ve sus datos)
- Rate limiting en endpoints sensibles
- CORS configurado para dominios específicos

---

## 10. Referencias

- [Vue 3 Documentation](https://vuejs.org/)
- [PrimeVue Components](https://primevue.org/)
- [Tailwind CSS](https://tailwindcss.com/)
- [TanStack Query](https://tanstack.com/query/latest)
- [ASP.NET Core Documentation](https://docs.microsoft.com/aspnet/core)
- [Entity Framework Core](https://docs.microsoft.com/ef/core)
- [Dapper](https://github.com/DapperLib/Dapper)

---

## Changelog

| Fecha | Versión | Cambios |
|-------|---------|---------|
| 2024-12-18 | 1.0 | Documento inicial con plan completo |
