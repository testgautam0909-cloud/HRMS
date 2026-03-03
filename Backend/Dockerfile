FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY src/HRMS.Domain/HRMS.Domain.csproj HRMS.Domain/
COPY src/HRMS.Shared/HRMS.Shared.csproj HRMS.Shared/
COPY src/HRMS.Application/HRMS.Application.csproj HRMS.Application/
COPY src/HRMS.Infrastructure/HRMS.Infrastructure.csproj HRMS.Infrastructure/
COPY src/HRMS.API/HRMS.API.csproj HRMS.API/

RUN dotnet restore HRMS.API/HRMS.API.csproj

COPY src/ .

RUN dotnet publish HRMS.API/HRMS.API.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

RUN adduser --disabled-password --gecos "" appuser
USER appuser

COPY --from=build /app/publish .

EXPOSE 8080

ENTRYPOINT ["dotnet", "HRMS.API.dll"]
