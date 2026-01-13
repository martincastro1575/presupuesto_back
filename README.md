# 💰 Planificador de Gastos Personales - API

API REST desarrollada en .NET 9 para gestión de gastos personales.

## 📋 Requisitos Previos

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [SQL Server](https://www.microsoft.com/sql-server) (Express, Developer o superior)
- [Visual Studio 2022](https://visualstudio.microsoft.com/) o [VS Code](https://code.visualstudio.com/)

## 🚀 Instalación

### 1. Clonar/Copiar el proyecto

```bash
cd tu-carpeta-de-proyectos
# Si usas git:
git clone <url-del-repo>
# O simplemente copia la carpeta del proyecto
```

### 2. Restaurar paquetes NuGet

```bash
cd PlanificadorGastos
dotnet restore
```

### 3. Configurar la Base de Datos

#### Opción A: Usar EF Core Migrations (Recomendado)

```bash
cd src/PlanificadorGastos.API

# Crear la migración inicial
dotnet ef migrations add InitialCreate

# Aplicar la migración (crea la BD y tablas)
dotnet ef database update
```

#### Opción B: Ejecutar script SQL manualmente

1. Abrir SQL Server Management Studio (SSMS)
2. Crear la base de datos:
   ```sql
   CREATE DATABASE PlanificadorGastos;
   ```
3. Ejecutar el script `database/init.sql`

### 4. Configurar Connection String

Editar `src/PlanificadorGastos.API/appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=TU_SERVIDOR;Database=PlanificadorGastos;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

**Ejemplos de connection strings:**

```
# SQL Server local con Windows Auth
Server=localhost;Database=PlanificadorGastos;Trusted_Connection=True;TrustServerCertificate=True;

# SQL Server local con SQL Auth
Server=localhost;Database=PlanificadorGastos;User Id=sa;Password=TuPassword;TrustServerCertificate=True;

# SQL Server Express
Server=localhost\SQLEXPRESS;Database=PlanificadorGastos;Trusted_Connection=True;TrustServerCertificate=True;

# LocalDB (Visual Studio)
Server=(localdb)\MSSQLLocalDB;Database=PlanificadorGastos;Trusted_Connection=True;TrustServerCertificate=True;
```

### 5. Ejecutar la aplicación

```bash
cd src/PlanificadorGastos.API
dotnet run
```

O en Visual Studio: `F5` o `Ctrl+F5`

### 6. Acceder a Swagger

Abrir en el navegador: `https://localhost:5001/swagger` o `http://localhost:5000/swagger`

## 📁 Estructura del Proyecto

```
PlanificadorGastos/
├── src/
│   └── PlanificadorGastos.API/
│       ├── Controllers/          # Endpoints REST
│       ├── Services/             # Lógica de negocio
│       │   ├── Interfaces/
│       │   └── Implementations/
│       ├── Models/
│       │   ├── Entities/         # Entidades de BD
│       │   ├── DTOs/             # Objetos de transferencia
│       │   └── Common/           # Clases comunes
│       ├── Data/
│       │   ├── Configurations/   # Configuraciones EF Core
│       │   └── Repositories/
│       ├── Infrastructure/
│       │   ├── Authentication/   # JWT
│       │   └── Extensions/
│       ├── Middleware/
│       ├── Program.cs
│       └── appsettings.json
├── database/
│   └── init.sql                  # Script inicial BD
└── PlanificadorGastos.sln
```

## 🔐 Autenticación

La API usa JWT Bearer Tokens. Flujo:

1. **Registrar usuario:** `POST /api/auth/register`
2. **Iniciar sesión:** `POST /api/auth/login` → Devuelve `token` y `refreshToken`
3. **Usar token:** Agregar header `Authorization: Bearer {token}`
4. **Renovar token:** `POST /api/auth/refresh-token`

## 📡 Endpoints Principales

### Autenticación (`/api/auth`)
| Método | Endpoint | Descripción |
|--------|----------|-------------|
| POST | `/api/auth/register` | Registrar usuario |
| POST | `/api/auth/login` | Iniciar sesión |
| POST | `/api/auth/refresh-token` | Renovar token JWT |
| POST | `/api/auth/logout` | Cerrar sesión (revocar token) |
| GET | `/api/auth/me` | Usuario actual |

### Categorías (`/api/categorias`)
| Método | Endpoint | Descripción |
|--------|----------|-------------|
| GET | `/api/categorias` | Listar categorías (filtro: `?tipo=1` Gasto, `?tipo=2` Ingreso) |
| GET | `/api/categorias/{id}` | Obtener categoría por ID |
| POST | `/api/categorias` | Crear categoría |
| PUT | `/api/categorias/{id}` | Actualizar categoría |
| DELETE | `/api/categorias/{id}` | Eliminar categoría |

> **Nota:** Las categorías tienen un campo `tipo` que indica si son de Gasto (1), Ingreso (2) o Ambos (3).

### Gastos (`/api/gastos`)
| Método | Endpoint | Descripción |
|--------|----------|-------------|
| GET | `/api/gastos` | Listar gastos (paginado) |
| GET | `/api/gastos/{id}` | Obtener gasto por ID |
| POST | `/api/gastos` | Crear gasto |
| PUT | `/api/gastos/{id}` | Actualizar gasto |
| DELETE | `/api/gastos/{id}` | Eliminar gasto |

### Presupuestos (`/api/presupuestos`)
| Método | Endpoint | Descripción |
|--------|----------|-------------|
| GET | `/api/presupuestos` | Listar presupuestos |
| GET | `/api/presupuestos/{anio}/{mes}` | Obtener por período |
| POST | `/api/presupuestos` | Crear/actualizar presupuesto |
| DELETE | `/api/presupuestos/{id}` | Eliminar presupuesto |

### Ingresos (`/api/ingresos`)
| Método | Endpoint | Descripción |
|--------|----------|-------------|
| GET | `/api/ingresos` | Listar ingresos |
| GET | `/api/ingresos/{id}` | Obtener ingreso por ID |
| GET | `/api/ingresos/periodo/{anio}/{mes}` | Obtener por período |
| POST | `/api/ingresos` | Crear ingreso |
| PUT | `/api/ingresos/{id}` | Actualizar ingreso |
| DELETE | `/api/ingresos/{id}` | Eliminar ingreso |

### Reportes (`/api/reportes`)
| Método | Endpoint | Descripción |
|--------|----------|-------------|
| GET | `/api/reportes/resumen-mensual` | Resumen del mes (ingresos, gastos, balance) |
| GET | `/api/reportes/por-categoria` | Gastos agrupados por categoría |
| GET | `/api/reportes/ingresos-por-categoria` | Ingresos agrupados por categoría |
| GET | `/api/reportes/evolucion` | Evolución mensual de ingresos y gastos |
| GET | `/api/reportes/comparativo` | Comparativo mes actual vs anterior |

## ⚙️ Configuración JWT

En `appsettings.json`, cambiar la `SecretKey` por una clave segura en producción:

```json
{
  "JwtSettings": {
    "SecretKey": "TU_CLAVE_SUPER_SECRETA_DE_AL_MENOS_32_CARACTERES",
    "Issuer": "PlanificadorGastos.API",
    "Audience": "PlanificadorGastos.Client",
    "ExpirationInMinutes": 60,
    "RefreshTokenExpirationInDays": 7
  }
}
```

## 🧪 Probar la API

### Con Swagger
1. Ir a `/swagger`
2. Registrar un usuario en `/api/auth/register`
3. Login en `/api/auth/login`
4. Click en "Authorize" → Pegar el token
5. Probar los endpoints

### Con Postman/Insomnia
Importar la colección o crear requests manualmente.

### Ejemplo: Crear un gasto

```bash
curl -X POST https://localhost:5001/api/gastos \
  -H "Authorization: Bearer {tu-token}" \
  -H "Content-Type: application/json" \
  -d '{
    "categoriaId": 1,
    "monto": 1500.50,
    "fecha": "2025-12-18",
    "descripcion": "Supermercado"
  }'
```

## 📝 Comandos Útiles

```bash
# Restaurar paquetes
dotnet restore

# Compilar
dotnet build

# Ejecutar
dotnet run --project src/PlanificadorGastos.API

# Ejecutar en modo watch (hot reload)
dotnet watch run --project src/PlanificadorGastos.API

# Crear migración
dotnet ef migrations add NombreMigracion --project src/PlanificadorGastos.API

# Aplicar migraciones
dotnet ef database update --project src/PlanificadorGastos.API

# Revertir última migración
dotnet ef migrations remove --project src/PlanificadorGastos.API
```

## 🐛 Solución de Problemas

### Error de conexión a SQL Server
- Verificar que SQL Server esté corriendo
- Verificar el connection string
- Asegurar que el firewall permita conexiones

### Error "Certificate not trusted"
Agregar `TrustServerCertificate=True` al connection string

### Migraciones no funcionan
```bash
# Instalar herramienta EF
dotnet tool install --global dotnet-ef
```

## 📄 Licencia

Proyecto personal - Uso libre

---

**Próximo paso:** Crear el frontend con Vue 3 🚀
