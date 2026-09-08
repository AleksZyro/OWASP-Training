# Datenschutz und Betreiber-Checkliste

OWASP Forge ist standardmässig eine lokale Lernanwendung. Sie speichert den Fortschritt in einer SQLite-Datei auf dem Rechner, auf dem die Anwendung gestartet wurde. Es gibt keinen Benutzeraccount, keine Analyse- oder Werbedienste, keine Telemetrie und keine Übermittlung an OWASP.

## Verantwortlichkeit

Die Software wurde von **Aleksandar Zyro** erstellt. OWASP Forge ist keine eigene juristische Person und OWASP ist weder Betreiber dieser lokalen Instanz noch Empfänger von Lerndaten. Für eine öffentliche oder schulische Bereitstellung muss der Betreiber vor dem Start mindestens folgende Angaben ergänzen:

- vollständiger Name der verantwortlichen juristischen oder natürlichen Person;
- ladungsfähige Postadresse und Kontaktmöglichkeit;
- gegebenenfalls Datenschutzbeauftragte oder zuständige Schulstelle;
- anwendbares Recht und zuständige Aufsichtsbehörde;
- Freigabeprozess für Auftragsbearbeiter und Hosting.

Diese Angaben dürfen nicht aus dem Quelltext geraten oder erfunden werden. Die gebündelte Seite weist deshalb ausdrücklich auf die noch einzutragende Betreiberidentität hin. Dieses Dokument ist eine technische Vorlage und keine Rechtsberatung.

## Dateninventar

Die Anwendung schreibt nur lokale Fortschrittsdaten: Challenge-ID, Status, Abschlusszeitpunkt, Anzahl Versuche, verwendete Hinweise und Punkteabzüge. Formulare werden mit ASP.NET-Core-Antiforgery geschützt. Es werden keine Namen, E-Mail-Adressen, Passwörter, API-Schlüssel, Produktionsdaten oder Lernprofile über ein Netzwerk übertragen.

## Zweck und Löschung

Der Zweck ist ausschliesslich die Fortschrittsanzeige im lokalen Lernlauf. Einträge bleiben bis zum Zurücksetzen einer Station oder bis zum Löschen der Datenbank bestehen. Der komplette lokale Speicher liegt standardmässig unter `src/OwaspForge.Web/data/forge.db`; bei Docker ist der Speicher absichtlich temporär. Vor dem Löschen muss die Anwendung gestoppt werden.

## Infrastruktur und Dritte

Die Plattform bindet an `127.0.0.1`. Challenge-Container haben standardmässig kein Netzwerk und sind nur über fest konfigurierte localhost-Ports erreichbar. Es gibt keine externen Fonts, CDNs, Tracker, Cloud-Synchronisation oder beliebigen URL-Abrufe. Das technische Antiforgery-Cookie dient nur dem CSRF-Schutz. Logs dürfen keine Geheimnisse oder Antworten enthalten.

## Vor einer Veröffentlichung

Der Betreiber muss die Identität und Kontaktangaben einsetzen, eine rechtliche Prüfung für den konkreten Standort durchführen, Aufbewahrungs- und Löschfristen bestätigen, Schul-/Jugendschutzvorgaben prüfen und die Datenschutzerklärung in der tatsächlich eingesetzten Sprache veröffentlichen. Eine öffentliche Instanz darf nicht als von OWASP betriebener Dienst dargestellt werden.
