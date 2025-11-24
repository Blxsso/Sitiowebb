# Etapa de compilación
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copiar todo el código
COPY . .

# Publicar en modo Release dentro de la carpeta out
RUN dotnet publish -c Release -o out

# Etapa de runtime (donde correrá la app)
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

# Copiar los archivos publicados desde la etapa de build
COPY --from=build /app/out .

# Ejecutar tu aplicación
ENTRYPOINT ["dotnet", "Sitiowebb.dll"]