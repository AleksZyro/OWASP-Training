# OWASP Forge – Plan

## Projektziel

OWASP Forge ist eine lokale Lernplattform für sechs grundlegende OWASP-Risiken. Sie ist kein Scanner und kein Angriffswerkzeug: Demos, Prüfungen und Docker-Container sind ausschliesslich für die mitgelieferten Lerninhalte vorgesehen.

## Architektur

- `src/OwaspForge.Web`: ASP.NET Core 8 Razor Pages, sichere HTTP-Standardkonfiguration und SQLite-Fortschritt.
- `src/OwaspForge.Core`: Challenge-Registry, serverseitige Prüfregeln und Fortschrittslogik.
- `challenges`: je Challenge eine klar markierte, statische Docker-Sandbox ohne Netzwerk.
- `tests`: xUnit-Tests für Registry, Prüfungen und HTTP-Integration.

## MVP

Startseite mit Fortschritt, Challenge-Detailseiten mit Lerninhalt, gestaffelten Hinweisen, einer serverseitig geprüften Antwortauswahl mit spezifischem Feedback, Reset und einer einblendbaren Lösung. Die sechs IDs sind `sql-injection`, `xss`, `idor`, `authentication`, `file-upload` und `ssrf`.

## Sicherheitsmodell

Die Plattform bindet standardmässig nur an `127.0.0.1`. Client-Daten werden nie als Abschlussnachweis vertraut; die Lösung wird serverseitig gegen eine Registry-Regel validiert. Es gibt keine Shell-Ausführung, keine frei wählbaren URLs und keine Host-Scans. Container sind read-only, ohne Capabilities und mit `network_mode: none`; sie enthalten nur lokale Erklärseiten. Alle historischen unsicheren Varianten sind als **intentionally vulnerable** gekennzeichnet und werden nicht ausgeführt.

## Teststrategie

`dotnet test` deckt Challenge-Regeln und HTTP-Flows ab. `dotnet format --verify-no-changes` prüft Formatierung. Compose-Konfiguration wird mit `docker compose config` geprüft; Docker-Start und manueller Smoke-Test werden ausgeführt, falls Docker im lokalen System verfügbar ist. CI führt Restore, Build, Test, Format, Abhängigkeitsprüfung und Gitleaks aus.

## Definition of Done

Die sechs Challenges werden angezeigt, können abgeschlossen und zurückgesetzt werden, speichern Fortschritt lokal in SQLite und haben Tests. Dokumentation, Docker Compose und GitHub Actions erklären bzw. automatisieren die lokale sichere Ausführung.

## Risiken und Grenzen

Die Sandbox ist ein didaktisches Modell und keine Produktions-Härtungsprüfung. Ohne laufende Docker-Engine lassen sich die statischen Challenge-Container nicht starten; die Plattform selbst bleibt lokal ausführbar.

## Spätere Erweiterungen

Separate kurzlebige Container pro Lernlauf, Playwright-E2E-Tests, weitere OWASP-Themen, lokalisierte Inhalte und ein Export des persönlichen Lernfortschritts.

## UI-Redesign: Security Lab

Das Redesign verwendet ein dunkles, zugängliches Lab-Console-System: Graphit und tiefes Navy als Fläche, Cyan für Interaktion, Grün für gesicherte Zustände, Amber für Evidenz und Rot nur für Risiken. Die Startseite wird zum vertikalen Missionspfad; Challenge-Seiten führen als klar nummerierter Ablauf durch Kontext, Auswirkung, Prüfung und Lösung. Die deutsche und englische UI erhalten dieselbe Struktur. Es gibt keine externen Fonts, Telemetrie oder schweren Frontend-Abhängigkeiten; der Fokus liegt auf Kontrast, Tastaturbedienung und mobilen Breakpoints.
