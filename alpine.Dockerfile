# syntax=docker/dockerfile:1
FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build
ARG TARGETARCH
WORKDIR /src

COPY icon.png LICENSE README.md ./
COPY *.sln ./
COPY *.props ./
COPY LXGaming.Health/*.csproj LXGaming.Health/
COPY LXGaming.Health.Client/*.csproj LXGaming.Health.Client/
RUN dotnet restore LXGaming.Health.Client --arch $TARGETARCH

COPY LXGaming.Health/ LXGaming.Health/
COPY LXGaming.Health.Client/ LXGaming.Health.Client/
RUN dotnet publish LXGaming.Health.Client --arch $TARGETARCH --configuration Release --no-restore --output /app

FROM mcr.microsoft.com/dotnet/runtime-deps:8.0-alpine
WORKDIR /app
COPY --from=build /app ./
USER $APP_UID
ENTRYPOINT ["./LXGaming.Health.Client"]