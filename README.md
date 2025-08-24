# credfeto-local-aur

Experiment to cache Arch AUR locally (or use the github AUR mirror transparently)


## Notes

* https://archive.esc.sh/blog/setting-up-a-git-http-server-with-nginx/
* https://linuxhint.com/setup_git_http_server_docker/
* https://github.com/rockstorm101/gitweb-docker?tab=readme-ov-file  (gitweb)

```bash
docker run -v /path/to/repos:/srv/git:ro -p 80:80 rockstorm/gitweb
```

Config to point to sites:
* aururl => where the repos are
* aurrpcurl => where the RPC to get packages live
```json
{
        "aururl": "https://aur.markridgwell.com",
        "aurrpcurl": "https://aur.markridgwell.com/rpc?"
}


```

## Build Status

| Branch  | Status                                                                                                                                                                                                                                |
|---------|---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| main    | [![Build: Pre-Release](https://github.com/credfeto/credfeto-local-aur/actions/workflows/build-and-publish-pre-release.yml/badge.svg)](https://github.com/credfeto/credfeto-local-aur/actions/workflows/build-and-publish-pre-release.yml) |
| release | [![Build: Release](https://github.com/credfeto/credfeto-local-aur/actions/workflows/build-and-publish-release.yml/badge.svg)](https://github.com/credfeto/credfeto-local-aur/actions/workflows/build-and-publish-release.yml)             |

## Changelog

View [changelog](CHANGELOG.md)

## Contributors

<!-- ALL-CONTRIBUTORS-LIST:START - Do not remove or modify this section -->
<!-- prettier-ignore-start -->
<!-- markdownlint-disable -->

<!-- markdownlint-restore -->
<!-- prettier-ignore-end -->

<!-- ALL-CONTRIBUTORS-LIST:END -->
