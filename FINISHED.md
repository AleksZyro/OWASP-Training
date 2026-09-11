# OWASP Forge – Abschluss

## Implementiert

- Razor-Pages-Plattform auf `127.0.0.1:5080`, sechs OWASP-Kurse mit je drei aufeinander aufbauenden Fragen (18 Lernschritte), geführtem Intro-/Prüfungsablauf, gestaffelten Hinweisen mit Punkteabzug, sicherer Lösungsauswahl, nachvollziehbarem Reset, Weiterführung und SQLite-Fortschritt.
- Jede Frage hat eigene Optionen und jede Station eine fachbezogene Lösungserklärung; Antworten werden gemeinsam serverseitig validiert.
- Neues neutrales SVG-Schildlogo ohne «OF»-Initialen; die Oberfläche bleibt vollständig lokal und ohne externe Assets.
- Sechs getrennte, statische und interaktive lokale Demo-Container auf `127.0.0.1:5101` bis `5106`; je Demo ein separates internes Docker-Netz, read-only Dateisystem, keine Capabilities und keine ausnutzbare historische Variante.
- Serverseitige Registry-Validierung, CSRF-Schutz, erweiterte sichere Standardheader, zentrale Fehlerbehandlung, versionierte EF-Core-Migrationen, Playwright-Browser-Flow und CI-Prüfungen für Abhängigkeiten, Compose-Build, Trivy-Dateisystemscan und Gitleaks.
- README, Beitrags-, Verhaltens- und Sicherheitsrichtlinie sowie Architektur-, Challenge-, Erweiterungs- und Troubleshooting-Dokumentation.

## Ausgeführte Prüfungen

- `dotnet build OwaspForge.sln --no-restore` – erfolgreich, 0 Warnungen/Fehler.
- `dotnet test OwaspForge.sln --no-restore` – 29/29 erfolgreich.
- Playwright Chromium installiert und Browser-Flow tatsächlich ausgeführt – 1/1 erfolgreich.
- `dotnet format OwaspForge.sln --verify-no-changes` – erfolgreich.
- `docker compose config` – erfolgreich.
- Lokaler HTTP-Smoketest: Start auf `127.0.0.1:5080`, deutsch- und englischsprachige Startseite, Datenschutzhinweise und alle sechs Challenge-Routen HTTP 200; CSP-, Frame- und Cross-Origin-Header bestätigt.
- Lokaler Paket-Vulnerability-Scan mit `dotnet list package --vulnerable --include-transitive` ausgeführt.
- Lokaler Paket-Vulnerability-Scan nach Testabhängigkeits-Upgrade – keine anfälligen Pakete.
- Regressionstest bestätigt, dass `?solved=true` die Lösung nicht vor einer serverseitig bestätigten Antwort freischaltet.
- Codex-Security-Diff-Scan über die Demo-, Browser- und localhost-Port-Änderungen: keine berichtspflichtigen Findings.

## Einschränkungen

Die Challenge-Demos sind bewusst sichere, interaktive Erklärungen statt ausführbarer verwundbarer Anwendungen. Docker Compose wurde syntaktisch geprüft; Docker-Build, Compose-Start und Container-Image-Scan konnten lokal nicht ausgeführt werden, weil Docker Desktop auf diesem Rechner nicht läuft. Die CI enthält nun einen Start-/HTTP-Smoke-Test und räumt die Container danach auf. Vor einer öffentlichen oder schulischen Bereitstellung müssen Betreiberidentität und Kontaktangaben in der Datenschutzvorlage ergänzt und rechtlich geprüft werden. Der Docker-Lernlauf verwendet absichtlich temporären Fortschritt; der reguläre lokale Start speichert ihn persistent.

## Start und nächste Schritte

`dotnet run --project src/OwaspForge.Web`; optional `docker compose up --build`. Hinweise kosten je 10 Punkte, falsche Antworten 15 Punkte; die Lösung wird erst nach einer korrekten Antwort freigeschaltet.
