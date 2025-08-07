# Sudoku Collective

Sudoku Collective is an open source Web API that is used to learn frontend client side technologies such as [React](https://reactjs.org/) or [Vue.js](https://vuejs.org/). With this API developers will create an app that allows players to play [sudoku](https://en.wikipedia.org/wiki/Sudoku) puzzles and compare their performance against other players. The benefit of using this tool is that once the developer creates their first app they will obtain an understanding of how the API works and will then be better able to compare and understand various frontend technologies like [React](https://reactjs.org/) or [Vue.js](https://vuejs.org/). The API is [fully documented](https://sudokucollective.com/swagger/index.html) so developers can integrate their client apps with the API. The goals are to learn, develop and have fun!

## Requirements

- [.Net 8.0](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Docker](https://www.docker.com/) (recommended for easier setup)

**Or for local development without Docker:**
- [PostgreSQL 14](https://www.postgresql.org/download/)
- [Redis Server - version 6.2.7](https://redis.io/download)

For the Redis Cache Server on Windows 10 it is recommended you use [Windows Subsystem for Linux](https://docs.microsoft.com/en-us/windows/wsl/install-win10) in order to install and run Redis through Ubuntu on [Windows Subsystem for Linux](https://docs.microsoft.com/en-us/windows/wsl/install-win10).  The instructions for downloading from the official Ubuntu PPA are contained in the above Redis link.  For Windows 11 you can install and configure a local Redis Server using [Docker](https://www.docker.com/).

## Installation

### Option 1: Docker Compose (Recommended)

The easiest way to get started is using Docker Compose, which will set up all required services automatically:

1. **Clone the repository**
   ```bash
   git clone <repository-url>
   cd SudokuCollective
   ```

2. **Configure the application settings**
   - Rename `dummysettings.json` to `appsettings.json` and update the values where it states `[Your value here]`
   - Follow the same process for the respective appsetting environment files (`appsettings.Development.json`, `appsettings.Staging.json`, `appsettings.Production.json`)
   - For the Test project, rename `appsettings.Test.json` dummy file

3. **Start all services with Docker Compose**
   ```bash
   docker-compose up -d
   ```

   This will start:
   - **API Server** - Available at `http://localhost:5000` (HTTP) and `https://localhost:5001` (HTTPS)
   - **PostgreSQL Database** - Available at `localhost:5432`
   - **Redis Cache** - Available at `localhost:6379`
   - **PGAdmin** - Database administration tool available at `http://localhost:8080`

4. **Initialize the database**
   ```bash
   dotnet ef database update
   ```

5. **Access PGAdmin (Database Management)**
   - Open `http://localhost:8080` in your browser
   - Login with the default credentials (configure these in docker-compose.yml)
   - Add your PostgreSQL server using:
     - Host: `postgres_server`
     - Port: `5432`
     - Database: `sudokuCollectiveDB`
     - Username: `sudokucollectivedbo`

### Option 2: Manual Installation

In the API project you will find a **dummysettings.json** file that is a stand in for the **appsettings.json** file that configures the project.  Additionally you will find dummy files for the **appsettings.Development.json**, **appsettings.Staging.json**, and **appsettings.Production.json** files and **appsettings.Test.json** in the Test project.  Simply rename the **dummysettings.json** to **appsettings.json** and place your value where it states **[Your value here]**.  Following the same process for the respective appsetting environment files.

There is also a dummy file for docker-compose, **docker-compose-dummy.yml**.  Simply rename **docker-compose-dummy.yml** to **docker-compose.yml** and place your values for **POSTGRES_USER**, **POSTGRES_PASSWORD**, **POSTGRES_DB**, **ConnectionStrings_DatabaseConnection**, and **ConnectionStrings_CacheConnection** where it states **[Your value here]**.

For the **License** field in **DefaultAdminApp**, **DefaultClientApp**, and **DefaultSandboxApp** you can enter a hexadecimal value, random values can be generated [here](https://www.guidgenerator.com/online-guid-generator.aspx), braces shouldn't be included and you should use hyphens.

Once the above is done run the following command to instantiate the database:

`dotnet ef database update`

Once done the project will be ready to run.

## Docker Services

The application uses Docker Compose to orchestrate the following services:

| Service | Description | Port | Purpose |
|---------|-------------|------|---------|
| **sudokucollective.api** | Main API application | 5000 (HTTP), 5001 (HTTPS) | Core Web API |
| **postgres_server** | PostgreSQL database | 5432 | Data persistence |
| **redis_server** | Redis cache | 6379 | Caching and session storage |
| **pgadmin** | Database administration | 8080 | Database management interface |

### Useful Docker Commands

- **Start all services**: `docker-compose up -d`
- **Stop all services**: `docker-compose down`
- **View logs**: `docker-compose logs [service-name]`
- **Rebuild and restart**: `docker-compose up --build -d`
- **View running containers**: `docker-compose ps`

### Connecting to Docker Containers via Bash

Sometimes you need to access the inside of a running container for debugging, troubleshooting, or exploring the environment. Here's how to connect to each service:

#### Connect to the API Container
```bash
# Connect to the main API application container
docker exec -it sudokucollective.api-1 bash

# Alternative: If the container name is different, find it first
docker ps
docker exec -it <container-name> bash
```

#### Connect to PostgreSQL Container
```bash
# Connect to PostgreSQL container
docker exec -it postgres_server bash

# Connect directly to PostgreSQL database
docker exec -it postgres_server psql -U sudokucollectivedbo -d sudokuCollectiveDB
```

#### Connect to Redis Container
```bash
# Connect to Redis container
docker exec -it redis_server bash

# Connect directly to Redis CLI
docker exec -it redis_server redis-cli
```

#### Connect to PGAdmin Container
```bash
# Connect to PGAdmin container
docker exec -it pgadmin bash
```

#### Useful Commands Inside Containers

**Inside the API Container:**
```bash
# Check .NET version
dotnet --version

# List application files
ls -la /app

# Check environment variables
env | grep -i connection

# View application logs
cat /app/logs/app.log  # If logging to file is configured
```

**Inside PostgreSQL Container:**
```bash
# List all databases
psql -U sudokucollectivedbo -l

# Connect to the application database
psql -U sudokucollectivedbo -d sudokuCollectiveDB

# Inside psql, useful commands:
# \dt  - List all tables
# \d <table_name>  - Describe a table
# \q  - Quit psql
```

**Inside Redis Container:**
```bash
# Test Redis connection
redis-cli ping

# View all keys
redis-cli keys "*"

# Monitor Redis commands in real-time
redis-cli monitor
```

#### Troubleshooting Tips

1. **Check if containers are running:**
   ```bash
   docker-compose ps
   ```

2. **View container logs:**
   ```bash
   # View logs for a specific service
   docker-compose logs sudokucollective.api
   docker-compose logs postgres_server
   docker-compose logs redis_server
   docker-compose logs pgadmin
   
   # Follow logs in real-time
   docker-compose logs -f sudokucollective.api
   ```

3. **Restart a specific service:**
   ```bash
   docker-compose restart sudokucollective.api
   ```

4. **Check container resource usage:**
   ```bash
   docker stats
   ```

## Development

There is also a related [Vue.js](https://vuejs.org/) administrative app which allows you to manage app licenses, [Sudoku Collective Admin Console](https://github.com/Joseph-Anthony-King/SudokuCollective.Admin).  The installation instructions for that project can be reviewed in its [README](https://github.com/Joseph-Anthony-King/SudokuCollective.Admin/blob/master/README.md) file.
