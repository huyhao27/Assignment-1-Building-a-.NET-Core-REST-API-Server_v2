# Game Inventory API — GNS301 Assignment 1

A RESTful Web API built with **ASP.NET Core (.NET 8)** and **MongoDB**, using a 3-layer architecture
(Controller → Service → Repository), JWT authentication and role-based authorization (Admin / Player).

## Tech stack

| Component | Version |
|---|---|
| .NET SDK | 8.0 (pinned in `global.json`) |
| MongoDB.Driver | 3.1.0 |
| Swashbuckle.AspNetCore | 7.2.0 |
| Microsoft.AspNetCore.Authentication.JwtBearer | 8.0.0 |
| System.IdentityModel.Tokens.Jwt | 8.0.0 |
| FluentValidation (+ DependencyInjectionExtensions) | 11.11.0 |
| MongoDB | `mongo:latest` in Docker |

## Project structure

```
GameInventoryApi/
├── Controllers/    AuthController, InventoryController (Admin), PlayerProfileController (Player)
├── Services/       AuthService (JWT), InventoryService, PlayerProfileService
├── Repositories/   IMongoRepository<T>, MongoRepository<T>
├── Models/         User, InventoryItem, PlayerProfile, MongoDbSettings, JwtSettings
├── DTOs/           LoginDto, AuthResponseDto, InventoryItemDto, PlayerProfileDto, PatchInventoryItemDto
├── Validators/     FluentValidation rules for each request body
├── Filters/        ValidationFilter (runs the validators before every action)
├── Data/           SeedData (runs on every startup)
└── Program.cs      DI, JWT, Swagger, middleware
```

## How to run

1. Start MongoDB in Docker (either command works):

   ```bash
   docker compose up -d
   ```

   ```bash
   docker run -d --name game-mongo -p 27017:27017 -e MONGO_INITDB_ROOT_USERNAME=admin -e MONGO_INITDB_ROOT_PASSWORD=password mongo:latest
   ```

   Check it is running with `docker ps`.

2. Run the API:

   ```bash
   cd GameInventoryApi
   dotnet run --launch-profile https
   ```

3. Open Swagger: <https://localhost:7258/swagger> (or <http://localhost:5141/swagger> with the `http` profile).

> On every startup `SeedData` **wipes** the `Users`, `InventoryItems` and `PlayerProfiles` collections and re-inserts the test data.

## Seeded accounts

| Username | Password | Role | Data |
|---|---|---|---|
| admin | admin123 | Admin | — |
| player1 | player123 | Player | Profile (Level 10, 2450 XP) + 1 inventory item (Iron Sword) |
| player2 | player123 | Player | No profile (GET profile returns 404) |

## Endpoints

| Method | Route | Access | Description |
|---|---|---|---|
| POST | `/api/Auth/login` | Anonymous | Returns a JWT token |
| GET | `/api/Inventory` | Admin | List all inventory items |
| GET | `/api/Inventory/{id}` | Admin | Get one item |
| POST | `/api/Inventory` | Admin | Create an item (201 Created) |
| PUT | `/api/Inventory/{id}` | Admin | Replace an item (204) |
| PATCH | `/api/Inventory/{id}` | Admin | Partially update an item, returns the updated item (200) |
| DELETE | `/api/Inventory/{id}` | Admin | Delete an item (204) |
| GET | `/api/PlayerProfile` | Player | Get **own** profile (identified by the `PlayerId` claim in the token) |
| PUT | `/api/PlayerProfile` | Player | Update **own** profile; 403 if `playerId` in the body is not the caller's |

## Bonus

### PATCH (`PATCH /api/Inventory/{id}`)

Unlike PUT, which replaces the whole document, PATCH only changes the fields present in the body
(`PatchInventoryItemDto`, all fields nullable). `LastUpdated` is refreshed automatically.

```json
{ "quantity": 7 }
```

### FluentValidation

Every request body is validated before it reaches the controller. `ValidationFilter` looks up an
`IValidator<T>` for each action argument; validators are registered with `AddValidatorsFromAssemblyContaining<Program>()`.
An invalid body returns **400** with the standard `ValidationProblemDetails` shape:

```json
{
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": { "Quantity": ["'Quantity' must be between 0 and 9999. You entered -5."] }
}
```

| Body | Rules |
|---|---|
| `LoginDto` | Username, Password not empty |
| `InventoryItem` (POST/PUT) | ItemId, Name, PlayerId not empty; Quantity 0–9999 |
| `PatchInventoryItemDto` | At least one field; any field sent must satisfy the same rules as above |
| `PlayerProfile` (PUT) | Id, PlayerId, Username not empty; Level 1–100; Experience ≥ 0 |

## Testing with Swagger

1. `POST /api/Auth/login` with `{ "username": "admin", "password": "admin123" }` and copy `token`.
2. Click **Authorize** and enter `Bearer <token>` (the word `Bearer`, a space, then the token).
3. Call the Inventory endpoints.
4. Log in as `player1` / `player123`, authorize again with the new token and call the PlayerProfile endpoints.

`GameInventoryApi/GameInventoryApi.http` contains the same requests for the VS / VS Code REST client.

## Test results

Verified end-to-end against a running MongoDB:

| Scenario | Expected | Result |
|---|---|---|
| Login with wrong password | 401 | ✅ 401 |
| Login admin / player1 | 200 + token | ✅ |
| GET Inventory without token | 401 | ✅ 401 |
| GET Inventory as player1 | 403 | ✅ 403 |
| Admin: GET all / POST / GET by id / PUT / DELETE | 200 / 201 / 200 / 204 / 204 | ✅ |
| GET deleted item | 404 | ✅ 404 |
| GET PlayerProfile as admin | 403 | ✅ 403 |
| player1 GET / PUT own profile | 200 / 204, change persisted | ✅ |
| player1 PUT with another `playerId` | 403 | ✅ 403 |
| player2 PUT player1's profile | 403 | ✅ 403 |
| PATCH `{"quantity":7}` then `{"name":"Steel Sword"}` | 200, other fields kept | ✅ |
| PATCH unknown id / PATCH as player | 404 / 403 | ✅ |
| Login with empty username/password | 400 | ✅ 400 |
| POST item with empty fields and quantity -5 | 400, 4 errors | ✅ 400 |
| PATCH `{}` / PATCH quantity 100000 | 400 | ✅ 400 |
| PUT profile with Level 999, Experience -1 | 400 | ✅ 400 |

## Deviations from the handout (needed for the code to compile and run)

1. **Repository registration** — `MongoRepository<T>` takes a `string collectionName`, which the DI container cannot
   resolve, so `AddScoped(typeof(IMongoRepository<>), typeof(MongoRepository<>))` throws at runtime.
   Each repository is registered with a factory and an explicit collection name matching `SeedData`.
2. **`_id` filter** — `Builders<T>.Filter.Eq("_id", id)` compares against a raw string, which never matches the
   stored `ObjectId`; GET/PUT/DELETE by id silently found nothing. `MongoRepository.IdFilter` converts the id first.
3. **`?? NotFound()`** — `InventoryItem ?? NotFoundResult` does not compile (no common type);
   replaced with `if (x is null) return NotFound();`.
4. Added the missing `using` directives and namespaces (`SeedData` lives in `GameInventoryApi.Data`).

## Known limitations

- Passwords are stored in plain text (demo only, as stated in the handout).
- A player can still set any valid `Level` (1–100) / `Experience` through `PUT /api/PlayerProfile`; a real game should compute these on the server.
- The JWT secret is in `appsettings.json`; in production it belongs in user secrets or environment variables.
