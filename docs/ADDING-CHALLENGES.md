# Neue Challenge hinzufügen

1. Im passenden Kursordner unter `src/OwaspForge.Core/Challenges/` ein Stationsmodul als JSON anlegen. Am bestehenden Modul orientieren: `Metadata`, `Rules`, `German` und `English` sind die vier Bereiche.
2. In `Metadata` eine neue stabile ID, eine bekannte `CourseId`, eine eindeutige `Order`, Schwierigkeit (`easy` oder `medium`), Minuten, Punkte und OWASP-Quelle eintragen. Bestehende IDs und Reihenfolge nicht umnummerieren oder für andere Themen wiederverwenden.
3. Antwortschlüssel, ihre Reihenfolge und die richtige Antwort nur unter `Rules` definieren. In beiden Sprachbereichen müssen dieselben Schlüssel exakt vorkommen; Titel, Fragen, Optionen, Erklärungen, zwei Hinweise und Merkpunkte sind vollständig auszufüllen. Neue Stationen folgen weiterhin dem Lernablauf mit sechs Fragen.
4. Die neue Ressourcen-ID in `ChallengeCatalog.LoadModules` ausdrücklich registrieren. Das Projekt bettet JSON-Dateien automatisch als Ressourcen ein; eine Erweiterung benötigt keine neue Registry-Implementierung und kein Reflection- oder Plugin-System.
5. Falls eine Demo benötigt wird: eine rein statische Simulation unter `challenges/<id>` anlegen, einen Eintrag im `DemoCatalog` und dessen ID in `Metadata.DemoId` ergänzen. Ohne Demo bleibt `DemoId` null. Keine realen Uploads, Netzabrufe, Zugangsdaten oder dynamische Codeausführung hinzufügen.
6. Den Demo-Service mit festem `127.0.0.1`-Port, `read_only`, `cap_drop: ALL`, `no-new-privileges` und eigenem `internal`-Netz in Compose registrieren.
7. Katalog- und Paritätstests prüfen bereits gemeinsame Antwortregeln, bekannte Demo-Referenzen und vollständige Übersetzungen. Für die neue Station konkrete fachliche Regressionstests und gegebenenfalls einen HTTP-Demo-Linktest ergänzen. Erwartete Stationszahl und die bewusste Liste stabiler IDs im Kompatibilitätstest anpassen.
8. `dotnet restore --locked-mode`, Build, Tests, Formatter und `docker compose --profile demos config` ausführen. Bei UI-Änderungen Chromium mit dem generierten Playwright-Script installieren und den Browser-Flow bei den vier unterstützten Breiten prüfen.

Ein neues Modul verändert das Fortschrittsschema nicht. Eine neue ID beginnt ohne Fortschritt; bestehende IDs behalten Status, Abschlusszeit, Hinweise und Punkteabzüge. Ein Umbenennen einer ID wäre dagegen eine Datenmigration und braucht einen gesonderten Kompatibilitätsplan.
