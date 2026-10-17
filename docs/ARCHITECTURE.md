# Architektur

OWASP Forge trennt Lernplattform, Lernregeln, lokale Fortschrittsdaten und Demo-Stationen.

```text
Browser ── localhost:5080 ──> Razor Pages ──> Core-Registry / Validator
                                     │
                                     └──> SQLite (nur lokaler Fortschritt)

Browser ── localhost:5101–5106 ──> einzelne statische Demo-Container
```

`OwaspForge.Core` besitzt die stabilen Challenge-IDs, Lerntexte, serverseitigen korrekten Antworten und kleine testbare Secure-Coding-Regeln. `OwaspForge.Web` enthält Razor Pages, Antiforgery-Schutz, sichere Header, strukturierte Logs ohne Geheimnisse und die EF-Core-Migration für SQLite. Browserdaten entscheiden nie über den Abschluss; GET-Anfragen sind seiteneffektfrei, Fortschritt entsteht nur über geschützte POST-Aktionen.

Jede Demo ist ein eigener unprivilegierter Nginx-Container. Der Port ist nur an `127.0.0.1` gebunden; jedes Demo-Netz ist Docker-intern und von den anderen Demos getrennt. Die interaktiven Seiten sind sichere Simulationen: Sie haben keine Datenbank, keinen Upload-Endpunkt, keinen Netzabruf und keine ausführbare historische Variante.

## Challenge-Katalog und Lokalisierung

```text
OwaspForge.Core/Challenges/
  ChallengeCatalog.cs      explizite Registrierung, Validierung und Sprachprojektion
  ChallengeModule.cs       Metadaten, Antwortregeln und lokalisierter Inhalt
  CourseCatalog.cs         stabile Kurs-IDs und übersetzte Kursnamen
  DemoCatalog.cs           bekannte Demo-IDs und feste Loopback-Ports
  InputOutput/            sql-injection.json, xss.json, file-upload.json
  IdentityAccess/         authentication.json, session-csrf.json, idor.json
  SecureArchitecture/     ssrf.json, cryptography-secrets.json, security-misconfiguration.json
  SupplyChain/            dependencies-sbom.json, supply-chain-integrity.json, logging-monitoring.json
```

Jedes Stationsmodul enthält genau eine `Metadata`-Definition und gemeinsame `Rules` für Antwortschlüssel, Antwortreihenfolge und richtige Antworten. `German` und `English` enthalten die zugehörigen Titel, Lerntexte, Hinweise, Fragen und Antwortbeschriftungen. Eine englische Frage wird deshalb nicht mehr über einen möglicherweise wiederverwendeten Antwortschlüssel pauschal übersetzt.

JSON-Inhalte sind Build-Ressourcen in der Core-Assembly. Der Katalog lädt nur die ausdrücklich registrierten Ressourcen, ohne Verzeichnissuche, Plugin-Erkennung oder Netzwerkzugriff. Er prüft beim Aufbau eindeutige IDs und Reihenfolge, bekannte Kurse/Demos, vollständige Texte und die exakte Zuordnung beider Sprachen zur gemeinsamen Antwortstruktur. Die beiden bisherigen Registry-Klassen bleiben als kleine, kompatible Sprachansichten erhalten.

IDs, Stationsreihenfolge, Antwortschlüssel und URLs bleiben stabil. Das SQLite-Schema benötigt für diese Modularisierung keine Migration. Bestehende Abschlussdaten, Hinweise und Punkteabzüge werden weiterhin über dieselbe Challenge-ID gelesen. Die Startseite priorisiert begonnene offene Stationen; bei mehreren bleibt die Katalogreihenfolge massgebend. Ohne Aktivitätszeitstempel wird kein vermeintlich letzter Zugriff erfunden.

## Datenfluss

1. Eine Lernperson öffnet eine Challenge auf der Plattform.
2. Mit «Weiter» wird die Station über einen Antiforgery-geschützten POST als begonnen markiert; das reine Öffnen bleibt lesend.
3. Nach einer Antiforgery-geschützten Antwort validiert der Server die feste Registry-Regel.
4. Nur der Server schreibt den lokalen Abschluss in SQLite.
5. Die optionale Demo illustriert das Thema getrennt davon und erhält keine Plattformdaten.

## HTTP-Evidenz

Wo ein Protokollbezug hilft, liefert die Registry eine statische lokale Erwartung für Methode, Status, relevante Header, Identitätszustand und Autorisierungsentscheid. Sie dient dem Verständnis und erzeugt keinen HTTP-Abruf. Die tatsächlichen HTTP-Schutzheader sowie Antiforgery werden zusätzlich in Integrationstests geprüft.
