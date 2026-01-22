import sqlite3

# DUMMY FILE - DO NOT USE IN PRODUCTION
# Copy this file to 'add_server.py' and update with your actual credentials
# This script is used to manually add a PostgreSQL server to pgAdmin's database

conn = sqlite3.connect('/var/lib/pgadmin/pgadmin4.db')
cursor = conn.cursor()

# Get user ID - Update with your pgAdmin email
cursor.execute("SELECT id FROM user WHERE email = 'your-email@example.com'")
result = cursor.fetchone()
if result:
    user_id = result[0]
    
    # Check if server already exists
    cursor.execute("SELECT id FROM server WHERE user_id = ? AND name = 'Your Server Name'", (user_id,))
    existing = cursor.fetchone()
    
    if existing:
        print("Server already exists")
    else:
        # Insert server - Update all values with your actual server details
        cursor.execute("""
            INSERT INTO server (
                user_id, servergroup_id, name, host, port, 
                maintenance_db, username, password, save_password
            ) VALUES (?, 1, ?, ?, ?, ?, ?, ?, 1)
        """, (
            user_id,
            'Your Server Name',        # Update with your server name
            'your-postgres-host',      # Update with your PostgreSQL host
            5432,                      # Update with your PostgreSQL port
            'your-database-name',      # Update with your database name
            'your-username',           # Update with your PostgreSQL username
            'your-password'            # Update with your PostgreSQL password
        ))
        conn.commit()
        print("Server added successfully!")
else:
    print("User not found")

conn.close()
