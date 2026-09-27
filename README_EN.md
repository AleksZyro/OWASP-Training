# OWASP Forge

[Deutsch](./README.md) | **English**

OWASP Forge is a local ASP.NET Core learning platform for practising defensive web security. It combines twelve stations across four short courses, from injection and access control to secure architecture and software supply chains. **It is not a scanner, exploit tool or security certification.** Historical variants are marked *intentionally vulnerable* and are never executed.

## Project identity

This repository is part of the public portfolio of **Aleksandar Nikolić** (**Aleksandar Nikolic**, **AleksZyro**), an IMS student from Buchs AG, Switzerland.

- Portfolio: [aleksandar-nikolic.ch](https://aleksandar-nikolic.ch/)
- GitHub: [github.com/AleksZyro](https://github.com/AleksZyro)
- Contact and current availability: [aleksandar-nikolic.ch/#contact](https://aleksandar-nikolic.ch/#contact)

## Project Status

Current status: **local learning-platform MVP**. German and English interfaces cover the same twelve stations and share local SQLite progress.

## Learning path

Each station starts with a concrete local scenario and a safe code comparison before the questions begin. Every station contains six progressive questions, staged hints and a solution explanation that is revealed only after the station is completed.

| Course | Stations |
| --- | --- |
| Input and output | SQL Injection, Cross-Site Scripting, File Upload |
| Identity and access | Authentication, Session and CSRF, IDOR |
| Secure architecture | SSRF, Cryptography and Secrets, Security Misconfiguration |
| Operations and supply chain | Dependencies and SBOM, Supply-Chain Integrity, Logging and Monitoring |

That is twelve stations and 72 learning steps in total. Progress, points and reset state are stored locally; the project does not provide a public leaderboard or claim certification.

## Main features

- server-side answer validation and feedback for unsafe choices
- concrete examples before each question, with a clear distinction between historical and secure variants
- local progress, points, not-started/in-progress/completed states and reset per station
- staged hints with a transparent point deduction
- clear continuation to the next station, language switching within an open station and a local privacy notice
- German and English interface at `/` and `/en`
- six separate, safe local demo stations with fixed localhost ports; the other six stations are network-free concept lessons

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

Docker Compose exposes the platform only on `127.0.0.1:5080`. Run `docker compose --profile demos up --build` to start the six separately isolated demos on `127.0.0.1:5101` through `5106`. Progress in the Docker learning run is intentionally temporary; the regular local run stores it in `src/OwaspForge.Web/data/forge.db`.

## Tests and Quality Checks

```bash
dotnet build OwaspForge.sln --no-restore
dotnet test OwaspForge.sln --no-restore
dotnet format OwaspForge.sln --verify-no-changes
docker compose config
```

## Security Boundaries

OWASP Forge never scans hosts, stores real credentials, executes user code or accepts arbitrary URLs. The SSRF lesson allows only fixed mock identifiers. Containers are read-only, capability-free, on dedicated internal Docker networks and exposed only through fixed localhost ports. See the [security model](docs/SECURITY-MODEL.md).

Before a public or school deployment, complete and review the operator identity, contact details and legal basis in [docs/PRIVACY.md](docs/PRIVACY.md). The bundled local reference instance is not an online service.

## Documentation

[Architecture](docs/ARCHITECTURE.md) · [Security model](docs/SECURITY-MODEL.md) · [Privacy and operator checklist](docs/PRIVACY.md) · [Challenge guide](docs/CHALLENGES.md) · [Adding challenges](docs/ADDING-CHALLENGES.md) · [Troubleshooting](docs/TROUBLESHOOTING.md)

## Release and operator documentation

[Privacy and operator checklist](docs/PRIVACY.md) · [Security Policy](SECURITY.md) · [Changelog](CHANGELOG.md) · [License](LICENSE)

## Known Limitations

- The sandbox is a didactic model, not a production hardening assessment.
- Docker engine availability is required only for container runs.
- Docker image build and runtime checks require a running Docker engine.

## Sources

- [OWASP Top 10](https://owasp.org/Top10/)
- [OWASP SQL Injection](https://owasp.org/www-community/attacks/SQL_Injection)
- [OWASP Cross-Site Scripting](https://owasp.org/www-community/attacks/xss/)
- [OWASP Access Control](https://owasp.org/Top10/A01_2021-Broken_Access_Control/)
- [OWASP Authentication](https://owasp.org/Top10/A07_2021-Identification_and_Authentication_Failures/)
- [OWASP SSRF](https://owasp.org/Top10/A10_2021-Server-Side_Request_Forgery_%28SSRF%29/)
