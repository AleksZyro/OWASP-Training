# OWASP Forge – Plan

## Wartbarkeit und Kursübersicht – 17. Oktober 2026

- Aktuelles `origin/main` vor Änderungen abrufen und mit lokalem `main` vergleichen; bestehende Änderungen aus dem vorherigen Lauf erhalten.
- Challenge-Inhalte in vier Kursordner mit je drei explizit registrierten Stationsmodulen zerlegen. Gemeinsame Metadaten und Antwortschlüssel nur einmal definieren; DE-/EN-Texte gehören jeweils in dasselbe Stationsmodul.
- Katalog beim Aufbau auf eindeutige IDs/Reihenfolge, vollständige Lokalisierung und bekannte Demo-Referenzen prüfen. Bestehende Registry-Schnittstellen, URLs und Fortschritts-IDs erhalten.
- Startseiten mit Fortschritt pro Kurs, eindeutigen Statusbezeichnungen und Fortsetzung einer begonnenen Station verbessern; semantische Gruppen, Code, Hinweise, Feedback, Fokus und mobile Darstellung prüfen.
- Tests für Katalogparität, sichere Defaults, bestehende Fortschrittsdatensätze und Browser-Flows ausführen. Erst nach Restore, Build, Tests, Formatierung und Compose-Prüfung direkt auf `main` committen und auf `origin/main` pushen.

## Projektziel

OWASP Forge ist eine lokale Lernplattform für zwölf grundlegende OWASP-Themen in vier Kursen. Sie ist kein Scanner und kein Angriffswerkzeug: Demos, Prüfungen und Docker-Container sind ausschliesslich für die mitgelieferten Lerninhalte vorgesehen.

## Architektur

- `src/OwaspForge.Web`: ASP.NET Core 8 Razor Pages, sichere HTTP-Standardkonfiguration und SQLite-Fortschritt.
- `src/OwaspForge.Core`: Challenge-Registry, serverseitige Prüfregeln und Fortschrittslogik.
- `challenges`: je Challenge eine klar markierte, statische Docker-Sandbox ohne Netzwerk.
- `tests`: xUnit-Tests für Registry, Prüfungen und HTTP-Integration.

## MVP

Startseite mit Fortschritt, Challenge-Detailseiten mit Lerninhalt, gestaffelten Hinweisen, einer serverseitig geprüften Antwortauswahl mit spezifischem Feedback, Reset und einer einblendbaren Lösung. Vier Kursblöcke enthalten je drei Stationen mit je sechs Fragen (72 Fragen): Eingabe und Ausgabe (`sql-injection`, `xss`, `file-upload`), Identität und Zugriffe (`authentication`, `session-csrf`, `idor`), sichere Architektur (`ssrf`, `cryptography-secrets`, `security-misconfiguration`) sowie Betrieb und Lieferkette (`dependencies-sbom`, `supply-chain-integrity`, `logging-monitoring`). Die sechs ursprünglichen Demo-Stationen behalten ihre isolierten, statischen lokalen Docker-Demos; die sechs Ergänzungen sind sichere Konzeptstationen ohne Netzwerkzugriff.

## Sicherheitsmodell

Die Plattform bindet standardmässig nur an `127.0.0.1`. Client-Daten werden nie als Abschlussnachweis vertraut; die Lösung wird serverseitig gegen eine Registry-Regel validiert. Es gibt keine Shell-Ausführung, keine frei wählbaren URLs und keine Host-Scans. Container sind read-only und ohne Capabilities. Demos liegen in getrennten internen Netzen; die Plattform verwendet eine separate Bridge mit festem Loopback-Host-Port. Die Demo-Container enthalten nur lokale Erklärseiten. Alle historischen unsicheren Varianten sind als **intentionally vulnerable** gekennzeichnet und werden nicht ausgeführt.

## Teststrategie

`dotnet test` deckt Challenge-Regeln und HTTP-Flows ab. `dotnet format --verify-no-changes` prüft Formatierung. Compose-Konfiguration wird mit `docker compose config` geprüft; Docker-Start und manueller Smoke-Test werden ausgeführt, falls Docker im lokalen System verfügbar ist. CI führt Restore, Build, Test, Format, Abhängigkeitsprüfung und Gitleaks aus.

