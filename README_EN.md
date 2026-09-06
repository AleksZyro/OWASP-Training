# OWASP Forge

[Deutsch](./README.md) | **English**

OWASP Forge is a local ASP.NET Core learning platform for SQL Injection, XSS, IDOR, authentication, file upload and SSRF. **It is not a scanner, exploit tool or security certification.** Historical variants are marked *intentionally vulnerable* and are never executed.

## Project Status

Current status: **local learning-platform MVP**. German and English interfaces cover the same six challenges and share local SQLite progress.

## Main Features

- six local OWASP challenges with staged hints
- server-side answer validation and feedback for unsafe choices
- local progress, points, not-started/in-progress states and reset per challenge
- clear continuation to the next station, language switching within an open challenge and a local privacy notice
- German and English interface at `/` and `/en`

## Tech Stack

- .NET 8 / ASP.NET Core Razor Pages
- Entity Framework Core and SQLite
- xUnit, Docker Compose and GitHub Actions

## Installation and Development Start

```bash
git clone https://github.com/AleksZyro/OWASP-Training.git
cd OWASP-Training
dotnet restore --locked-mode
dotnet run --project src/OwaspForge.Web
```

Open `http://127.0.0.1:5080` for German or `http://127.0.0.1:5080/en` for English.

Docker Compose exposes the platform only on `127.0.0.1:5080`. Progress in the Docker learning run is intentionally temporary; the regular local run stores it in `src/OwaspForge.Web/data/forge.db`.

## Tests and Quality Checks

```bash
dotnet build OwaspForge.sln --no-restore
dotnet test OwaspForge.sln --no-restore
dotnet format OwaspForge.sln --verify-no-changes
docker compose config
```

## Security Boundaries

OWASP Forge never scans hosts, stores real credentials, executes user code or accepts arbitrary URLs. The SSRF lesson allows only fixed mock identifiers. Containers are read-only, capability-free and have no network. See the [security model](docs/SECURITY-MODEL.md).

## Known Limitations

- The sandbox is a didactic model, not a production hardening assessment.
- Docker engine availability is required only for container runs.
- Versioned EF Core migrations and browser E2E tests are planned follow-up work.

## Sources

- [OWASP Top 10](https://owasp.org/Top10/)
- [OWASP SQL Injection](https://owasp.org/www-community/attacks/SQL_Injection)
- [OWASP Cross-Site Scripting](https://owasp.org/www-community/attacks/xss/)
