# OWASP Forge

Lokale ASP.NET-Core-Lernplattform für SQL Injection, XSS, IDOR, Authentifizierung, Datei-Upload und SSRF. **Kein Scanner, kein Exploit-Werkzeug und keine Sicherheitszertifizierung.** Historische Varianten sind *intentionally vulnerable* markiert und werden nicht ausgeführt.

## Start

Mit .NET SDK 9: `dotnet run --project src/OwaspForge.Web`. Die Plattform bindet ausschliesslich an `http://127.0.0.1:5080`; Fortschritt liegt lokal in `data/forge.db`. Optional: `docker compose up --build` nutzt ebenfalls nur `127.0.0.1:5080`.

## Sicherheitsgrenzen

Keine Host-Scans, echten Zugangsdaten, frei wählbaren URLs, Benutzercode oder externen Ziele. Der SSRF-Teil akzeptiert nur feste Mock-Kennungen. Container sind read-only, ohne Capabilities und ohne Netzwerk. Details: [Sicherheitsmodell](docs/SECURITY-MODEL.md).

## Qualität

`dotnet test`, `dotnet format --verify-no-changes` und `docker compose config`.

## Quellen

[OWASP Top 10](https://owasp.org/Top10/), [SQL Injection](https://owasp.org/www-community/attacks/SQL_Injection), [XSS](https://owasp.org/www-community/attacks/xss/).
