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
```

https://aur.archlinux.org/rpc?arg[]=afetch-git&by=provides&type=search&v=5

```json        
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
```

// so could cache each result by ID 
// when no result git clone --bare https://aur.archlinux.org/{Name}.git 
and whenever the LastModified changes then git fetch the repo to get the latest



could also trigger clone and update on fetch of http://local-aur-url/{Name}.git/info/refs

http://localhost:8080/afetch-git.git/info/refs

LibGit2Sharp

```csharp
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


#### More notes

```logs
arch-n95:markr  ~/tmp  00:11  GIT_CURL_VERBOSE=1  git clone http://github.com/credfeto/credfeto.git --bare 
Cloning into bare repository 'credfeto.git'...
00:25:57.583251 http.c:889              == Info: Couldn't find host github.com in the .netrc file; using defaults
00:25:57.585188 http.c:889              == Info: Host github.com:80 was resolved.
00:25:57.585201 http.c:889              == Info: IPv6: (none)
00:25:57.585204 http.c:889              == Info: IPv4: 20.26.156.215
00:25:57.585223 http.c:889              == Info:   Trying 20.26.156.215:80...
00:25:57.609010 http.c:889              == Info: Connected to github.com (20.26.156.215) port 80
00:25:57.609055 http.c:889              == Info: using HTTP/1.x
00:25:57.609213 http.c:836              => Send header, 0000000246 bytes (0x000000f6)
00:25:57.609235 http.c:848              => Send header: GET /credfeto/credfeto.git/info/refs?service=git-upload-pack HTTP/1.1
00:25:57.609245 http.c:848              => Send header: Host: github.com
00:25:57.609255 http.c:848              => Send header: User-Agent: git/2.50.1
00:25:57.609266 http.c:848              => Send header: Accept: */*
00:25:57.609277 http.c:848              => Send header: Accept-Encoding: deflate, gzip, br, zstd
00:25:57.609284 http.c:848              => Send header: Accept-Language: en-GB, *;q=0.9
00:25:57.609293 http.c:848              => Send header: Pragma: no-cache
00:25:57.609303 http.c:848              => Send header: Git-Protocol: version=2
00:25:57.609309 http.c:848              => Send header:
00:25:57.609355 http.c:889              == Info: Request completely sent off
00:25:57.639629 http.c:836              <= Recv header, 0000000032 bytes (0x00000020)
00:25:57.639661 http.c:848              <= Recv header: HTTP/1.1 301 Moved Permanently
00:25:57.639682 http.c:836              <= Recv header, 0000000019 bytes (0x00000013)
00:25:57.639691 http.c:848              <= Recv header: Content-Length: 0
00:25:57.639709 http.c:836              <= Recv header, 0000000086 bytes (0x00000056)
00:25:57.639722 http.c:848              <= Recv header: Location: https://github.com/credfeto/credfeto.git/info/refs?service=git-upload-pack
00:25:57.639747 http.c:889              == Info: Ignoring the response-body
00:25:57.639761 http.c:889              == Info: setting size while ignoring
00:25:57.639771 http.c:836              <= Recv header, 0000000002 bytes (0x00000002)
00:25:57.639781 http.c:848              <= Recv header:
00:25:57.639809 http.c:889              == Info: Connection #0 to host github.com left intact
00:25:57.639850 http.c:889              == Info: Clear auth, redirects to port from 80 to 443
00:25:57.639863 http.c:889              == Info: Issue another request to this URL: 'https://github.com/credfeto/credfeto.git/info/refs?service=git-upload-pack'
00:25:57.639948 http.c:889              == Info: Couldn't find host github.com in the .netrc file; using defaults
00:25:57.639977 http.c:889              == Info: NTLM-proxy picked AND auth done set, clear picked
00:25:57.643110 http.c:889              == Info: Host github.com:443 was resolved.
00:25:57.643152 http.c:889              == Info: IPv6: (none)
00:25:57.643167 http.c:889              == Info: IPv4: 20.26.156.215
00:25:57.643242 http.c:889              == Info:   Trying 20.26.156.215:443...
00:25:57.675708 http.c:889              == Info: ALPN: curl offers h2,http/1.1
00:25:57.676478 http.c:889              == Info: TLSv1.3 (OUT), TLS handshake, Client hello (1):
00:25:57.689619 http.c:889              == Info:  CAfile: /etc/ssl/certs/ca-certificates.crt
00:25:57.689635 http.c:889              == Info:  CApath: none
00:25:57.709885 http.c:889              == Info: TLSv1.3 (IN), TLS handshake, Server hello (2):
00:25:57.710795 http.c:889              == Info: TLSv1.3 (IN), TLS change cipher, Change cipher spec (1):
00:25:57.710857 http.c:889              == Info: TLSv1.3 (IN), TLS handshake, Encrypted Extensions (8):
00:25:57.710928 http.c:889              == Info: TLSv1.3 (IN), TLS handshake, Certificate (11):
00:25:57.716633 http.c:889              == Info: TLSv1.3 (IN), TLS handshake, CERT verify (15):
00:25:57.716931 http.c:889              == Info: TLSv1.3 (IN), TLS handshake, Finished (20):
00:25:57.717002 http.c:889              == Info: TLSv1.3 (OUT), TLS change cipher, Change cipher spec (1):
00:25:57.717047 http.c:889              == Info: TLSv1.3 (OUT), TLS handshake, Finished (20):
00:25:57.717110 http.c:889              == Info: SSL connection using TLSv1.3 / TLS_AES_128_GCM_SHA256 / x25519 / id-ecPublicKey
00:25:57.717120 http.c:889              == Info: ALPN: server accepted h2
00:25:57.717129 http.c:889              == Info: Server certificate:
00:25:57.717141 http.c:889              == Info:  subject: CN=github.com
00:25:57.717152 http.c:889              == Info:  start date: Feb  5 00:00:00 2025 GMT
00:25:57.717160 http.c:889              == Info:  expire date: Feb  5 23:59:59 2026 GMT
00:25:57.717176 http.c:889              == Info:  subjectAltName: host "github.com" matched cert's "github.com"
00:25:57.717195 http.c:889              == Info:  issuer: C=GB; ST=Greater Manchester; L=Salford; O=Sectigo Limited; CN=Sectigo ECC Domain Validation Secure Server CA
00:25:57.717202 http.c:889              == Info:  SSL certificate verify ok.
00:25:57.717214 http.c:889              == Info:   Certificate level 0: Public key type EC/prime256v1 (256/128 Bits/secBits), signed using ecdsa-with-SHA256
00:25:57.717224 http.c:889              == Info:   Certificate level 1: Public key type EC/prime256v1 (256/128 Bits/secBits), signed using ecdsa-with-SHA384
00:25:57.717233 http.c:889              == Info:   Certificate level 2: Public key type EC/secp384r1 (384/192 Bits/secBits), signed using ecdsa-with-SHA384
00:25:57.717288 http.c:889              == Info: Connected to github.com (20.26.156.215) port 443
00:25:57.717296 http.c:889              == Info: using HTTP/2
00:25:57.717337 http.c:889              == Info: [HTTP/2] [1] OPENED stream for https://github.com/credfeto/credfeto.git/info/refs?service=git-upload-pack
00:25:57.717345 http.c:889              == Info: [HTTP/2] [1] [:method: GET]
00:25:57.717350 http.c:889              == Info: [HTTP/2] [1] [:scheme: https]
00:25:57.717356 http.c:889              == Info: [HTTP/2] [1] [:authority: github.com]
00:25:57.717362 http.c:889              == Info: [HTTP/2] [1] [:path: /credfeto/credfeto.git/info/refs?service=git-upload-pack]
00:25:57.717367 http.c:889              == Info: [HTTP/2] [1] [user-agent: git/2.50.1]
00:25:57.717372 http.c:889              == Info: [HTTP/2] [1] [accept: */*]
00:25:57.717385 http.c:889              == Info: [HTTP/2] [1] [accept-encoding: deflate, gzip, br, zstd]
00:25:57.717400 http.c:889              == Info: [HTTP/2] [1] [accept-language: en-GB, *;q=0.9]
00:25:57.717408 http.c:889              == Info: [HTTP/2] [1] [pragma: no-cache]
00:25:57.717415 http.c:889              == Info: [HTTP/2] [1] [git-protocol: version=2]
00:25:57.717485 http.c:836              => Send header, 0000000244 bytes (0x000000f4)
00:25:57.717498 http.c:848              => Send header: GET /credfeto/credfeto.git/info/refs?service=git-upload-pack HTTP/2
00:25:57.717506 http.c:848              => Send header: Host: github.com
00:25:57.717512 http.c:848              => Send header: User-Agent: git/2.50.1
00:25:57.717519 http.c:848              => Send header: Accept: */*
00:25:57.717523 http.c:848              => Send header: Accept-Encoding: deflate, gzip, br, zstd
00:25:57.717545 http.c:848              => Send header: Accept-Language: en-GB, *;q=0.9
00:25:57.717553 http.c:848              => Send header: Pragma: no-cache
00:25:57.717558 http.c:848              => Send header: Git-Protocol: version=2
00:25:57.717564 http.c:848              => Send header:
00:25:57.717615 http.c:889              == Info: Request completely sent off
00:25:57.736788 http.c:889              == Info: TLSv1.3 (IN), TLS handshake, Newsession Ticket (4):
00:25:57.736974 http.c:889              == Info: TLSv1.3 (IN), TLS handshake, Newsession Ticket (4):
00:25:57.847777 http.c:836              <= Recv header, 0000000013 bytes (0x0000000d)
00:25:57.847821 http.c:848              <= Recv header: HTTP/2 200
00:25:57.847850 http.c:836              <= Recv header, 0000000026 bytes (0x0000001a)
00:25:57.847861 http.c:848              <= Recv header: server: GitHub-Babel/3.0
00:25:57.847883 http.c:836              <= Recv header, 0000000059 bytes (0x0000003b)
00:25:57.847896 http.c:848              <= Recv header: content-type: application/x-git-upload-pack-advertisement
00:25:57.847914 http.c:836              <= Recv header, 0000000054 bytes (0x00000036)
00:25:57.847925 http.c:848              <= Recv header: content-security-policy: default-src 'none'; sandbox
00:25:57.847942 http.c:836              <= Recv header, 0000000040 bytes (0x00000028)
00:25:57.847953 http.c:848              <= Recv header: expires: Fri, 01 Jan 1980 00:00:00 GMT
00:25:57.847967 http.c:836              <= Recv header, 0000000018 bytes (0x00000012)
00:25:57.847979 http.c:848              <= Recv header: pragma: no-cache
00:25:57.847992 http.c:836              <= Recv header, 0000000053 bytes (0x00000035)
00:25:57.848005 http.c:848              <= Recv header: cache-control: no-cache, max-age=0, must-revalidate
00:25:57.848018 http.c:836              <= Recv header, 0000000023 bytes (0x00000017)
00:25:57.848026 http.c:848              <= Recv header: vary: Accept-Encoding
00:25:57.848039 http.c:836              <= Recv header, 0000000037 bytes (0x00000025)
00:25:57.848047 http.c:848              <= Recv header: date: Sun, 24 Aug 2025 23:25:57 GMT
00:25:57.848060 http.c:836              <= Recv header, 0000000023 bytes (0x00000017)
00:25:57.848068 http.c:848              <= Recv header: x-frame-options: DENY
00:25:57.848080 http.c:836              <= Recv header, 0000000073 bytes (0x00000049)
00:25:57.848088 http.c:848              <= Recv header: strict-transport-security: max-age=31536000; includeSubDomains; preload
00:25:57.848104 http.c:836              <= Recv header, 0000000059 bytes (0x0000003b)
00:25:57.848113 http.c:848              <= Recv header: x-github-request-id: 6A4B:328B39:12A161B:170E20F:68AB9F85
00:25:57.848129 http.c:836              <= Recv header, 0000000002 bytes (0x00000002)
00:25:57.848141 http.c:848              <= Recv header:
00:25:57.848214 http.c:889              == Info: Connection #1 to host github.com left intact
warning: redirecting to https://github.com/credfeto/credfeto.git/
00:25:57.848967 http.c:889              == Info: Couldn't find host github.com in the .netrc file; using defaults
00:25:57.849023 http.c:889              == Info: Re-using existing https: connection with host github.com
00:25:57.849276 http.c:889              == Info: [HTTP/2] [3] OPENED stream for https://github.com/credfeto/credfeto.git/git-upload-pack
00:25:57.849297 http.c:889              == Info: [HTTP/2] [3] [:method: POST]
00:25:57.849312 http.c:889              == Info: [HTTP/2] [3] [:scheme: https]
00:25:57.849323 http.c:889              == Info: [HTTP/2] [3] [:authority: github.com]
00:25:57.849338 http.c:889              == Info: [HTTP/2] [3] [:path: /credfeto/credfeto.git/git-upload-pack]
00:25:57.849352 http.c:889              == Info: [HTTP/2] [3] [user-agent: git/2.50.1]
00:25:57.849366 http.c:889              == Info: [HTTP/2] [3] [accept-encoding: deflate, gzip, br, zstd]
00:25:57.849380 http.c:889              == Info: [HTTP/2] [3] [content-type: application/x-git-upload-pack-request]
00:25:57.849392 http.c:889              == Info: [HTTP/2] [3] [accept: application/x-git-upload-pack-result]
00:25:57.849406 http.c:889              == Info: [HTTP/2] [3] [accept-language: en-GB, *;q=0.9]
00:25:57.849419 http.c:889              == Info: [HTTP/2] [3] [git-protocol: version=2]
00:25:57.849432 http.c:889              == Info: [HTTP/2] [3] [content-length: 181]
00:25:57.849598 http.c:836              => Send header, 0000000316 bytes (0x0000013c)
00:25:57.849620 http.c:848              => Send header: POST /credfeto/credfeto.git/git-upload-pack HTTP/2
00:25:57.849634 http.c:848              => Send header: Host: github.com
00:25:57.849647 http.c:848              => Send header: User-Agent: git/2.50.1
00:25:57.849656 http.c:848              => Send header: Accept-Encoding: deflate, gzip, br, zstd
00:25:57.849667 http.c:848              => Send header: Content-Type: application/x-git-upload-pack-request
00:25:57.849679 http.c:848              => Send header: Accept: application/x-git-upload-pack-result
00:25:57.849689 http.c:848              => Send header: Accept-Language: en-GB, *;q=0.9
00:25:57.849700 http.c:848              => Send header: Git-Protocol: version=2
00:25:57.849710 http.c:848              => Send header: Content-Length: 181
00:25:57.849722 http.c:848              => Send header:
00:25:57.849739 http.c:889              == Info: upload completely sent off: 181 bytes
00:25:57.973849 http.c:836              <= Recv header, 0000000013 bytes (0x0000000d)
00:25:57.973891 http.c:848              <= Recv header: HTTP/2 200
00:25:57.973919 http.c:836              <= Recv header, 0000000026 bytes (0x0000001a)
00:25:57.973931 http.c:848              <= Recv header: server: GitHub-Babel/3.0
00:25:57.973949 http.c:836              <= Recv header, 0000000052 bytes (0x00000034)
00:25:57.973961 http.c:848              <= Recv header: content-type: application/x-git-upload-pack-result
00:25:57.973979 http.c:836              <= Recv header, 0000000054 bytes (0x00000036)
00:25:57.973990 http.c:848              <= Recv header: content-security-policy: default-src 'none'; sandbox
00:25:57.974005 http.c:836              <= Recv header, 0000000040 bytes (0x00000028)
00:25:57.974017 http.c:848              <= Recv header: expires: Fri, 01 Jan 1980 00:00:00 GMT
00:25:57.974034 http.c:836              <= Recv header, 0000000018 bytes (0x00000012)
00:25:57.974045 http.c:848              <= Recv header: pragma: no-cache
00:25:57.974060 http.c:836              <= Recv header, 0000000053 bytes (0x00000035)
00:25:57.974071 http.c:848              <= Recv header: cache-control: no-cache, max-age=0, must-revalidate
00:25:57.974087 http.c:836              <= Recv header, 0000000023 bytes (0x00000017)
00:25:57.974098 http.c:848              <= Recv header: vary: Accept-Encoding
00:25:57.974112 http.c:836              <= Recv header, 0000000037 bytes (0x00000025)
00:25:57.974123 http.c:848              <= Recv header: date: Sun, 24 Aug 2025 23:25:57 GMT
00:25:57.974140 http.c:836              <= Recv header, 0000000023 bytes (0x00000017)
00:25:57.974151 http.c:848              <= Recv header: x-frame-options: DENY
00:25:57.974165 http.c:836              <= Recv header, 0000000073 bytes (0x00000049)
00:25:57.974177 http.c:848              <= Recv header: strict-transport-security: max-age=31536000; includeSubDomains; preload
00:25:57.974194 http.c:836              <= Recv header, 0000000059 bytes (0x0000003b)
00:25:57.974205 http.c:848              <= Recv header: x-github-request-id: 6A4B:328B39:12A162B:170E229:68AB9F85
00:25:57.974223 http.c:836              <= Recv header, 0000000002 bytes (0x00000002)
00:25:57.974234 http.c:848              <= Recv header:
00:25:57.974305 http.c:889              == Info: Connection #1 to host github.com left intact
00:25:57.977223 http.c:889              == Info: Couldn't find host github.com in the .netrc file; using defaults
00:25:57.977267 http.c:889              == Info: Re-using existing https: connection with host github.com
00:25:57.977348 http.c:889              == Info: [HTTP/2] [5] OPENED stream for https://github.com/credfeto/credfeto.git/git-upload-pack
00:25:57.977365 http.c:889              == Info: [HTTP/2] [5] [:method: POST]
00:25:57.977377 http.c:889              == Info: [HTTP/2] [5] [:scheme: https]
00:25:57.977388 http.c:889              == Info: [HTTP/2] [5] [:authority: github.com]
00:25:57.977399 http.c:889              == Info: [HTTP/2] [5] [:path: /credfeto/credfeto.git/git-upload-pack]
00:25:57.977409 http.c:889              == Info: [HTTP/2] [5] [user-agent: git/2.50.1]
00:25:57.977420 http.c:889              == Info: [HTTP/2] [5] [accept-encoding: deflate, gzip, br, zstd]
00:25:57.977432 http.c:889              == Info: [HTTP/2] [5] [content-type: application/x-git-upload-pack-request]
00:25:57.977442 http.c:889              == Info: [HTTP/2] [5] [accept: application/x-git-upload-pack-result]
00:25:57.977453 http.c:889              == Info: [HTTP/2] [5] [accept-language: en-GB, *;q=0.9]
00:25:57.977460 http.c:889              == Info: [HTTP/2] [5] [git-protocol: version=2]
00:25:57.977469 http.c:889              == Info: [HTTP/2] [5] [content-length: 258]
00:25:57.977609 http.c:836              => Send header, 0000000316 bytes (0x0000013c)
00:25:57.977628 http.c:848              => Send header: POST /credfeto/credfeto.git/git-upload-pack HTTP/2
00:25:57.977638 http.c:848              => Send header: Host: github.com
00:25:57.977648 http.c:848              => Send header: User-Agent: git/2.50.1
00:25:57.977658 http.c:848              => Send header: Accept-Encoding: deflate, gzip, br, zstd
00:25:57.977668 http.c:848              => Send header: Content-Type: application/x-git-upload-pack-request
00:25:57.977680 http.c:848              => Send header: Accept: application/x-git-upload-pack-result
00:25:57.977690 http.c:848              => Send header: Accept-Language: en-GB, *;q=0.9
00:25:57.977699 http.c:848              => Send header: Git-Protocol: version=2
00:25:57.977708 http.c:848              => Send header: Content-Length: 258
00:25:57.977717 http.c:848              => Send header:
00:25:57.977733 http.c:889              == Info: upload completely sent off: 258 bytes
00:25:58.112887 http.c:836              <= Recv header, 0000000013 bytes (0x0000000d)
00:25:58.112936 http.c:848              <= Recv header: HTTP/2 200
00:25:58.112970 http.c:836              <= Recv header, 0000000026 bytes (0x0000001a)
00:25:58.112985 http.c:848              <= Recv header: server: GitHub-Babel/3.0
00:25:58.113007 http.c:836              <= Recv header, 0000000052 bytes (0x00000034)
00:25:58.113023 http.c:848              <= Recv header: content-type: application/x-git-upload-pack-result
00:25:58.113048 http.c:836              <= Recv header, 0000000054 bytes (0x00000036)
00:25:58.113061 http.c:848              <= Recv header: content-security-policy: default-src 'none'; sandbox
00:25:58.113082 http.c:836              <= Recv header, 0000000040 bytes (0x00000028)
00:25:58.113095 http.c:848              <= Recv header: expires: Fri, 01 Jan 1980 00:00:00 GMT
00:25:58.113113 http.c:836              <= Recv header, 0000000018 bytes (0x00000012)
00:25:58.113125 http.c:848              <= Recv header: pragma: no-cache
00:25:58.113143 http.c:836              <= Recv header, 0000000053 bytes (0x00000035)
00:25:58.113155 http.c:848              <= Recv header: cache-control: no-cache, max-age=0, must-revalidate
00:25:58.113171 http.c:836              <= Recv header, 0000000023 bytes (0x00000017)
00:25:58.113183 http.c:848              <= Recv header: vary: Accept-Encoding
00:25:58.113199 http.c:836              <= Recv header, 0000000037 bytes (0x00000025)
00:25:58.113211 http.c:848              <= Recv header: date: Sun, 24 Aug 2025 23:25:58 GMT
00:25:58.113229 http.c:836              <= Recv header, 0000000023 bytes (0x00000017)
00:25:58.113241 http.c:848              <= Recv header: x-frame-options: DENY
00:25:58.113261 http.c:836              <= Recv header, 0000000073 bytes (0x00000049)
00:25:58.113283 http.c:848              <= Recv header: strict-transport-security: max-age=31536000; includeSubDomains; preload
00:25:58.113302 http.c:836              <= Recv header, 0000000059 bytes (0x0000003b)
00:25:58.113314 http.c:848              <= Recv header: x-github-request-id: 6A4B:328B39:12A1642:170E23B:68AB9F85
00:25:58.113334 http.c:836              <= Recv header, 0000000002 bytes (0x00000002)
00:25:58.113347 http.c:848              <= Recv header:
remote: Enumerating objects: 12183, done.
remote: Counting objects: 100% (120/120), done.
remote: Compressing objects: 100% (72/72), done.
00:25:58.572353 http.c:889              == Info: Connection #1 to host github.com left intact
remote: Total 12183 (delta 74), reused 75 (delta 36), pack-reused 12063 (from 2)
Receiving objects: 100% (12183/12183), 2.85 MiB | 6.45 MiB/s, done.
Resolving deltas: 100% (8940/8940), done.
```