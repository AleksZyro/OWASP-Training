# Sicherheitsmodell und Architektur

`OwaspForge.Web` ist die Plattform; `OwaspForge.Core` enthält Registry und serverseitige Prüfregeln. Browserwerte sind kein Abschlussnachweis. SQLite speichert nur ID und Abschlusszeit lokal. CSP, `nosniff`, `DENY`, `no-referrer`, CSRF-Schutz und zentrale Fehlerbehandlung sind aktiv. Keine Eingabe erreicht Shell-Befehle oder einen Netzwerkausgang.
