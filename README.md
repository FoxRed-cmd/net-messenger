# net-messenger

<p style="text-align: center">
    <img alt="Static Badge" src="https://img.shields.io/badge/.NET-9.0-4E2ACD">
</p>

## Add new project to solution

```PowerShell
dotnet sln add .\Project\Project.csproj
```

## Init and set user secrets

Location: %APPDATA%\Microsoft\UserSecrets\<user_secrets_id>\secrets.json

```PowerShell
dotnet user-secrets init --project .\AuthService
```

```PowerShell
dotnet user-secrets set "Key:SubKey" "Value" --project .\AuthService
```

## Command to start container with postgres

```PowerShell
docker run --name <container_name> -e POSTGRES_DB=<your_db_name> -e POSTGRES_USER=<your_user> -e POSTGRES_PASSWORD=<your_password> -p 5432:5432 -d postgres
```
