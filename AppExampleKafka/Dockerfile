FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY AppExampleKafka.Domain/AppExampleKafka.Domain.csproj AppExampleKafka.Domain/
COPY AppExampleKafka.Application/AppExampleKafka.Application.csproj AppExampleKafka.Application/
COPY AppExampleKafka.Infraestructure/AppExampleKafka.Infraestructure.csproj AppExampleKafka.Infraestructure/
COPY AppExampleKafka.API/AppExampleKafka.API.csproj AppExampleKafka.API/
RUN dotnet restore AppExampleKafka.API/AppExampleKafka.API.csproj
COPY AppExampleKafka.Domain/ AppExampleKafka.Domain/
COPY AppExampleKafka.Application/ AppExampleKafka.Application/
COPY AppExampleKafka.Infraestructure/ AppExampleKafka.Infraestructure/
COPY AppExampleKafka.API/ AppExampleKafka.API/
RUN dotnet publish AppExampleKafka.API/AppExampleKafka.API.csproj -c Release --no-restore -o /app
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app .
USER $APP_UID
EXPOSE 8080
ENTRYPOINT ["dotnet", "AppExampleKafka.API.dll"]
