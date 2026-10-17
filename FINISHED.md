# OWASP Forge – Abschluss

## Wartbarkeit und Kursübersicht – 17. Oktober 2026

- Zwölf Stationsmodule in vier Kursordnern unter `OwaspForge.Core/Challenges`; pro Modul gemeinsame Metadaten und Antwortregeln, daneben vollständige DE-/EN-Inhalte. Die bisherigen Registries sind kleine Sprachansichten.
- Stabile IDs, Kurs-/Stationsreihenfolge, URLs und SQLite-Datensätze bleiben kompatibel. Katalogvalidierung und Tests sichern Übersetzungsparität, Antwortstruktur, bekannte Demos und vollständige Pflichttexte.
- Zuvor über Antwortschlüssel pauschal übersetzte englische Optionen sind jetzt je Frage übersetzt; Hinweise und Aufgaben sind stationsbezogen.
- Vier direkt erreichbare Kurse mit eigenem Fortschritt, Gesamtabschlusszahl, eindeutigen Statusbezeichnungen und Fortsetzung begonnener Stationen. Ohne Aktivitätszeitstempel wird keine zeitlich letzte Station behauptet.
- «Weiter» startet eine Station mit einem geschützten POST. GET bleibt lesend. Nach dem Sprachwechsel bleibt die Lösung einer abgeschlossenen Station sichtbar; nach Abschluss sind weitere Punkteabzüge gesperrt.
- Browserprüfung mit tatsächlich geladenem CSS: DE/EN bei 375, 768, 1024 und 1440 Pixeln, keine horizontalen Seitenüberläufe, sichtbare Tastaturnavigation sowie Abschluss, Sprachwechsel und Reset. Screenshots liegen ausserhalb des Repositorys im temporären Verzeichnis.
- Sichere Header, Antiforgery, Cookie-Flags, HTML-Encoding, geheimnisfreie Logs und feste Demo-Ports im Code geprüft; keine neue externe Netzwerkfunktion. CI prüft die Plattform über ihren localhost-Port und die sechs Demo-HTTP-Endpunkte innerhalb der isolierten Container.
- Die auf Remote-main eingeführte separate Plattform-Bridge bleibt erhalten. Sie ist keine Egress-Sperre; die Demo-Netze bleiben intern. Die frühere, ungeprüfte Behauptung eines internen Plattformnetzes wurde in der Dokumentation berichtigt.
- Dokumentation für neue Module und Architektur aktualisiert; keine neue Bibliothek und kein geändertes Datenbankschema.

Aktuelle Verifikation: Locked Restore, Build ohne Warnungen/Fehler, 45/45 xUnit-Tests inklusive Chromium, Formatprüfung sowie Compose-Konfiguration mit und ohne Demo-Profil erfolgreich. NuGet meldet keine bekannten anfälligen Pakete; der lokale Secret-Pattern-Check hat keine Treffer. `ruff check src tests` findet keine Python-Dateien, `pytest -q` keine Python-Tests; die fachlichen Tests sind die xUnit-Suite.

Docker-Build und Container-Smoke-Test bleiben lokal unverifiziert: Auch nach einem begrenzten Startversuch liefert Docker Desktop keine erreichbare Linux-Engine. Die CI enthält diese Prüfungen; ein erfolgreicher Remote-Lauf wird hier nicht behauptet.

## Nachhol-Loop: Application-Security-Labor

- GET-Routen sind seiteneffektfrei; ein Regressionstest verhindert, dass blosses Öffnen einer Station Fortschritt schreibt.
- Kernregeln für Parameterbindung, HTML-Encoding, Ownership und lokale SSRF-Mock-Allowlisting sind als side-effect-free Beispiele implementiert und getestet.
- Stationen zeigen, wo sinnvoll, eine lokale HTTP-Evidenz mit Methode, Status, Headern, Identitätszustand und Entscheid.
- Ereignislogs enthalten nur Stations-ID, Ereignistyp und sichere Zähler – keine Antworten, Passwörter oder Tokens.
- Compose entfernt Linux-Capabilities auch bei der Plattform; `.env.example` dokumentiert die nicht geheime Datenpfad-Option. Die Plattform bleibt auf ihrer separaten Bridge, Demos auf internen Netzen.
- README und englisches README beschreiben Purpose, Labs, Architektur, Lernmodell, Tests, Threat Model, Datenschutz und Grenzen im tatsächlichen Umfang.

## Implementiert

