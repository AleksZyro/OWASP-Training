# OWASP Forge

Lokale ASP.NET-Core-Lernplattform für SQL Injection, XSS, IDOR, Authentifizierung, Datei-Upload und SSRF. **Kein Scanner, kein Exploit-Werkzeug und keine Sicherheitszertifizierung.** Historische Varianten sind *intentionally vulnerable* markiert und werden nicht ausgeführt.

## Funktionen

- sechs lokale OWASP-Kurse mit je drei aufeinander aufbauenden Fragen und gestaffelten Hinweisen (18 Lernschritte)
- serverseitige Prüfung der Antworten mit einer Erklärung zu jeder falschen Massnahme
- lokaler Fortschritt mit Punkten, Status «nicht begonnen», «in Bearbeitung» und Reset pro Challenge
- klare Weiterführung zur nächsten Station, Sprachwechsel innerhalb einer offenen Challenge und lokaler Datenschutzhinweis
- absichtlich unsichere Varianten werden nur erklärt und nie ausgeführt
- sechs getrennte lokale Demo-Stationen mit sicheren Interaktionen und festem localhost-Port

## Start

Mit dem in `global.json` festgelegten .NET SDK: `dotnet run --project src/OwaspForge.Web`. Die Plattform bindet ausschliesslich an `http://127.0.0.1:5080`; Fortschritt liegt lokal in `src/OwaspForge.Web/data/forge.db`. Optional: `docker compose up --build` nutzt ebenfalls nur `127.0.0.1:5080`. Der Docker-Lernlauf speichert Fortschritt absichtlich nur temporär im Container.

Die optionalen Demo-Stationen starten getrennt mit `docker compose --profile demos up --build`. Sie liegen auf `127.0.0.1:5101` bis `127.0.0.1:5106`; jede Station verlinkt direkt auf ihre Demo.

## Sicherheitsgrenzen

Keine Host-Scans, echten Zugangsdaten, frei wählbaren URLs, Benutzercode oder externen Ziele. Der SSRF-Teil akzeptiert nur feste Mock-Kennungen. Container sind read-only und ohne Capabilities; Plattform und jede Demo liegen ausschliesslich in internen Docker-Netzen und sind nur über ihren festen localhost-Port erreichbar. Details: [Sicherheitsmodell](docs/SECURITY-MODEL.md).

## Weitere Dokumentation

[Architektur](docs/ARCHITECTURE.md) · [Datenschutz und Betreiber-Checkliste](docs/PRIVACY.md) · [Neue Challenges](docs/ADDING-CHALLENGES.md) · [Troubleshooting](docs/TROUBLESHOOTING.md) · [Challenge-Leitfaden](docs/CHALLENGES.md)

## Qualität

`dotnet build OwaspForge.sln --no-restore`, `dotnet test OwaspForge.sln --no-restore`, `dotnet format OwaspForge.sln --verify-no-changes` und `docker compose config`.

## Quellen

[OWASP Top 10](https://owasp.org/Top10/), [SQL Injection](https://owasp.org/www-community/attacks/SQL_Injection), [XSS](https://owasp.org/www-community/attacks/xss/).
