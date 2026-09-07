# Neue Challenge hinzufügen

1. Eine stabile, kleingeschriebene ID in beiden Registries (`ChallengeRegistry` und `EnglishChallengeRegistry`) anlegen.
2. Lernziel, Sicherheitsauswirkung, erwarteten Fix, zwei Hinweise, OWASP-Quelle und genau eine serverseitig richtige Option ergänzen.
3. Einen Validator-Test für richtige und falsche Optionen hinzufügen.
4. Falls eine Demo nötig ist: einen neuen Ordner unter `challenges/<id>` mit einer rein statischen Simulation erstellen. Keine realen Payloads, Uploads, Netzabrufe, Zugangsdaten oder dynamische Codeausführung hinzufügen.
5. Den neuen Demo-Service mit festem `127.0.0.1`-Port, `read_only`, `cap_drop: ALL`, `no-new-privileges` und einem eigenen `internal`-Netz in Compose registrieren.
6. Einen HTTP-Test für die feste Demo-URL und die deutsche sowie englische Oberfläche ergänzen.
7. Build, Tests, Formatter und `docker compose --profile demos config` ausführen.
