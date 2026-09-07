# Troubleshooting

## Port bereits belegt

Beende nur den Prozess, der den betroffenen localhost-Port belegt, oder passe lokal `launchSettings.json` beziehungsweise die Compose-Portzuordnung an. Die Plattform verwendet standardmässig 5080, die Demos 5101 bis 5106.

## Docker Desktop läuft nicht

Die Plattform funktioniert weiterhin mit `dotnet run --project src/OwaspForge.Web`. Starte Docker Desktop und prüfe danach `docker compose --profile demos config` sowie `docker compose --profile demos up --build`.

## Eine Demo ist nicht erreichbar

Der Demo-Profile muss explizit gestartet werden. Prüfe mit `docker compose --profile demos ps`, ob der zugehörige Container läuft. Demos sind absichtlich nur via `127.0.0.1` erreichbar.

## Fortschritt zurücksetzen

Nutze den Reset-Knopf der Station. Für einen vollständigen lokalen Neustart beende die Plattform und entferne ausschliesslich die ignorierte Datei `src/OwaspForge.Web/data/forge.db` mitsamt möglichen `-shm`- und `-wal`-Dateien.
