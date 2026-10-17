# OWASP Forge – Local Application Security Lab

[Deutsch](./README.md) | **English**

OWASP Forge is a local ASP.NET Core lab for defensive secure coding. It demonstrates how common web weaknesses arise, which server-side rule prevents them, and how a regression test protects that rule. It is **not a scanner, exploit tool, or security certification**.

The repository is part of **Aleksandar Nikolić's** portfolio ([website](https://aleksandar-nikolic.ch/) · [GitHub](https://github.com/AleksZyro)). OWASP Forge is not a separate legal entity or hosted service.

## Purpose and safety boundary

The application is only for a local, controlled learning environment. It does not attack third-party targets, accept arbitrary destination URLs, execute user code, store real credentials, or include malware, persistence, or exfiltration functionality. Historical variants are visibly marked *intentionally vulnerable* but are never executed.

The platform and optional demo containers publish fixed `127.0.0.1` ports only. Containers are read-only and capability-free. Each demo has its own internal network. The platform uses a separate bridge network for host access; that network does not block egress. See the [security model](docs/SECURITY-MODEL.md).

## Security labs

The UI provides four courses, twelve stations, and 72 progressive questions. Every station follows the same sequence: context → historical cause → secure measure → server-side check → explanation → reset.

| Lab | Vulnerability | Local demonstration | Mitigation | Regression test |
| --- | --- | --- | --- | --- |
| SQL Injection | Data is assembled into SQL code | Inert local search | Parameters separate structure and values | Command text stays unchanged for injection-like input |
| Cross-Site Scripting | Untrusted text is treated as HTML | Safe text rendering | Context-aware output encoding and CSP | Markup is encoded as text |
| File Upload | Type, name, and location are trusted | No actual file ingestion | Allowlist, limits, generated name, storage outside web root | The server validates the learning rule |
| Authentication | Direct comparison and weak sessions | No real credentials | Hashing, server-side session checks, limits | The server validates the learning rule |
| Sessions and CSRF | State changes lack origin validation | Antiforgery-protected forms | SameSite/HttpOnly and token validation | POST without a token is rejected |
| IDOR | Object ID is confused with permission | Local ownership simulation | Check ownership or role | A foreign owner is denied |
| SSRF | Client selects the destination | Fixed mock key; no fetch | Server allowlist and no container egress | An arbitrary URL cannot resolve |
| Configuration and operations | Unsafe defaults | Visible local controls | Headers, neutral errors, logs without secrets | HTTP tests verify headers |

Further stations cover cryptography and secrets, SBOMs, supply-chain integrity, and logging and monitoring. The project does not claim complete OWASP Top 10 coverage.

## Architecture

```text
Browser ── localhost:5080 ──> Razor Pages ──> Core registry / validator
                                      │
                                      └──> SQLite (local learning progress)

Browser ── localhost:5101–5106 ──> separate static demo containers
```

- `src/OwaspForge.Web`: Razor Pages, antiforgery, central security headers, SQLite migrations, and secret-free event logs.
- `src/OwaspForge.Core`: fixed challenge definitions, server-side validation, and small side-effect-free secure-coding examples.
- `challenges`: six isolated static browser simulations; none contains an exploitable vulnerable backend.
- `tests`: unit, HTTP integration, browser, and security regression tests.

Twelve station modules are grouped by course under `src/OwaspForge.Core/Challenges/`. Each module keeps German and English text together while defining language-independent metadata and answer rules once. The explicit catalog validates parity, ordering, and known demo references. Existing IDs and SQLite progress stay compatible. The course overview shows progress per course and resumes started stations.

The HTTP evidence in a station shows the expected local method, response decision, headers, identity state, and authorization decision. It is educational information and makes no extra network request.

## Vulnerable → Secure → Regression

1. A clearly separated historical variant explains the root cause.
2. A local simulation explains the impact without a real attack.
3. The secure direction names the server-side mitigation.
4. The challenge checks answers on the server.
5. Tests protect parameter binding, output encoding, ownership, and mock allowlisting from regression.

## Run locally

Prerequisites: the .NET SDK defined in `global.json`; Docker Desktop is optional for container demos.

```powershell
git clone https://github.com/AleksZyro/OWASP-Training.git
cd OWASP-Training
dotnet restore --locked-mode
dotnet run --project src/OwaspForge.Web
```

Open `http://127.0.0.1:5080` for German or `http://127.0.0.1:5080/en` for English. Progress stays local in `src/OwaspForge.Web/data/forge.db`.

```powershell
# Platform in an isolated local container
docker compose up --build

# Also start the six static demos
docker compose --profile demos up --build
```

The demos use `127.0.0.1:5101` to `5106`. `.env.example` documents optional Compose configuration; `.env` is ignored.

## Tests and quality checks

```powershell
dotnet build OwaspForge.sln --no-restore
powershell -File tests/OwaspForge.Tests/bin/Debug/net8.0/playwright.ps1 install chromium
dotnet test OwaspForge.sln --no-restore
dotnet format OwaspForge.sln --verify-no-changes
dotnet list OwaspForge.sln package --vulnerable --include-transitive
docker compose --profile demos config
```

GitHub Actions runs locked restore, build, tests, formatting, dependency checks, a Compose build/smoke check, Trivy, and Gitleaks.

## Compact threat model

| Element | Security assumption |
| --- | --- |
| Assets | local progress, antiforgery token, registry integrity, and container isolation |
| Trust boundaries | browser → Razor Pages → SQLite; host → fixed loopback ports; repository → CI runner |
| Threat actors | a foreign browser origin or unprivileged local process — not the Docker daemon or host administrator |
| Attack surfaces | routes, forms, static demo input, local configuration, and CI supply chain |
| Assumptions | single-user local use, trusted process configuration, and no public deployment |

## Privacy and operation

The reference instance processes local learning progress only: station ID, status, timestamps, attempts, hints, and point deductions. There are no accounts, names, email addresses, or real credentials. Before public or school hosting, add the operator identity, contact channel, legal basis, and hosting configuration from the [privacy checklist](docs/PRIVACY.md).

## What this project demonstrates

- Parameter binding, encoding, object authorization, and allowlists are testable rules, not buzzwords.
- CSRF protection, secure headers, and neutral errors belong in the actual web application.
- Logs support investigation without exposing answers, passwords, or tokens.
- Tests and reproducible CI make secure changes reviewable.

## Further documentation

[Architecture](docs/ARCHITECTURE.md) · [Challenge guide](docs/CHALLENGES.md) · [Security model](docs/SECURITY-MODEL.md) · [Add a challenge](docs/ADDING-CHALLENGES.md) · [Troubleshooting](docs/TROUBLESHOOTING.md) · [Security policy](SECURITY.md) · [Changelog](CHANGELOG.md)

## Sources

[OWASP Top 10](https://owasp.org/Top10/) · [OWASP SQL Injection](https://owasp.org/www-community/attacks/SQL_Injection) · [OWASP XSS](https://owasp.org/www-community/attacks/xss/) · [OWASP Broken Access Control](https://owasp.org/Top10/A01_2021-Broken_Access_Control/) · [OWASP SSRF](https://owasp.org/Top10/A10_2021-Server-Side_Request_Forgery_%28SSRF%29/)
