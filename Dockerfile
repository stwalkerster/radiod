FROM mcr.microsoft.com/dotnet/runtime:8.0

ARG GIT_VERSION_HASH=undefined

WORKDIR /opt
ADD * /opt

LABEL org.opencontainers.image.source="https://github.com/stwalkerster/radiod"
LABEL org.opencontainers.image.revision="$GIT_VERSION_HASH"
LABEL org.opencontainers.image.vendor="Simon Walker <github@stwalkerster.co.uk>"
LABEL org.opencontainers.image.description="radiod - IRC-based control of LiquidSoap"

USER 1000