## Definition of Done

Alle zwölf Stationen werden angezeigt, können abgeschlossen und zurückgesetzt werden, speichern Fortschritt lokal in SQLite und haben Tests. Dokumentation, Docker Compose und GitHub Actions erklären bzw. automatisieren die lokale sichere Ausführung.

## Risiken und Grenzen

Die Sandbox ist ein didaktisches Modell und keine Produktions-Härtungsprüfung. Ohne laufende Docker-Engine lassen sich die statischen Challenge-Container nicht starten; die Plattform selbst bleibt lokal ausführbar.

## Spätere Erweiterungen

Separate kurzlebige Container pro Lernlauf, Playwright-E2E-Tests, weitere OWASP-Themen, lokalisierte Inhalte und ein Export des persönlichen Lernfortschritts.

## UI-Redesign: Security Lab

Das Redesign verwendet ein dunkles, zugängliches Lab-Console-System: Graphit und tiefes Navy als Fläche, Cyan für Interaktion, Grün für gesicherte Zustände, Amber für Evidenz und Rot nur für Risiken. Die Startseite wird zum vertikalen Missionspfad; Challenge-Seiten führen als klar nummerierter Ablauf durch Kontext, Auswirkung, Prüfung und Lösung. Die deutsche und englische UI erhalten dieselbe Struktur. Es gibt keine externen Fonts, Telemetrie oder schweren Frontend-Abhängigkeiten; der Fokus liegt auf Kontrast, Tastaturbedienung und mobilen Breakpoints.

## Qualitätsverbesserung: Lernfluss und Verifikation

Ein gestarteter Lernlauf wird lokal als «in Bearbeitung» gespeichert und kann weiterhin mit Reset entfernt werden. Die Startseiten empfehlen die nächste noch nicht gesicherte Station, ohne eine Rangliste oder Zertifizierung zu suggerieren. HTTP-Integrationstests prüfen die lokalen Security-Header, beide Sprachen sowie den Antiforgery-geschützten Abschlussablauf. Da die Plattform über HTTP auf localhost läuft, verwendet das Antiforgery-Cookie keinen `__Host-`-Präfix, der zwingend HTTPS voraussetzt.

## Stabilisierung: Bedienung und Laufzeit

Der Sprachwechsel bewahrt auf Challenge-Seiten die aktuelle Stations-ID. Erfolgreiche Prüfungen führen zur nächsten offenen Station, ein Reset bestätigt den tatsächlichen lokalen Zustand und verhindert doppelte Formularübermittlungen. Die SQLite-Schemaerstellung erfolgt mit einer versionierten EF-Core-Migration und übernimmt bestehende lokale Fortschrittsdatenbanken. Das Dockerfile führt zuerst einen lockdateibasierten Restore aus; Compose hält die Plattform auf einer separaten Bridge und veröffentlicht sie nur über `127.0.0.1`; diese Bridge ist keine Egress-Sperre.

## UX-Nachschärfung: geführter Kursweg

Der Lernpfad wird als ruhige, vertikale Folge von Stationen statt als technisches Dashboard dargestellt. Numerische Badges und dichte Informationsboxen werden reduziert: Status wird zusätzlich als Text vermittelt, Metadaten treten hinter Lernziel und nächster Handlung zurück. Auf der Detailseite folgt der Inhalt einer durchgehenden Lernsequenz (Kontext, Auswirkung, Schutzmassnahme, Ergebnis, Lösung); die historische, absichtlich unsichere Variante bleibt als klar abgegrenzter Risikohinweis sichtbar.

## Ausbau: isolierte Demo-Stationen und Lieferqualität

