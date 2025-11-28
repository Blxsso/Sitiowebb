# Etapa 1: construir el proyecto
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copiamos TODO el repo al contenedor
COPY . .

# Entramos en la carpeta del proyecto (la que contiene Sitioweb.csproj)
WORKDIR /src/Sitiowebb

# Restauramos dependencias
RUN dotnet restore "Sitioweb.csproj"

# Publicamos en modo Release
RUN dotnet publish "Sitioweb.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Etapa 2: imagen final solo con el runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Copiamos lo publicado desde la etapa build
COPY --from=build /app/publish .

# Render usará este puerto interno
ENV ASPNETCORE_URLS=http://+:10000
EXPOSE 10000

# Ejecutar tu app
ENTRYPOINT ["dotnet", "Sitioweb.dll"]