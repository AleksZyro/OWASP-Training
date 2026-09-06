# Sicherheitsmodell und Architektur

`OwaspForge.Web` ist die Plattform; `OwaspForge.Core` enthält Registry und serverseitige Prüfregeln. Browserwerte sind kein Abschlussnachweis. SQLite speichert nur Challenge-ID und Abschlusszeit lokal. Der Fortschritt wird mit einer versionierten EF-Core-Migration initialisiert; bestehende lokale `EnsureCreated`-Datenbanken werden beim ersten Start ohne Datenverlust in die Migrationshistorie übernommen.

Die Plattform bindet lokal standardmässig nur an `127.0.0.1:5080`. Im Docker-Lauf ist ausschliesslich der Host-Port an `127.0.0.1` gebunden; das Plattformnetz ist intern und die separate Sandbox hat gar kein Netzwerk. CSP, `nosniff`, `DENY`, `no-referrer`, Cross-Origin-Policies, eingeschränkte Browser-Features, CSRF-Schutz und zentrale Fehlerbehandlung sind aktiv. Keine Eingabe erreicht Shell-Befehle oder einen Netzwerkausgang.