Jede der sechs Stationen erhält eine eigene, statische Demo-App in einem separaten internen Docker-Netz. Die Demos sind interaktive Sicherheits-Simulationen, keine ausführbaren Schwachstellen: Sie akzeptieren keine beliebigen Netzadressen, führen keine Uploads oder Skripte aus und enthalten weder reale Zugangsdaten noch produktive Daten. Die Compose-Demos werden nur über feste localhost-Ports veröffentlicht. Ergänzend kommen eine Architektur-, Erweiterungs- und Troubleshooting-Dokumentation, eine CI-Abhängigkeitsprüfung sowie Browser-Flow-Tests hinzu. Docker-Build und Container-Scan bleiben an eine verfügbare Docker-Engine gebunden.

## Lernfluss und faire Bewertung

Die Detailseite startet mit einer greifbaren lokalen Ausgangslage: pro Station werden eine historische, klar markierte «intentionally vulnerable»-Variante und eine sichere Richtung gegenübergestellt. Erst danach führt ein schlichtes «Weiter» zur Antwortauswahl. Hinweise werden einzeln und in Reihenfolge freigeschaltet; jeder Hinweis kostet 10 Punkte. Falsche Antworten werden serverseitig protokolliert und kosten 15 Punkte. Die Anzeige begrenzt die erzielten Punkte auf mindestens null. Die Lösungserklärung bleibt bis zur korrekt validierten Antwort verborgen; bereits abgeschlossene Challenges lassen sich nicht durch nachträgliche Formularaufrufe verändern. Redundante Beschriftungen wie «Test» oder technische Prüfhinweise werden vermieden. Die Regeln gelten identisch in deutscher und englischer Oberfläche.

## Lerninhalte: erst verstehen, dann anwenden

Jede Station zeigt vor der Antwortauswahl eine konkrete lokale Situation, die Sicherheitsauswirkung, die Aufgabe und einen Vergleich zwischen historischer und sicherer Variante. Drei kurze Merkpunkte übersetzen das Beispiel in praktische Regeln. Die vorhandenen sechs Fragen prüfen anschliessend dieselbe Schutzidee in unterschiedlichen Situationen; Hinweise bleiben optional und werden transparent von der Punktzahl abgezogen. Die Inhalte und Merkpunkte sind in Deutsch und Englisch deckungsgleich, damit Sprache nur die Darstellung und nicht den Lernumfang verändert.

## Application-Security-Labor: Nachhol-Loop

Der Ausbau macht die vorhandene Lernplattform als Portfolio-Labor nachvollziehbarer: pro Kernstation stehen historische, klar markierte Ursachen, sichere Gegenmassnahmen, lokale HTTP-Evidenz und Regressionstests nebeneinander. Der Plattformcode erhält strukturierte Ereignislogs ohne Antworten oder Geheimnisse. GET-Anfragen bleiben seiteneffektfrei; persistenter Fortschritt entsteht nur aus antiforgery-geschützten POST-Aktionen. Compose verwendet für Demos interne Netze und für die Plattform eine separate Bridge, capability-freie read-only-Container und eine dokumentierte, nicht geheime `.env.example`.

## Kursausbau: zwölf Stationen mit je sechs Fragen

Jede Station enthält sechs aufeinander aufbauende Fragen. Eine Station gilt erst als abgeschlossen, wenn alle sechs Antworten serverseitig korrekt sind; fehlende oder falsche Antworten werden einzeln bewertet. Damit entstehen 72 Lernschritte in vier klaren Kursblöcken. Weitere Stationen können künftig als zusätzliche `ChallengeDefinition`-Einträge ergänzt werden, ohne die Fortschritts- oder Sicherheitsgrenzen zu verändern.

## Release-Härtung

Vor einem öffentlichen oder schulischen Release müssen Betreiberidentität und Kontaktangaben in `docs/PRIVACY.md` ergänzt, Docker-Build und Smoke-Test mit einer verfügbaren Engine ausgeführt und die CI-Prüfungen erfolgreich abgeschlossen werden. Die CI startet alle sechs Demo-Container kurzzeitig, prüft die Plattform über localhost und Demo-HTTP innerhalb der isolierten Container und räumt sie anschliessend auf. Release-Metadaten, MIT-Lizenz und Changelog sind enthalten; eine rechtliche Prüfung der konkreten Betreiberinstanz bleibt erforderlich.
