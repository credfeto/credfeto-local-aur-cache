# Changelog
All notable changes to this project will be documented in this file.

<!--
Please ADD ALL Changes to the UNRELEASED SECTION and not a specific release
-->

## [Unreleased]
### Security
### Added
### Fixed
- GitHub Actions: on_new_pr workflow now checks out the repository before using local composite actions
### Changed
- Dependencies - Updated Credfeto.Enumeration to 1.2.144.1906
- Dependencies - Updated Meziantou.Analyzer to 3.0.98
- Enable publish trimming on server binary, reducing self-contained linux-x64 single-file size from 100MB to 22MB
- Remove Serilog.Enrichers.Demystifier — it uses trim-unsafe reflection (StackFrame, Type.GetMethods, Module.ResolveMember) incompatible with PublishTrimmed
### Removed
### Deployment Changes
<!--
Releases that have at least been deployed to staging, BUT NOT necessarily released to live.  Changes should be moved from [Unreleased] into here as they are merged into the appropriate release branch
-->
## [0.0.12] - 2026-06-02
### Fixed
- Use EphemeralKeySet when loading HTTPS certificate to fix Docker container startup failure
- ExecuteResultAsync now checks git process exit code after WaitForExitAsync and throws GitException on non-zero exit, preventing silent HTTP 200 responses on git failures
- EnsureNotLocked now detects bare (mirror) repos and checks the correct lock file path (<workingDir>/index.lock instead of <workingDir>/.git/index.lock)
- ExtractForm was reading values from Request.Query instead of Request.Form, causing legacy POST RPC calls to silently fail
- ExtractForm now returns empty collection for non-form POST requests instead of throwing InvalidOperationException
### Changed
- Dependencies - Updated Credfeto.Version.Information.Generator to 1.0.125.1199
- Dependencies - Updated Credfeto.Enumeration to 1.2.143.1876
- Dependencies - Updated FunFair.CodeAnalysis to 7.2.0.1978
- Dependencies - Updated Meziantou.Analyzer to 3.0.96
- Dependencies - Updated SonarAnalyzer.CSharp to 10.27.0.140913
- Dependencies - Updated Credfeto.Docker.HealthCheck.Http.Client to 0.0.65.765
- Dependencies - Updated Credfeto.Extensions.Linq to 1.0.149.1594
- Dependencies - Updated Credfeto.Services.Startup to 1.1.147.1632
- Dependencies - Updated FunFair.Test.Common to 6.2.25.2243
- Dependencies - Updated FunFair.Test.Source.Generator to 6.2.25.2243
- Dependencies - Updated Microsoft.Extensions to 10.0.8

## [0.0.11] - 2026-05-15
### Added
- Unhandled exception middleware returning HTTP 429 with Retry-After: 30 so clients back off during transient upstream failures
### Fixed
- Fixed typo in UnitTests.props import path variable $(SolutonDir) → $(SolutionDir) in all test projects; also moved Cache.Tests import to directly after PropertyGroup for consistency
### Changed
- Dependencies - Updated Figgle to 0.6.6
- Dependencies - Updated Philips.CodeAnalysis.DuplicateCodeAnalyzer to 2.0.0
- Dependencies - Updated Philips.CodeAnalysis.MaintainabilityAnalyzers to 2.0.0
- Dependencies - Updated Serilog.Extensions.Logging to 10.0.0
- Dependencies - Updated Serilog.Sinks.Console to 6.1.1
- Dependencies - Updated Serilog to 4.3.1
- Dependencies - Updated System.Interactive.Async to 7.0.1
- Dependencies - Updated Microsoft.Extensions to 10.0.7
- Dependencies - Updated Credfeto.Version.Information.Generator to 1.0.124.1183
- Dependencies - Updated FunFair.Test.Common to 6.2.23.2204
- Dependencies - Updated SonarAnalyzer.CSharp to 10.25.0.139117
- Dependencies - Updated Credfeto.Date to 1.1.151.1695
- Dependencies - Updated Credfeto.Docker.HealthCheck.Http.Client to 0.0.64.749
- Dependencies - Updated Credfeto.Extensions.Linq to 1.0.148.1565
- Dependencies - Updated Credfeto.Services.Startup to 1.1.145.1592
- Dependencies - Updated Credfeto.Enumeration to 1.2.142.1836
- Dependencies - Updated FunFair.CodeAnalysis to 7.1.42.1940
- Dependencies - Updated FunFair.Test.Common to 6.2.23.2204
- Dependencies - Updated Meziantou.Analyzer to 3.0.74

