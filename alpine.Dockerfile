# syntax=docker/dockerfile:1
FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS build
ARG TARGETARCH
WORKDIR /src

COPY --link icon.png LICENSE README.md ./
COPY --link *.sln ./
COPY --link *.props ./
COPY --link LXGaming.Health/*.csproj LXGaming.Health/
COPY --link LXGaming.Health.Client/*.csproj LXGaming.Health.Client/
RUN dotnet restore LXGaming.Health.Client --arch $TARGETARCH

COPY --link LXGaming.Health/ LXGaming.Health/
COPY --link LXGaming.Health.Client/ LXGaming.Health.Client/
RUN dotnet publish LXGaming.Health.Client --arch $TARGETARCH --configuration Release --no-restore --output /app

FROM mcr.microsoft.com/dotnet/runtime-deps:10.0-alpine
WORKDIR /app
COPY --from=build --link /app ./
USER $APP_UID
ENTRYPOINT ["./LXGaming.Health.Client"]