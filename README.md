# Task Management Application

Local development setup guide for the Task Management application with .NET backend and React frontend.

---

## Prerequisites

### Required Software
- **.NET 9.0 SDK** - [Download here](https://dotnet.microsoft.com/download/dotnet/9.0)
- **Node.js 18+** - [Download here](https://nodejs.org/)
- **Docker Desktop** - [Download here](https://www.docker.com/products/docker-desktop)
- **RabbitMQ** - [Download here](https://www.rabbitmq.com/download.html)

### Verify Installation
```bash
dotnet --version  # Should show 9.0.x
node --version    # Should show 18.x or higher
npm --version
docker --version
```

---

## Database Setup

This application uses **SQL Server 2022** running in a Docker container.

### 1. Start Docker Desktop
- Launch Docker Desktop from your Start menu
- Wait until the whale icon in the system tray is steady (not animated)
- Verify Docker is running: `docker ps`

### 2. Run SQL Server Container

```bash
docker run -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=YourStrong@Passw0rd" -p 1433:1433 --name sqlserver -d mcr.microsoft.com/mssql/server:2022-latest
```

**What this does:**
- Downloads SQL Server 2022 container image (first run only)
- Creates a container named `sqlserver`
- Sets SA password to `YourStrong@Passw0rd`
- Exposes SQL Server on port `1433`
- Runs in detached mode (background)

### 3. Verify SQL Server is Running

```bash
docker ps
```

You should see the `sqlserver` container with status `Up`.

### 4. Database Migration

**No manual migration needed!** The API automatically applies migrations on startup (line 115 in `Program.cs`).

The first time you run the API, it will:
- Create the `TaskManagementDb` database
- Apply all EF Core migrations
- Create tables: Tasks, Tags, UserDetails, TaskTags

---

## Managing SQL Server Container

### Stop SQL Server
```bash
docker stop sqlserver
```

### Start Existing Container
```bash
docker start sqlserver
```

### Remove Container (deletes all data)
```bash
docker stop sqlserver
docker rm sqlserver
```

### View SQL Server Logs
```bash
docker logs sqlserver
```

---

## Running the Application

### 1. Start Docker & SQL Server
Ensure Docker Desktop is running and SQL Server container is started:
```bash
docker start sqlserver
```

### 2. Start RabbitMQ
Ensure RabbitMQ is running on `localhost:5672` (default configuration).

**Windows:**
```bash
# If installed as service, it should auto-start
# Or run manually from installation directory
rabbitmq-server
```

**Verify:** RabbitMQ Management UI at http://localhost:15672 (guest/guest)

---

### 3. Run Backend API

```bash
cd TaskManagement.API
dotnet run
```

**API runs at:** http://localhost:7071  
**Swagger UI:** http://localhost:7071 (opens automatically in dev mode)  
**SignalR Hub:** http://localhost:7071/notificationHub

**On first run:** The API will automatically create the database and apply migrations.

---

### 4. Run Background Service

The background service handles task notifications and overdue task checks via RabbitMQ.

```bash
cd TaskManagement.Service
dotnet run
```

This service:
- Checks for overdue tasks every minute
- Sends notifications via RabbitMQ
- Processes background jobs

**Note:** The service requires the database to be initialized by the API first.

---

### 5. Run Frontend UI

```bash
cd task-management-ui
npm install
npm run dev
```

**UI runs at:** http://localhost:5173

---

## Quick Start Commands

```bash
# Terminal 1: SQL Server (if not already running)
docker start sqlserver

# Terminal 2: API (starts first to create database)
cd TaskManagement.API && dotnet run

# Terminal 3: Background Service (start after API initializes DB)
cd TaskManagement.Service && dotnet run

# Terminal 4: Frontend
cd task-management-ui && npm install && npm run dev
```

---

## Configuration

### Database Connection Strings

Both the API and Service use the same SQL Server database:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost,1433;Database=TaskManagementDb;User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=True;MultipleActiveResultSets=true"
}
```

**Connection Details:**
- **Server:** localhost:1433
- **Database:** TaskManagementDb
- **User:** sa
- **Password:** YourStrong@Passw0rd

### API Configuration (`TaskManagement.API/appsettings.json`)
- **Database:** SQL Server (TaskManagementDb)
- **RabbitMQ:** localhost:5672
- **JWT:** Pre-configured secret key
- **CORS:** Allows `http://localhost:5173` and `http://localhost:3000`

### Service Configuration (`TaskManagement.Service/appsettings.json`)
- **Database:** SQL Server (TaskManagementDb)
- **RabbitMQ:** localhost:5672
- **Check Interval:** 1 minute (configurable)

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

### Docker/SQL Server Issues

**Error: "Docker daemon not accessible"**
- Start Docker Desktop and wait for it to fully initialize
- Verify with: `docker ps`

**Error: "port 1433 already allocated"**
- Another SQL Server instance is running
- Stop the conflicting service or use a different port
- Check with: `netstat -an | findstr 1433`

**Error: "no such table: Tasks"**
- Database not initialized
- Run the API first (it auto-creates the database)
- Check SQL Server is running: `docker ps`

**Container won't start**
- Remove and recreate: `docker rm sqlserver` then run the docker run command again
- Check logs: `docker logs sqlserver`

### Database Issues

**Reset database completely:**
```bash
# Stop and remove container (deletes all data)
docker stop sqlserver
docker rm sqlserver

# Create new container
docker run -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=YourStrong@Passw0rd" -p 1433:1433 --name sqlserver -d mcr.microsoft.com/mssql/server:2022-latest

# Restart API to recreate database
cd TaskManagement.API && dotnet run
```

### RabbitMQ Connection Failed
- Ensure RabbitMQ is running: `netstat -an | findstr 5672`
- Check credentials in `appsettings.json` (default: guest/guest)
- Verify RabbitMQ Management UI: http://localhost:15672

### Port Conflicts
- **API:** Change port in `launchSettings.json`
- **Frontend:** Change port in `vite.config.ts`
- **SQL Server:** Use different port in docker run: `-p 1434:1433`

---

## Development Notes

- **Hot Reload:** Frontend supports Vite HMR
- **API Swagger:** Full API documentation at http://localhost:7071
- **Authentication:** JWT-based auth with SignalR support
- **Real-time Updates:** SignalR hub at `/notificationHub`
- **Database:** SQL Server 2022 with automatic migrations
- **Background Jobs:** Overdue task checking runs every 1 minute (configurable)

---

## Security Notes

**⚠️ For Development Only:**
- The SQL Server password (`YourStrong@Passw0rd`) is for local development only
- Never use these credentials in production
- Change JWT secret key before deploying to production
- Enable HTTPS in production environments
