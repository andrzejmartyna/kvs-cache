FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /app

COPY *.csproj ./
RUN dotnet restore

COPY . ./
RUN dotnet publish -c Release -o out

FROM mcr.microsoft.com/dotnet/runtime:7.0

RUN apt-get update && apt-get install -y curl

RUN curl -sL https://aka.ms/InstallAzureCLIDeb | bash -

WORKDIR /app
COPY --from=build /app/out .

ENTRYPOINT ["dotnet", "kcs-cache.dll"]
