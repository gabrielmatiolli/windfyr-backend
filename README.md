#iniciar projeto e rodar migrations
dotnet ef database update --project Windfyr.Repository --startup-project Windfyr.WebApi; dotnet run --project Windfyr.WebApi

#iniciar 
dotnet run --project Windfyr.WebApi