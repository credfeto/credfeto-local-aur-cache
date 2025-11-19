# credfeto-local-aur-cache

Server for caching AUR locally.

Current State:

* Server: Experimental
* Client: Yay: Working
* Client: Paru: Working

## Why?

I have multiple computers and virtual machines that are running Arch Linux, I've been using services
like [pacoloco](https://github.com/anatol/pacoloco) to avoid downloading packages multiple times over what was a slowish
internet connection, but couldn't find anything similar to work with the AUR.

When the AUR started to get DDOS'd back in August 2025 updating everything including AUR packages became quite
unpredictable and with multiple computers because more time-consuming than I was happy with.

Currently, it is set up as just a local cache. If the AUR isn't available for any reason, the cache will return the
current versions of packages in the cache and not found for anything it does not know about. This will allow a
``yay -Syu`` to work without erroring about cthe AUR not being available.

### Why Dotnet?

Dotnet is what I've been using for work and therefore what I'm most familiar with. So while I could have written this in
many other languages, getting something working quickly was the priority.

## How it works

This runs a local http (and optional https) server which responds to the two parts of the AUR that yay uses:

* RPC calls - to query the AUR for packages and get status of packages
* GIT server - to clone the AUR package repos and keep them up-to-date

Transparently, whenever a package's metadata has been downloaded then the git repo should have been downloaded to the
cache too.

Note this server just serves the metadata and the PKGBUILD repos not any repos etc used by the individual packages.

Note this does not currently support switching to the
GitHub [mirror](https://github.com/credfeto/credfeto-local-aur-cache/issues/7) for the PKGBUILD downloads.

## Usage

Example usage with other services [Docker](https://github.com/credfeto/credfeto-linux-package-cache)

## Server configuration

<!-- NOTE: TODO publish stable images to dockerhub/github -->

Recommended: use the `stable` docker image

```bash
docker run \
        -v /cache/aur/metadata:/data/metadata:rw\
        -v /cache/aur/repos:/data/repos:rw \
        -p "8080:8080/tcp" \
        credfeto/aur-proxy:stable \
        -d
```

If mapping a trusted local TLS certificate, then this should be mapped as:

```bash
docker run \
        -v /cache/aur/metadata:/data/metadata:rw\
        -v /cache/aur/repos:/data/repos:rw \
        -v ./certificates/aur.local.pfx:/usr/src/app/server.pfx:r \
        -p "8081:8081/tcp" \
        -p "8081:8081/udp" \
        credfeto/aur-proxy:stable \
        -d
```

Note that the server supports http3 for TLS hence UDP being mapped.

It is recommended if using TLS to put the server behind NGINX or similar and use

### Docker Compose example

```yml
# docker-compose.yml
services:
  cache-aur:
    image: credfeto/aur-proxy:stable
    container_name: cache-aur
    hostname: cache-aur
    restart: always
    stop_grace_period: 5s
    stop_signal: SIGINT
    volumes:
      - cache-aur-metadata:/data/metadata:rw
      - cache-aur-repos:/data/repos:rw
      - ./aur.local.pfx:/usr/src/app/server.pfx:r
    ports:
      - "8080:8080/tcp"
      - "8081:8081/tcp"
      - "8081:8081/udp"

volumes:
  cache-aur-metadata:
    name: cache-aur-metadata
    external: true
  cache-aur-repos:
    name: cache-aur-repos
    external: true
```

### Server Configuration

Should be mapped into `/usr/src/app/appsettings-local.json` or specified via the named environment variables

| Setting                      | Environment Variable         | Description                                                           | Default                            |
|------------------------------|------------------------------|-----------------------------------------------------------------------|------------------------------------|
| ``Proxy::Git::Executable``   | ``Proxy__Git__Executable``   | Where the git exe lives                                               | ``/usr/bin/git``                   |
| ``Proxy::Upstream::Rpc``     | ``Proxy__Upstream__Rpc``     | Where the git exe lives                                               | ``https://aur.archlinux.org/rpc?`` |
| ``Proxy::Upstream::Repos``   | ``Proxy__Upstream__Repos``   | Where to clone the repos from                                         | ``https://aur.archlinux.org``      |
| ``Proxy::Storage::Metadata`` | ``Proxy__Storage__Metadata`` | Directory to store the metadata that's cached from the upstream RPC   | ``/data/metadata``                 |
| ``Proxy::Storage::Repos``    | ``Proxy__Storage__Repos``    | Directory to store the metadata that's cached from the upstream Repos | ``/data/repos``                    |

appsettings-local.json:

```json
{
  "Proxy": {
    "Git": {
      "Exeutable": "/usr/bin/git"
    },
    "Upstream": {
      "Rpc": "https://aur.archlinux.org/rpc?",
      "Repos": "https://aur.archlinux.org"
    },
    "Storage": {
      "Metadata": "/data/metadata",
      "Repos": "/data/repos"
    }
  }
}
```

### Server Extensions

``http://localhost:8080/cache``  - gets a list of the locally installed packaged and the date they were last accessed

## Client configuration

### YAY

In `$HOME/.config/yay/config.json` aururl and aurrpcurl should be set to point at the server

```json
{
  "aururl": "http://localhost:8080",
  "aurrpcurl": "http://localhost:8080/rpc?"
}
```

When accessing remotely from another client pc then adjust localhost to your server ip to access the cache.

```json
{
  "aururl": "http://192.168.101.27:8080",
  "aurrpcurl": "http://192.168.101.27:8080/rpc?"
}
```

If using TLS:

```json
{
  "aururl": "https://my.server.domain:8081",
  "aurrpcurl": "https://my.server.domain:8081/rpc?"
}
```

### Paru

In `$HOME/.config/paru/paru.conf` AurUrl and AurRpcUrl should be set to point at the server

```ini
[options]
AurUrl = http://localhost:8080
AurRpcUrl = http://localhost:8080/rpc?
```

When accessing remotely from another client pc then adjust localhost to your server ip to access the cache.

```ini
[options]
AurUrl = http://192.168.101.27:8080
AurRpcUrl = http://192.168.101.27:8080/rpc?
```

If using TLS:

```ini
[options]
AurUrl = https://my.server.domain:8081
AurRpcUrl = https://my.server.domain:8081/rpc?
```

## Build Status

| Branch  | Status                                                                                                                                                                                                                                                |
|---------|-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| main    | [![Build: Pre-Release](https://github.com/credfeto/credfeto-local-aur-cache/actions/workflows/build-and-publish-pre-release.yml/badge.svg)](https://github.com/credfeto/credfeto-local-aur-cache/actions/workflows/build-and-publish-pre-release.yml) |
| release | [![Build: Release](https://github.com/credfeto/credfeto-local-aur-cache/actions/workflows/build-and-publish-release.yml/badge.svg)](https://github.com/credfeto/credfeto-local-aur-cache/actions/workflows/build-and-publish-release.yml)             |

## Changelog

View [changelog](CHANGELOG.md)

## Contributors

<!-- ALL-CONTRIBUTORS-LIST:START - Do not remove or modify this section -->
<!-- prettier-ignore-start -->
<!-- markdownlint-disable -->

<!-- markdownlint-restore -->
<!-- prettier-ignore-end -->

<!-- ALL-CONTRIBUTORS-LIST:END -->

