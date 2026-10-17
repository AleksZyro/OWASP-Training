# OWASP Forge – lokales Application-Security-Labor

**Deutsch** | [English](./README_EN.md)

OWASP Forge ist ein lokales ASP.NET-Core-Labor für defensives Secure Coding. Es zeigt, wie typische Web-Schwachstellen entstehen, welche serverseitige Regel sie verhindert und wie ein Regressionstest diese Regel absichert. Es ist **kein Scanner, kein Exploit-Werkzeug und keine Sicherheitszertifizierung**.

Das Projekt ist Teil des Portfolios von **Aleksandar Nikolić** ([Website](https://aleksandar-nikolic.ch/) · [GitHub](https://github.com/AleksZyro)). OWASP Forge ist keine eigene juristische Person und kein gehosteter Dienst.

## Zweck und Sicherheitsgrenzen

Die Anwendung ist ausschliesslich für die lokale, kontrollierte Lernumgebung gedacht. Sie greift keine fremden Ziele an, akzeptiert keine frei wählbaren Ziel-URLs, führt keinen Benutzercode aus, speichert keine echten Zugangsdaten und enthält weder Malware noch Persistenz- oder Exfiltrationsfunktionen. Historische Varianten sind sichtbar als *intentionally vulnerable* markiert, werden aber nicht ausgeführt.

Die Plattform und alle optionalen Demo-Container veröffentlichen nur feste `127.0.0.1`-Ports. Container sind read-only und ohne Linux-Capabilities. Jede Demo hat ein eigenes internes Netz. Die Plattform verwendet für den Host-Zugriff ein separates Bridge-Netz; dieses ist keine Egress-Sperre. Details: [Sicherheitsmodell](docs/SECURITY-MODEL.md).

## Security Labs

Die Oberfläche umfasst vier Kurse, zwölf Stationen und 72 schrittweise Fragen. Jede Station folgt demselben Ablauf: Kontext → historische Ursache → sichere Massnahme → serverseitige Prüfung → Lösung → Reset.

| Lab | Schwachstelle | Lokale Demonstration | Gegenmassnahme | Regressionstest |
| --- | --- | --- | --- | --- |
| SQL Injection | Daten werden als SQL-Code zusammengesetzt | Inerte lokale Suche | Parameter trennen Struktur und Wert | Query-Text bleibt bei injection-artigem Input unverändert |
| Cross-Site Scripting | Untrusted Text wird als HTML behandelt | Sichere Textausgabe | Kontextgerechtes Output-Encoding und CSP | Markup wird als Text encodiert |
| Datei-Upload | Typ, Name und Speicherort werden vertraut | Keine Dateiübernahme | Allowlist, Limits, generierter Name, Speicher ausserhalb Webroot | Lernregel wird serverseitig validiert |
| Authentifizierung | Direktvergleich und schwache Sitzungen | Keine echten Credentials | Hashing, serverseitige Session-Prüfung, Limits | Lernregel wird serverseitig validiert |
| Sitzungen und CSRF | Zustandsänderung ohne Herkunftsprüfung | Antiforgery-geschützte Formulare | SameSite/HttpOnly und Tokenprüfung | POST ohne Token wird abgewiesen |
| IDOR | Objekt-ID wird mit Berechtigung verwechselt | Lokale Besitzsimulation | Eigentümerschaft oder Rolle prüfen | Fremder Besitzer wird abgewiesen |
| SSRF | Client bestimmt Abrufziel | Feste Mock-Kennung, kein Fetch | Serverseitige Allowlist und kein Container-Egress | Beliebige URL wird nicht aufgelöst |
| Konfiguration und Betrieb | Unsichere Defaults | Sichtbare lokale Schutzregeln | Header, neutrale Fehler, Logs ohne Geheimnisse | HTTP-Tests prüfen Header |

Zusätzliche Stationen behandeln Kryptografie und Geheimnisse, SBOMs, Lieferkettenintegrität sowie Logging und Monitoring. Das Projekt behauptet keine vollständige OWASP-Top-10-Abdeckung.

## Architektur

```text
Browser ── localhost:5080 ──> Razor Pages ──> Core-Registry / Validator
                                      │
                                      └──> SQLite (lokaler Lernfortschritt)

Browser ── localhost:5101–5106 ──> getrennte statische Demo-Container
```

- `src/OwaspForge.Web`: Razor Pages, Antiforgery, zentrale Sicherheitsheader, SQLite-Migrationen und geheimnisfreie Event-Logs.
- `src/OwaspForge.Core`: feste Challenge-Definitionen, serverseitige Validierung und kleine side-effect-free Secure-Coding-Beispiele.
- `challenges`: sechs isolierte statische Browser-Simulationen; keine davon enthält ein ausnutzbares verwundbares Backend.
- `tests`: Unit-, HTTP-Integrations-, Browser- und Security-Regressionstests.

Die zwölf Stationsmodule liegen nach Kurs unter `src/OwaspForge.Core/Challenges/`. Jedes Modul bündelt DE-/EN-Texte und definiert sprachunabhängige Metadaten sowie Antwortregeln nur einmal. Der explizite Katalog prüft Parität, Reihenfolge und bekannte Demo-Referenzen; die bisherigen IDs und SQLite-Fortschritte bleiben erhalten. Die Kursübersicht zeigt Fortschritt pro Kurs und setzt begonnene Stationen fort.

Die HTTP-Evidenz einer Station zeigt die erwartete lokale Methode, Statusentscheidung, Header, Identitätszustand und Autorisierungsentscheidung. Sie ist didaktisch und löst keine zusätzliche Netzwerk-Anfrage aus.

## Vulnerable → Secure → Regression

1. Eine klar abgegrenzte historische Variante zeigt die Ursache.
2. Eine lokale Simulation erklärt die Auswirkung ohne reale Angriffsaktion.
3. Die sichere Richtung benennt die serverseitige Gegenmassnahme.
4. Die Challenge prüft Antworten auf dem Server.
5. Tests sichern Parameterbindung, Output-Encoding, Ownership und Mock-Allowlisting gegen Regressionen ab.

## Lokal starten

Voraussetzungen: das in `global.json` festgelegte .NET SDK; Docker Desktop ist optional für Container-Demos.

```powershell
git clone https://github.com/AleksZyro/OWASP-Training.git
cd OWASP-Training
dotnet restore --locked-mode
dotnet run --project src/OwaspForge.Web
```

Öffne `http://127.0.0.1:5080`; Englisch liegt unter `http://127.0.0.1:5080/en`. Fortschritt bleibt lokal in `src/OwaspForge.Web/data/forge.db`.

```powershell
# Plattform im isolierten lokalen Container
docker compose up --build

# Zusätzlich die sechs statischen Demos starten
docker compose --profile demos up --build
```

Die Demos liegen auf `127.0.0.1:5101` bis `5106`. `.env.example` dokumentiert die optionale Compose-Konfiguration; `.env` bleibt ignoriert.

## Tests und Qualitätschecks

```powershell
dotnet build OwaspForge.sln --no-restore
powershell -File tests/OwaspForge.Tests/bin/Debug/net8.0/playwright.ps1 install chromium
dotnet test OwaspForge.sln --no-restore
dotnet format OwaspForge.sln --verify-no-changes
dotnet list OwaspForge.sln package --vulnerable --include-transitive
docker compose --profile demos config
```

GitHub Actions führt Restore mit Lock-Dateien, Build, Tests, Formatierung, Dependency-Checks, Compose-Build/Smoke-Test, Trivy und Gitleaks aus.

## Kompaktes Threat Model

| Element | Schutzannahme |
| --- | --- |
| Assets | lokaler Fortschritt, Antiforgery-Token, Registry-Integrität und Container-Isolation |
| Vertrauensgrenzen | Browser → Razor Pages → SQLite; Host → feste Loopback-Ports; Repository → CI-Runner |
| Angreifende | fremde Browser-Origin oder unprivilegierter lokaler Prozess – nicht Docker-Daemon oder Host-Administrator |
| Angriffsflächen | Routen, Formulare, statische Demo-Eingaben, lokale Konfiguration und CI-Lieferkette |
| Annahmen | Einzelbenutzer-Lokalbetrieb, vertrauenswürdige Prozesskonfiguration und keine öffentliche Bereitstellung |

## Datenschutz und Betrieb

Die Referenzinstanz verarbeitet nur lokalen Lernfortschritt: Stations-ID, Status, Zeitstempel, Versuche, Hinweise und Punkteabzüge. Keine Konten, Namen, E-Mail-Adressen oder echten Zugangsdaten. Vor öffentlichem oder schulischem Betrieb müssen Betreiberidentität, Kontakt, Rechtsgrundlage und Hosting-Konfiguration gemäss [Datenschutz-Checkliste](docs/PRIVACY.md) ergänzt werden.

## Was dieses Projekt zeigt

- Parameterbindung, Encoding, objektbezogene Autorisierung und Allowlists sind überprüfbare Regeln, keine Schlagworte.
- CSRF-Schutz, sichere Header und neutrale Fehlerseiten gehören zur realen Webanwendung.
- Logs helfen bei der Untersuchung, ohne Antworten, Passwörter oder Tokens preiszugeben.
- Tests und reproduzierbare CI machen sichere Änderungen nachvollziehbar.

## Weitere Dokumentation

[Architektur](docs/ARCHITECTURE.md) · [Challenge-Leitfaden](docs/CHALLENGES.md) · [Sicherheitsmodell](docs/SECURITY-MODEL.md) · [Neue Challenge hinzufügen](docs/ADDING-CHALLENGES.md) · [Troubleshooting](docs/TROUBLESHOOTING.md) · [Security Policy](SECURITY.md) · [Changelog](CHANGELOG.md)

## Quellen

[OWASP Top 10](https://owasp.org/Top10/) · [OWASP SQL Injection](https://owasp.org/www-community/attacks/SQL_Injection) · [OWASP XSS](https://owasp.org/www-community/attacks/xss/) · [OWASP Broken Access Control](https://owasp.org/Top10/A01_2021-Broken_Access_Control/) · [OWASP SSRF](https://owasp.org/Top10/A10_2021-Server-Side_Request_Forgery_%28SSRF%29/)
