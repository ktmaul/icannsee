FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-alpine AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-alpine AS build
WORKDIR /src
COPY ["Icannsee.Web/Icannsee.Web.csproj", "Icannsee.Web/"]
RUN dotnet restore "Icannsee.Web/Icannsee.Web.csproj"
COPY . .
WORKDIR "/src/Icannsee.Web"
RUN dotnet build "Icannsee.Web.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Icannsee.Web.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Icannsee.Web.dll"]