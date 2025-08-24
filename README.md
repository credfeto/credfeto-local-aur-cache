# credfeto-local-aur-cache

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

https://aur.archlinux.org/rpc?arg[]=afetch-git&by=provides&type=search&v=5

{
"resultcount":1,
"results":[
{
"Description":"Fast and simple system info written in C, that can be configured at compile time by editing the config.h file",
"FirstSubmitted":1603171383,
"ID":849486,
"LastModified":1610998028,
"Maintainer":"McFranko",
"Name":"afetch-git",
"NumVotes":3,
"OutOfDate":1687880376,
"PackageBase":"afetch-git",
"PackageBaseID":158933,
"Popularity":0,
"URL":"https://github.com/13-CF/afetch",
"URLPath":"/cgit/aur.git/snapshot/afetch-git.tar.gz","Version":"1-1"
}
],
"type":"search",
"version":5
}


// so could cache each result by ID 
// when no result git clone --bare https://aur.archlinux.org/{Name}.git 
and whenever the LastModified changes then git fetch the repo to get the latest



could also trigger clone and update on fetch of http://local-aur-url/{Name}.git/info/refs

http://localhost:8080/afetch-git.git/info/refs


LibGit2Sharp
Repository.Clone("https://example.com", workDir, new CloneOptions() { IsBare = true });


```

## Build Status

| Branch  | Status                                                                                                                                                                                                                                |
|---------|---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
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
