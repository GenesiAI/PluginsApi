# Migrations
Add migrations with 
```
dotnet ef migrations add "XYZ"  --output-dir .\Migrations\ --namespace AiPlugin.Migrations --project .\Infrastructure\Infrastructure.csproj --startup-project .\Api\Api.csproj -v -- --environment Development
```

Apply them with 

```
dotnet ef database update --project .\Infrastructure\Infrastructure.csproj --startup-project .\Api\Api.csproj -v -- --environment Development 
```

Change the environment with `--environment Development` to `--environment Staging` or `--environment Production` to apply the migrations to the other database