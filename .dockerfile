FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copia los archivos del proyecto
COPY *.sln .
COPY GestionFinanciera/*.csproj ./GestionFinanciera/
RUN dotnet restore

COPY . .
WORKDIR /app/GestionFinanciera
RUN dotnet publish -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/GestionFinanciera/out .

# Render espera el puerto 8080 por defecto [citation:4]
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "GestionFinanciera.dll"]