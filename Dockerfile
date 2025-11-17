FROM mcr.microsoft.com/dotnet/runtime-deps:10.0-noble

WORKDIR /usr/src/app

# Bundle App and basic config
COPY Credfeto.Aur.Mirror.Server .
COPY appsettings.json .

RUN mkdir /data && \
    mkdir /data/metadata && \
    mkdir /data/repos && \
    apt-get update && \
    apt-get install -y git

EXPOSE 8080
ENTRYPOINT [ "/usr/src/app/Credfeto.Aur.Mirror.Server" ]
 
# Perform a healthcheck.  note that ECS ignores this, so this is for local development
HEALTHCHECK --interval=5s --timeout=2s --retries=3 --start-period=5s CMD [ "/usr/src/app/Credfeto.Aur.Mirror.Server", "--health-check", "http://127.0.0.1:8080/ping?source=docker" ]
