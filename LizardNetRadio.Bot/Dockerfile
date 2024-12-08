FROM mcr.microsoft.com/dotnet/runtime:8.0

ARG GIT_VERSION_HASH=undefined

WORKDIR /opt
ADD * /opt

LABEL org.opencontainers.image.source="https://github.com/stwalkerster/radiod"
LABEL org.opencontainers.image.revision="$GIT_VERSION_HASH"
LABEL org.opencontainers.image.vendor="Simon Walker <github@stwalkerster.co.uk>"
LABEL org.opencontainers.image.description="radiod - IRC-based control of LiquidSoap"

ADD https://rolesanywhere.amazonaws.com/releases/1.3.0/X86_64/Linux/aws_signing_helper /usr/bin/aws_signing_helper
RUN chmod a+x /usr/bin/aws_signing_helper

USER 1000
CMD [ "/opt/LizardNetRadio.Bot" ]

ENV HOME=/opt
