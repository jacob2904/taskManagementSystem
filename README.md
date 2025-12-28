# Task Management Application

Local development setup guide for the Task Management application with .NET backend and React frontend.

---

## Prerequisites

### Required Software
- **.NET 9.0 SDK** - [Download here](https://dotnet.microsoft.com/download/dotnet/9.0)
- **Node.js 18+** - [Download here](https://nodejs.org/)
- **RabbitMQ** - [Download here](https://www.rabbitmq.com/download.html)

### Verify Installation
```bash
dotnet --version  # Should show 9.0.x
node --version    # Should show 18.x or higher
npm --version
```

---

## Database Setup

**No manual migration needed!** The database is automatically created on first run.

The API uses SQLite and runs `EnsureCreated()` on startup, which:
- Creates `TaskManagement.db` in the API directory
- Applies all migrations automatically

---

## Running the Application

### 1. Start RabbitMQ
Ensure RabbitMQ is running on `localhost:5672` (default configuration).

**Windows:**
```bash
# If installed as service, it should auto-start
# Or run manually from installation directory
rabbitmq-server
```

**Verify:** RabbitMQ Management UI at http://localhost:15672 (guest/guest)

---

### 2. Run Backend API

```bash
cd TaskManagement.API
dotnet run
```

**API runs at:** http://localhost:7071  
**Swagger UI:** http://localhost:7071 (opens automatically in dev mode)  
**SignalR Hub:** http://localhost:7071/notificationHub

---

### 3. Run Background Service (Optional)

The background service handles task notifications via RabbitMQ.

```bash
cd TaskManagement.Service
dotnet run
```

This service:
- Listens for task updates from RabbitMQ
- Processes background jobs
- Sends notifications via SignalR

---

### 4. Run Frontend UI

```bash
cd task-management-ui
npm install
npm run dev
```

**UI runs at:** http://localhost:5173

---

## Quick Start Commands

```bash
# Terminal 1: API
cd TaskManagement.API && dotnet run

# Terminal 2: Background Service
cd TaskManagement.Service && dotnet run

# Terminal 3: Frontend
cd task-management-ui && npm install && npm run dev
```

---

## Configuration

### API Configuration (`TaskManagement.API/appsettings.json`)
- **Database:** SQLite (`TaskManagement.db`)
- **RabbitMQ:** localhost:5672
- **JWT:** Pre-configured secret key
- **CORS:** Allows `http://localhost:5173` and `http://localhost:3000`

### Service Configuration (`TaskManagement.Service/appsettings.json`)
- **Database:** Shared SQLite database with API
- **RabbitMQ:** localhost:5672
- **Check Interval:** 1 minute

---

## Project Structure

```
Assignment/
├── TaskManagement.API/          # Main REST API (.NET 9.0)
├── TaskManagement.Service/      # Background worker service
├── TaskManagement.Data/         # EF Core entities & DbContext
├── TaskManagement.Tests/        # Unit tests
└── task-management-ui/          # React frontend (Vite)
```

---

## Troubleshooting

**Database issues:**
- Delete `TaskManagement.db*` files and restart API to recreate

**Port conflicts:**
- API: Change port in `launchSettings.json`
- Frontend: Change port in `vite.config.ts`

**RabbitMQ connection failed:**
- Ensure RabbitMQ is running: `netstat -an | findstr 5672`
- Check credentials in `appsettings.json` (default: guest/guest)

---

## Development Notes

- **Hot Reload:** Frontend supports Vite HMR
- **API Swagger:** Full API documentation at http://localhost:7071
- **Authentication:** JWT-based auth with SignalR support
- **Real-time Updates:** SignalR hub at `/notificationHub`
