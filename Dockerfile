FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/nightly/sdk:9.0-alpine-aot AS build
ARG TARGETARCH
WORKDIR /source

# Copy source code and publish app
COPY --link . .
RUN dotnet publish -o /app -r linux-musl-$TARGETARCH

# Runtime stage
FROM mcr.microsoft.com/dotnet/nightly/runtime-deps:9.0-alpine-aot
WORKDIR /app
COPY --link --from=build /app .
USER $APP_UID
ENTRYPOINT ["./queuesim"]