- Razor-Pages-Plattform auf `127.0.0.1:5080`, vier OWASP-Kurse mit je drei Stationen und je sechs aufeinander aufbauenden Fragen (12 Stationen, 72 Lernschritte), geführtem Lernintro mit Auswirkung, Lernfokus, Codevergleich und drei Merkpunkten, gestaffelten Hinweisen mit Punkteabzug, sicherer Lösungsauswahl, nachvollziehbarem Reset, Weiterführung und SQLite-Fortschritt.
- Jede Frage hat eigene Optionen und jede Station eine fachbezogene Lösungserklärung; Antworten werden gemeinsam serverseitig validiert.
- Neues neutrales SVG-Schildlogo ohne «OF»-Initialen; die Oberfläche bleibt vollständig lokal und ohne externe Assets.
- Sechs getrennte, statische und interaktive lokale Demo-Container auf `127.0.0.1:5101` bis `5106`; je Demo ein separates internes Docker-Netz, read-only Dateisystem, keine Capabilities und keine ausnutzbare historische Variante.
- Serverseitige Registry-Validierung, CSRF-Schutz, erweiterte sichere Standardheader, zentrale Fehlerbehandlung, versionierte EF-Core-Migrationen, Playwright-Browser-Flow und CI-Prüfungen für Abhängigkeiten, Compose-Build, Trivy-Dateisystemscan und Gitleaks.
- README, Beitrags-, Verhaltens- und Sicherheitsrichtlinie sowie Architektur-, Challenge-, Erweiterungs- und Troubleshooting-Dokumentation.

## Ausgeführte Prüfungen

- `dotnet build OwaspForge.sln --no-restore` – am 14. Oktober 2026 erfolgreich, 0 Warnungen/Fehler.
- `dotnet test OwaspForge.sln --no-build --no-restore` – am 14. Oktober 2026 36/36 erfolgreich, inklusive Browser-Flow und neuer Security-Regressionen.
- Playwright Chromium installiert und Browser-Flow tatsächlich ausgeführt – 1/1 erfolgreich.
- `dotnet format OwaspForge.sln --verify-no-changes` – erfolgreich.
- Lokaler HTTP-Smoketest am 14. Oktober 2026: deutsche Startseite, englische Startseite und SQL-Injection-Station je HTTP 200; HTTP-Evidenz, CSP und `X-Frame-Options: DENY` bestätigt.
- `docker compose config` – erfolgreich.
- Lokaler HTTP-Smoketest: Start auf `127.0.0.1:5080`, deutsch- und englischsprachige Startseite, Datenschutzhinweise und alle sechs Challenge-Routen HTTP 200; CSP-, Frame- und Cross-Origin-Header bestätigt.
- Lokaler Paket-Vulnerability-Scan mit `dotnet list package --vulnerable --include-transitive` ausgeführt.
- Lokaler Paket-Vulnerability-Scan nach Testabhängigkeits-Upgrade – keine anfälligen Pakete.
- `dotnet list OwaspForge.sln package --vulnerable --include-transitive` – am 14. Oktober 2026 keine anfälligen Pakete.
- Lokaler Secret-Pattern-Check – am 14. Oktober 2026 keine Treffer für private Schlüssel oder typische Zugangstoken.
- Codex Security Standard Scan – am 14. Oktober 2026 abgeschlossen; ein Low-Finding zu zustandsverändernden GET-Routen identifiziert und in diesem Loop behoben.
- Regressionstest bestätigt, dass `?solved=true` die Lösung nicht vor einer serverseitig bestätigten Antwort freischaltet.
- Codex-Security-Diff-Scan über die Demo-, Browser- und localhost-Port-Änderungen: keine berichtspflichtigen Findings.
- Nach dem Kursausbau: `dotnet build --configuration Release` – erfolgreich, 0 Warnungen/Fehler; `dotnet test --configuration Release --no-build` – 30/30 erfolgreich.
- Nach der Lerninhalts- und UI-Runde: `dotnet build OwaspForge.sln --configuration Release --no-restore` – erfolgreich, 0 Warnungen/Fehler; `dotnet test OwaspForge.sln --configuration Release --no-build` – 30/30 erfolgreich; `dotnet format OwaspForge.sln --verify-no-changes --no-restore` – erfolgreich.
- Lokaler Smoke-Test nach der Lerninhalts- und UI-Runde: deutsche und englische Station mit Lernrahmen, drei Merkpunkten und Quiz-Weiterleitung sowie Startseite mit vertikalem Kursweg und zwölf Stationen auf `127.0.0.1:5099` geprüft.

## Einschränkungen

Die Challenge-Demos sind bewusst sichere, interaktive Erklärungen statt ausführbarer verwundbarer Anwendungen. Docker Compose wurde syntaktisch geprüft; Docker-Build, Compose-Start und Container-Image-Scan konnten lokal nicht ausgeführt werden, weil Docker Desktop auf diesem Rechner nicht läuft. Die CI enthält nun einen Start-/HTTP-Smoke-Test und räumt die Container danach auf. Vor einer öffentlichen oder schulischen Bereitstellung müssen Betreiberidentität und Kontaktangaben in der Datenschutzvorlage ergänzt und rechtlich geprüft werden. Der Docker-Lernlauf verwendet absichtlich temporären Fortschritt; der reguläre lokale Start speichert ihn persistent.

## Start und nächste Schritte

`dotnet run --project src/OwaspForge.Web`; optional `docker compose up --build`. Hinweise kosten je 10 Punkte, falsche Antworten 15 Punkte; die Lösung wird erst nach einer korrekten Antwort freigeschaltet.
