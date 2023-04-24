# Ai plugin, making AI accessible to everyone - even your grandma!

all nice stuffs here

## Migrate
migrations with 
`
dotnet ef migrations add Initial --output-dir ../Infrastructure/Migrations --namespace AiPlugin.Migrations --project ../Infrastructure/Infrastructure.csproj --startup-project .\Api.csproj -- --env Development 
`
apply them with 
`
dotnet ef database update --project ../Infrastructure/Infrastructure.csproj --startup-project .\Api.csproj -- --env Development 
`