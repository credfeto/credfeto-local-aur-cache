FROM alpine:latest


WORKDIR /usr/src/app

CMD mkdir /data && \
    mkdir /data/metadata && \
    mkdir /data/metadata && \
    chown -R 1654:1654 /data
    chown -R 1654:1654 /usr/src/app

RUN apk add --no-cache \
        bash \
        ca-certificates \
        curl  \
        doas \
        git \
        gnupg \
        icu-libs \
        jq \
        krb5-libs \
        libgcc \
        libintl \
        libssl3 \
        libstdc++ \
        openssh \
        sed \
        zlib

RUN rm -fr /sbin/apk /etc/apk /lib/apk /usr/share/apk /var/lib/apk

SHELL ["/bin/ash", "-o", "pipefail", "-c"]

USER 1654:1654

# Bundle App and basic config
COPY Credfeto.Aur.Mirror.Server .
COPY appsettings.json .


EXPOSE 8080
ENTRYPOINT [ "/usr/src/app/Credfeto.Aur.Mirror.Server" ]
 
# Perform a healthcheck.  note that ECS ignores this, so this is for local development
HEALTHCHECK --interval=5s --timeout=2s --retries=3 --start-period=5s CMD [ "/usr/src/app/Credfeto.Aur.Mirror.Server", "--health-check", "http://127.0.0.1:8080/ping?source=docker" ]
