# pgAdmin Configuration Files

This directory contains configuration files for pgAdmin used in Docker Compose.

## Security Notice

The following files contain sensitive credentials and are **excluded from Git**:
- `add_server.py` - Python script with database credentials
- `pgadmin-servers.json` - Server configuration with passwords
- `pgadmin-pgpass` - PostgreSQL password file

## Setup Instructions

1. **Copy dummy files to create your actual configuration:**
   ```bash
   # Copy and configure add_server.py
   cp dummy_add_server.py add_server.py
   # Edit add_server.py with your actual credentials
   
   # Copy and configure pgadmin-servers.json
   cp dummy_pgadmin-servers.json pgadmin-servers.json
   # Edit pgadmin-servers.json with your actual credentials
   
   # Copy and configure pgadmin-pgpass
   cp dummy_pgadmin-pgpass pgadmin-pgpass
   # Edit pgadmin-pgpass with your actual credentials
   ```

2. **Update the files with your actual credentials:**
   - Replace `your-email@example.com` with your pgAdmin email
   - Replace `your-postgres-host` with your PostgreSQL host (e.g., `postgres_server`)
   - Replace `your-database-name` with your database name (e.g., `sudokuCollectiveDB`)
   - Replace `your-username` with your PostgreSQL username
   - Replace `your-password` with your PostgreSQL password

3. **The actual files are already in `.gitignore`** - they will never be committed to Git

## Files

- `dummy_add_server.py` - Template for add_server.py (committed to Git)
- `dummy_pgadmin-servers.json` - Template for pgadmin-servers.json (committed to Git)
- `dummy_pgadmin-pgpass` - Template for pgadmin-pgpass (committed to Git)
- `add_server.py` - Actual script with real credentials (**NOT in Git**)
- `pgadmin-servers.json` - Actual config with real credentials (**NOT in Git**)
- `pgadmin-pgpass` - Password file (**NOT in Git**)

## Usage

After configuring, you can add the PostgreSQL server to pgAdmin by running:
```bash
docker cp add_server.py pgadmin:/tmp/add_server.py
docker exec pgadmin python /tmp/add_server.py
```
