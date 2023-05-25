﻿FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:7.0 AS build
ARG TARGETPLATFORM
WORKDIR /src

COPY icon.png LICENSE README.md ./
COPY *.sln .
COPY *.props .
COPY LXGaming.Health/*.csproj ./LXGaming.Health/
COPY LXGaming.Health.Client/*.csproj ./LXGaming.Health.Client/
RUN case "$TARGETPLATFORM" in \
        linux/amd64) RUNTIME=linux-x64 ;; \
        linux/arm64) RUNTIME=linux-arm64 ;; \
        *) echo "Unsupported Platform: $TARGETPLATFORM"; exit 1 ;; \
    esac && \
    dotnet restore LXGaming.Health.Client --runtime $RUNTIME

COPY LXGaming.Health/. ./LXGaming.Health/
COPY LXGaming.Health.Client/. ./LXGaming.Health.Client/
WORKDIR /src/LXGaming.Health.Client
RUN case "$TARGETPLATFORM" in \
        linux/amd64) RUNTIME=linux-x64 ;; \
        linux/arm64) RUNTIME=linux-arm64 ;; \
        *) echo "Unsupported Platform: $TARGETPLATFORM"; exit 1 ;; \
    esac && \
    dotnet publish --configuration Release --no-restore --output /app --runtime $RUNTIME --self-contained true /p:PublishSingleFile=true /p:PublishTrimmed=true /p:SuppressTrimAnalysisWarnings=true /p:TrimMode=partial

FROM mcr.microsoft.com/dotnet/runtime-deps:7.0
WORKDIR /app
COPY --from=build /app ./
ENTRYPOINT ["./LXGaming.Health.Client"]