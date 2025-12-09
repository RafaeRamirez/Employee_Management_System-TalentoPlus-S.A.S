FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY TalentoPlus.sln .
COPY TalentoPlus.Domain/TalentoPlus.Domain.csproj TalentoPlus.Domain/
COPY TalentoPlus.Application/TalentoPlus.Application.csproj TalentoPlus.Application/
COPY TalentoPlus.Infrastructure/TalentoPlus.Infrastructure.csproj TalentoPlus.Infrastructure/
COPY TalentoPlus.Web/TalentoPlus.Web.csproj TalentoPlus.Web/
RUN dotnet restore TalentoPlus.Web/TalentoPlus.Web.csproj
COPY . .
RUN dotnet publish TalentoPlus.Web/TalentoPlus.Web.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "TalentoPlus.Web.dll"]
