# TableX API

API REST para la gestión de un bar: usuarios, mesas, productos, pedidos y pagos. Es el backend que usará la aplicación de TableX para que los camareros tomen nota y la caja cobre.

Hecha con **ASP.NET Core 8**, **Entity Framework Core** y **SQL Server**, con autenticación por **JWT** y contraseñas cifradas con **BCrypt**.

> **Estado:** en desarrollo. Ahora mismo está implementada la gestión de usuarios y el login. El modelo de datos de mesas, productos, pedidos y pagos ya está definido, pero todavía no tiene endpoints.

---

## Tecnologías

| | |
|---|---|
| Framework | .NET 8 / ASP.NET Core Web API |
| Base de datos | SQL Server (alojada en Somee) |
| ORM | Entity Framework Core 8 |
| Autenticación | JWT Bearer |
| Contraseñas | BCrypt.Net-Next |
| Documentación | Swagger (Swashbuckle) |

---

## Estructura del proyecto

```
TableX_API/
├── Controllers/        # Endpoints de la API
│   ├── AuthController.cs     # Login y usuario actual
│   └── UserController.cs     # Gestión de usuarios
├── Services/           # Lógica de negocio
│   ├── AuthService.cs        # Comprobación de credenciales y generación del JWT
│   └── UserService.cs        # Listar, consultar y crear usuarios
├── DTOs/               # Objetos de entrada y salida de la API
├── Models/             # Entidades de la base de datos y modelos de error
├── Data/
│   └── AppDbContext.cs       # Contexto de EF Core y mapeo de tablas
├── Extensions/         # Métodos de extensión (mapeo a DTO, transacciones, claims)
├── Lib/Consts/         # Constantes: roles, estados, métodos de pago, mensajes de error
└── Program.cs          # Configuración: BD, JWT, Swagger y gestión de errores
```

---

## Roles

| Rol | Qué puede hacer |
|---|---|
| `caja` | Todo: ver la lista de usuarios, consultar cualquier usuario y crear usuarios nuevos |
| `camarero` | Iniciar sesión y consultar sus propios datos |

El **primer usuario** del sistema se puede crear sin iniciar sesión y tiene que tener el rol `caja`. A partir de ahí, solo un usuario `caja` puede crear usuarios nuevos.

---

## Endpoints

### Autenticación — `api/auth`

| Método | Ruta | Acceso | Descripción |
|---|---|---|---|
| `POST` | `/api/auth/login` | Público | Inicia sesión y devuelve el token JWT |
| `GET` | `/api/auth/Me` | Cualquier usuario con sesión | Devuelve los datos del usuario que ha iniciado sesión |

**Ejemplo de login**

```http
POST /api/auth/login
Content-Type: application/json

{
  "username": "eric",
  "password": "1234"
}
```

```json
{
  "token": "token info...",
  "expiresAt": "2026-10-15T08:00:00Z",
  "user": {
    "id": 1,
    "username": "eric",
    "role": "role caja ?? camarero"
  }
}
```

### Usuarios — `api/users`

| Método | Ruta | Acceso | Descripción |
|---|---|---|---|
| `GET` | `/api/users/all` | Solo `caja` | Lista todos los usuarios |
| `GET` | `/api/users/{id}` | Solo `caja` | Devuelve un usuario por su id |
| `POST` | `/api/users/register` | `caja` (o nadie si es el primer usuario) | Crea un usuario nuevo |

**Ejemplo de registro**

```http
POST /api/users/register
Authorization: Bearer <token de un usuario caja>
Content-Type: application/json

{
  "username": "eric2",
  "password": "1234",
  "role": "role caja ?? camarero "
}
```

Validaciones:
- `username`: obligatorio, de 3 a 50 caracteres, sin repetir.
- `password`: obligatoria, de 4 a 100 caracteres. Se guarda cifrada con BCrypt, nunca en texto plano.
- `role`: obligatorio, `caja` o `camarero`.

---

## Autenticación

1. Llama a `POST /api/auth/login` con usuario y contraseña.
2. Guarda el `token` de la respuesta.
3. En las peticiones protegidas, envíalo en la cabecera:
   ```
   Authorization: Bearer <token>
   ```

El token caduca a los **720 minutos** (12 horas). Se puede cambiar en `Jwt:ExpiresMinutes`.

**Desde Swagger:** haz login, copia el token, pulsa el botón **Authorize** 🔒 y pégalo sin escribir "Bearer" delante. Desde ese momento todas las peticiones lo llevan.

---

## Formato de errores

Todos los errores devuelven el mismo formato:

```json
{
  "statusCode": 403,
  "message": "No tienes permiso para realizar esta acción."
}
```

| Código | Cuándo |
|---|---|
| `400` | Datos no válidos |
| `401` | Sin sesión, token caducado o credenciales incorrectas |
| `403` | Sesión iniciada pero sin el rol necesario |
| `404` | El recurso no existe |
| `409` | Conflicto, por ejemplo un nombre de usuario repetido |
| `500` | Error inesperado del servidor |

Los mensajes están centralizados en `Lib/Consts/ErrorMessages.cs`.

---

## Modelo de datos

| Tabla | Descripción |
|---|---|
| `users` | Usuarios del sistema (`username`, `password_hash`, `role`) |
| `mesas` | Mesas del bar, con número único |
| `productos` | Carta: nombre, tipo (`comestible` / `bebida`) y precio |
| `pedidos` | Pedido de una mesa, con el camarero que lo toma y su estado (`abierto` / `cerrado`) |
| `detalle_pedido` | Líneas de cada pedido: producto, cantidad, precio y si está pagada |
| `pagos` | Cobros de un pedido: importe, fecha y método (`efectivo` / `tarjeta`) |

---

## Puesta en marcha

### Requisitos

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- Una base de datos SQL Server con las tablas anteriores
- Visual Studio 2022, o cualquier editor si usas la CLI de `dotnet`

### Configuración

La API lee la conexión y la configuración JWT de `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=SERVIDOR;Initial Catalog=TablexDB;User ID=USUARIO;Password=CONTRASEÑA;Encrypt=True;TrustServerCertificate=True"
  },
  "Jwt": {
    "Key": "una-clave-secreta-de-al-menos-32-caracteres",
    "Issuer": "TablexAPI",
    "Audience": "TablexClient",
    "ExpiresMinutes": 720
  }
}
```

> ⚠️ **No subas credenciales reales al repositorio.** En local es mejor usar los *User Secrets* de .NET:
> ```bash
> dotnet user-secrets init
> dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Data Source=...;Password=..."
> dotnet user-secrets set "Jwt:Key" "tu-clave-secreta"
> ```

### Ejecutar

```bash
dotnet restore
dotnet run --launch-profile https
```

O desde Visual Studio, con el perfil **https**.

Swagger queda disponible en:

```
https://localhost:7206/swagger
```

### Crear el primer usuario

Con la tabla `users` vacía, llama a `POST /api/users/register` **sin token** y con `"role": "caja"`. Después inicia sesión con ese usuario para crear el resto.
