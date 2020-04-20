FROM microsoft/dotnet:2.2-aspnetcore-runtime AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM microsoft/dotnet:2.2-sdk AS build
WORKDIR /src
COPY ["KenLogistics/KenLogistics.Web.csproj", "KenLogistics/"]
COPY ["KenLogistics.Repository/KenLogistics.Repository.csproj", "KenLogistics.Repository/"]
COPY ["KenLogistics.Data/KenLogistics.Data.csproj", "KenLogistics.Data/"]
COPY ["KenLogistics.Infrastructure/KenLogistics.Infrastructure.csproj", "KenLogistics.Infrastructure/"]
RUN dotnet restore "KenLogistics/KenLogistics.Web.csproj"
COPY . .
WORKDIR "/src/KenLogistics"
RUN dotnet build "KenLogistics.Web.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "KenLogistics.Web.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENV ASPNETCORE_ENVIRONMENT Production
CMD ASPNETCORE_URLS=http://*:$PORT dotnet ExpenseTracker.Web.dll