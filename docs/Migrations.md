# Migrations
Add migrations with 
```
dotnet ef migrations add "XYZ"  --output-dir .\Migrations\ --namespace AiPlugin.Migrations --project .\Infrastructure\Infrastructure.csproj --startup-project .\Api\Api.csproj -- --env Development
```

Apply them with 

```
dotnet ef database update --project .\Infrastructure\Infrastructure.csproj --startup-project .\Api\Api.csproj -- --env Development 
```

Change the environment with `--env Development` to `--env Staging` or `--env Production` to apply the migrations to the other database