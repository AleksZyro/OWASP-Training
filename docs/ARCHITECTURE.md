# Architektur

OWASP Forge trennt Lernplattform, Lernregeln, lokale Fortschrittsdaten und Demo-Stationen.

```text
Browser ── localhost:5080 ──> Razor Pages ──> Core-Registry / Validator
                                     │
                                     └──> SQLite (nur lokaler Fortschritt)

Browser ── localhost:5101–5106 ──> einzelne statische Demo-Container
```

`OwaspForge.Core` besitzt die stabilen Challenge-IDs, Lerntexte und die serverseitigen korrekten Antworten. `OwaspForge.Web` enthält Razor Pages, Antiforgery-Schutz, sichere Header und die EF-Core-Migration für SQLite. Browserdaten entscheiden nie über den Abschluss.

Jede Demo ist ein eigener unprivilegierter Nginx-Container. Der Port ist nur an `127.0.0.1` gebunden; jedes Demo-Netz ist Docker-intern und von den anderen Demos getrennt. Die interaktiven Seiten sind sichere Simulationen: Sie haben keine Datenbank, keinen Upload-Endpunkt, keinen Netzabruf und keine ausführbare historische Variante.

## Datenfluss

1. Eine Lernperson öffnet eine Challenge auf der Plattform.
2. Die Plattform markiert sie lokal als begonnen.
3. Nach einer Antiforgery-geschützten Antwort validiert der Server die feste Registry-Regel.
4. Nur der Server schreibt den lokalen Abschluss in SQLite.
5. Die optionale Demo illustriert das Thema getrennt davon und erhält keine Plattformdaten.
