# LINDE Registro API

Web API REST para gestionar los **productores** agrícolas y sus **predios**, basada en los casos de uso CU-02 (Gestionar productores) y CU-03 (Gestionar predios) del sistema LINDE, un sistema de gestión de polígonos de predios para la certificación de exportación en Santa Cruz, Bolivia. Un productor puede tener muchos predios y cada predio pertenece a un solo productor.

## Versiones

| Componente | Versión |
|---|---|
| .NET | 10 (`net10.0`) |
| Entity Framework Core (SqlServer y Design) | 10.0.12 |
| Swashbuckle.AspNetCore (Swagger) | 10.3.1 |
| Base de datos | SQL Server (LocalDB, Express o Docker) |

## Integrantes

Equipo "Devs", Programación Web III, Univalle.

| Integrante | Usuario de GitHub | Parte que desarrolló |
|---|---|---|
| Mauricio Urioste Hurtado | MauricioUri06 | Proyecto base, modelos, DbContext con Fluent API, migración y datos de prueba |
| Josep Gonzalo Rodriguez Vargas | Amaranto-3232 | DTO y controlador de productores |
| Josue Carlos Soliz Mamani | JOSHUAUwU92 | DTO y controlador de predios, regla del dominio y recurso relacionado |
| Sebastian Maldonado Salazar | TheRealHarambe | Comentarios XML en Swagger y primera versión del archivo de pruebas `.http` |

## Repositorio

https://github.com/MauricioUri06/API-WEB-LINDE

## Requisitos

- SDK de .NET 10.
- SQL Server: LocalDB (viene con Visual Studio), SQL Server Express o un contenedor Docker.
- Herramienta de migraciones de EF Core: `dotnet tool install --global dotnet-ef`.

## Configurar la conexión a SQL Server

La cadena de conexión se llama `LindeDb`. En `appsettings.json` viene configurada para **LocalDB**, sin usuario ni contraseña:

```json
"ConnectionStrings": {
  "LindeDb": "Server=(localdb)\\MSSQLLocalDB;Database=LindeRegistroDb;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
}
```

Si usas otra instancia, no modifiques `appsettings.json`: guarda tu cadena con *user secrets*, que quedan fuera del repositorio. Dentro de la carpeta del proyecto:

```
dotnet user-secrets init
```

SQL Server Express:

```
dotnet user-secrets set "ConnectionStrings:LindeDb" "Server=localhost\SQLEXPRESS;Database=LindeRegistroDb;Trusted_Connection=True;TrustServerCertificate=True"
```

Docker (reemplaza `<TU_CONTRASEÑA>` por la de tu contenedor):

```
dotnet user-secrets set "ConnectionStrings:LindeDb" "Server=localhost,1433;Database=LindeRegistroDb;User Id=sa;Password=<TU_CONTRASEÑA>;TrustServerCertificate=True"
```

## Aplicar las migraciones

```
git clone https://github.com/MauricioUri06/API-WEB-LINDE.git
cd API-WEB-LINDE
dotnet tool install --global dotnet-ef
dotnet ef database update
```

Se crea la base `LindeRegistroDb` con las tablas `Productores` y `Predios`, el índice único del CI, la clave foránea con borrado restringido, las restricciones CHECK de superficie y desmonte, y los datos de prueba.

## Datos de prueba

Se cargan solos con la migración (`HasData`).

| Id | Productor | CI | Teléfono |
|---|---|---|---|
| 1 | Ana Mendoza | 4567891 | 70011223 |
| 2 | Luis Justiniano | 5678912 | 70022334 |
| 3 | Rosa Vargas | 6789123 | |

| Id | Predio | Municipio | Superficie (ha) | Desmonte (ha) | Estado | Productor |
|---|---|---|---|---|---|---|
| 1 | El Algarrobo | San Julián | 120.50 | 0 | Registrado | 1 |
| 2 | La Esperanza | Cuatro Cañadas | 45.00 | 10.00 | Registrado | 1 |
| 3 | Santa Rosa | Pailón | 300.00 | | Aprobado | 2 |
| 4 | Los Tajibos | Okinawa Uno | 3.50 | | Registrado | 3 |
| 5 | San Antonio | Mineros | 80.25 | 20.00 | Observado | 3 |

## Ejecutar y abrir Swagger

```
dotnet run
```

Abrir http://localhost:5282/swagger. Con el perfil `https` también responde en https://localhost:7175/swagger.

## Endpoints

| Método | Ruta | Descripción | Códigos |
|---|---|---|---|
| GET | `/api/productores` | Lista los productores | 200 |
| GET | `/api/productores/{id}` | Consulta un productor | 200, 404 |
| POST | `/api/productores` | Registra un productor | 201 (con Location), 400 |
| PUT | `/api/productores/{id}` | Actualiza un productor | 204, 400, 404 |
| DELETE | `/api/productores/{id}` | Elimina un productor sin predios | 204, 404, 409 |
| GET | `/api/predios?municipio=` | Lista los predios, con filtro opcional por municipio | 200 |
| GET | `/api/predios/{id}` | Consulta un predio | 200, 404 |
| POST | `/api/predios` | Registra un predio (estado inicial Registrado) | 201 (con Location), 400 |
| PUT | `/api/predios/{id}` | Actualiza un predio (no cambia su estado) | 204, 400, 404 |
| DELETE | `/api/predios/{id}` | Elimina un predio | 204, 404 |
| GET | `/api/productores/{productorId}/predios` | Recurso relacionado: predios de un productor | 200, 404 |

Reglas que valida la API:

- Campos obligatorios y formatos con DataAnnotations (el CI debe tener de 5 a 10 dígitos).
- CI único entre productores.
- Referencia inválida: no se puede crear ni actualizar un predio con un productor que no existe (400).
- Regla del dominio: el desmonte declarado no puede ser mayor que la superficie del predio (400).
- No se puede eliminar un productor que tiene predios (409).
- Todos los errores usan el formato ProblemDetails (RFC 7807).

## Estructura del proyecto

```
API-WEB-LINDE/
├── Controllers/
│   ├── ProductoresController.cs
│   └── PrediosController.cs
├── DTOs/
│   ├── Productores/   (ProductorCrearDto, ProductorActualizarDto, ProductorRespuestaDto)
│   └── Predios/       (PredioCrearDto, PredioActualizarDto, PredioRespuestaDto)
├── Models/            (Productor, Predio, EstadoPredio)
├── Data/
│   ├── LindeDbContext.cs
│   ├── DatosPrueba.cs
│   └── Configuraciones/ (ProductorConfiguracion, PredioConfiguracion)
├── Migrations/
├── docs/
│   ├── Evidencias_LINDE_Registro_API.pdf
│   └── capturas/
├── Properties/launchSettings.json
├── LINDE.Registro.Api.http
├── LINDE.Registro.Api.csproj
├── Program.cs
├── appsettings.json
└── appsettings.Development.json
```

## Evidencias

- PDF con las pruebas de éxitos, errores y persistencia: [docs/Evidencias_LINDE_Registro_API.pdf](docs/Evidencias_LINDE_Registro_API.pdf).
- Archivo de pruebas para Rider o Visual Studio: [LINDE.Registro.Api.http](LINDE.Registro.Api.http).
