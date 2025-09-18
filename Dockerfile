FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["SW.Surl.Web/SW.Surl.Web.csproj", "SW.Surl.Web/"]
COPY ["SW.Surl.Sdk/SW.Surl.Sdk.csproj", "SW.Surl.Sdk/"]
RUN dotnet restore "SW.Surl.Web/SW.Surl.Web.csproj"
COPY . .
WORKDIR "/src/SW.Surl.Web"
RUN dotnet build "SW.Surl.Web.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "SW.Surl.Web.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SW.Surl.Web.dll"]