## [0.0.10] - 2026-03-15
### Changed
- SDK - Updated DotNet SDK to 10.0.200

## [0.0.9] - 2026-03-07
### Changed
- Dependencies - Updated Credfeto.Enumeration to 1.2.137.1722
- Dependencies - Updated Meziantou.Analyzer to 3.0.15
- Dependencies - Updated Credfeto.Extensions.Linq to 1.0.144.1467

## [0.0.8] - 2026-02-23
### Changed
- Dependencies - Updated AsyncFixer to 2.1.0
- Dependencies - Updated Credfeto.Enumeration to 1.2.135.1701
- Dependencies - Updated Credfeto.Version.Information.Generator to 1.0.119.1070
- Dependencies - Updated FunFair.CodeAnalysis to 7.1.32.1699
- Dependencies - Updated Meziantou.Analyzer to 2.0.299
- Dependencies - Updated Microsoft.Sbom.Targets to 4.1.5
- Dependencies - Updated Philips.CodeAnalysis.MaintainabilityAnalyzers to 1.9.1
- Dependencies - Updated Roslynator.Analyzers to 4.15.0
- Dependencies - Updated SonarAnalyzer.CSharp to 10.19.0.132793
- Dependencies - Updated xunit.analyzers to 1.27.0
- Dependencies - Updated xunit.v3 to 3.2.2
- Dependencies - Updated Credfeto.Date to 1.1.145.1569
- Dependencies - Updated Credfeto.Docker.HealthCheck.Http.Client to 0.0.60.641
- Dependencies - Updated Credfeto.Extensions.Linq to 1.0.143.1450
- Dependencies - Updated Credfeto.Services.Startup to 1.1.139.1474
- SDK - Updated DotNet SDK to 10.0.103

## [0.0.7] - 2026-01-21
### Changed
- SDK - Updated DotNet SDK to 10.0.102

## [0.0.6] - 2025-12-17
### Changed
- SDK - Updated DotNet SDK to 10.0.101

## [0.0.5] - 2025-11-17
### Added
- Support for PARU
### Changed
- Moved caching to separate service and to priorities info reads through the local cache

## [0.0.4] - 2025-11-13
### Added
- Support serving repos over http
### Changed
- SDK - Updated DotNet SDK to 10.0.100

## [0.0.3] - 2025-09-20
### Changed
- Dependencies - Updated Credfeto.Enumeration to 1.2.129.1430
- Dependencies - Updated Credfeto.Version.Information.Generator to 1.0.115.836
- Dependencies - Updated FunFair.CodeAnalysis to 7.1.24.1452
- Dependencies - Updated Meziantou.Analyzer to 2.0.220
- Dependencies - Updated Philips.CodeAnalysis.MaintainabilityAnalyzers to 1.9.1
- Dependencies - Updated Credfeto.Date to 1.1.138.1330
- Dependencies - Updated Credfeto.Docker.HealthCheck.Http.Client to 0.0.53.355
- Dependencies - Updated Microsoft.Extensions to 9.0.9

## [0.0.2] - 2025-09-11
### Changed
- SDK - Updated DotNet SDK to 9.0.305

## [0.0.1] - 2025-08-26
### Changed
- SDK - Updated DotNet SDK to 9.0.304

## [0.0.0] - Project created