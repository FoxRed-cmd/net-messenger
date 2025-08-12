# net-messenger

## Command to start container with postgres
```PowerShell
docker run --name <container_name> -e POSTGRES_DB=<your_db_name> -e POSTGRES_USER=<your_user> -e POSTGRES_PASSWORD=<your_password> -p 5432:5432 -d postgres
```