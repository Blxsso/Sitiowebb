# Imagen base del SDK para compilar el proyecto
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copiar csproj y restaurar dependencias
COPY *.sln .
COPY Sitiowebb/*.csproj ./Sitiowebb/
RUN dotnet restore

# Copiar el resto del código y compilar
COPY . .
WORKDIR /app/Sitiowebb
RUN dotnet publish -c Release -o /out

# Imagen final para ejecutar la aplicación
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /out .

# Puerto en el que correrá la app en Render
ENV ASPNETCORE_URLS=http://+:10000
EXPOSE 10000

ENTRYPOINT ["dotnet", "Sitiowebb.dll"]
