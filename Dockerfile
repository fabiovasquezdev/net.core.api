FROM mcr.microsoft.com/dotnet/sdk:9.0 as build

WORKDIR /app
COPY . .

RUN dotnet restore "src/API/API.csproj"

RUN dotnet publish "src/API/API.csproj" -c Release -r linux-x64 --self-contained -o /app/out

FROM mcr.microsoft.com/dotnet/runtime-deps:9.0 as runtime

WORKDIR /app
COPY --from=build /app/out .

ENV ASPNETCORE_URLS=http://+:80
ENV ASPNETCORE_ENVIRONMENT=Production

EXPOSE 80
EXPOSE 443

HEALTHCHECK --interval=30s --timeout=3s --retries=3 \
    CMD curl -f http://localhost/health || exit 1

# Run the application
ENTRYPOINT ["./API"] 