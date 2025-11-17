# credfeto-local-aur-cache

Server for caching AUR locally.

Current State: Experimental

## Why?

I have multiple computers and virtual machines that are running Arch Linux, I've been using services
like [pacoloco](https://github.com/anatol/pacoloco) to avoid downloading packages multiple times over what was a slowish
internet connection, but couldn't find anything similar to work with the AUR.

When the AUR started to get DDOS'd back in August 2025 updating everything including AUR packages became quite
unpredictable and with multiple computers because more time-consuming than I was happy with.

### Why Dotnet?

Dotnet is what I've been using for work and therefore what I'm most familiar with. So while I could have written this in
many other languages, getting something working quickly was the priority.

## How it works

This runs a local http (and optional https) server which responds to the two parts of the AUR that yay uses:

* RPC calls - to query the AUR for packages and get status of packages
* GIT server - to clone the AUR package repos and keep them up-to-date

Transparently, whenever a package's metadata has been downloaded then the git repo should have been downloaded to the
cache too.

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
        docker-registry.markridgwell.com/credfeto/aur-proxy:stable \
        -d
```

`If mapping a trusted local TLS certificate, then this should be mapped as:
`

```bash
docker run \
        -v /cache/aur/metadata:/data/metadata:rw\
        -v /cache/aur/repos:/data/repos:rw \
        -v ./certificates/aur.local.pfx:/usr/src/app/server.pfx:r \
        -p "8081:8081/tcp" \
        -p "8081:8081/udp" \
        docker-registry.markridgwell.com/credfeto/aur-proxy:stable \
        -d
```

Note that the server supports http3 for TLS hence UDP being mapped.

Its recommended if using TLS to put the server behind NGINX or similar and use

## Client configuration

### YAY

In `$HOME/.config/yay/config.json` aururl and aurrpcurl should be set to point at the server

```json
{
  "aururl": "http://localhost:8080",
  "aurrpcurl": "https://localhost:8080/rpc?"
}
```

When accessing remotely from another client pc then adjust localhost to your server ip to access the cache.

```json
{
  "aururl": "http://192.168.101.27:8080",
  "aurrpcurl": "https://192.168.101.27:8080/rpc?"
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
[config]
AurUrl = http://localhost:8080
AurRpcUrl = https://localhost:8080/rpc?
```

When accessing remotely from another client pc then adjust localhost to your server ip to access the cache.
```ini
[config]
AurUrl = http://192.168.101.27:8080
AurRpcUrl = https://192.168.101.27:8080/rpc?
```

If using TLS:

```ini
[config]
AurUrl = http2://my.server.domain:8081
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

