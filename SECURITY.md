# Sicherheitsrichtlinie

OWASP Forge ist ausschliesslich ein lokales Lernprojekt. Die mitgelieferten Demos sind sichere Simulationen; sie dürfen nicht gegen fremde Hosts, Produktionsdaten oder reale Zugangsdaten eingesetzt werden.

## Unterstützte Versionen

| Version | Sicherheitsupdates |
| --- | --- |
| `main` | ja, solange das Projekt aktiv weiterentwickelt wird |
| veröffentlichte Tags | nach Möglichkeit; bitte den jeweiligen Release-Hinweis beachten |

## Meldung einer Sicherheitslücke

Bitte veröffentliche keine ausnutzbaren Details in einem öffentlichen Issue. Für ein öffentliches Repository ist der bevorzugte Kanal ein privates GitHub Security Advisory. Falls dieser Kanal für die konkrete Instanz nicht aktiviert ist, kontaktiere die verantwortliche Betreiberperson über den im Release angegebenen Kontaktweg und sende nur reproduzierbare Details zur lokalen Lernumgebung.

Es gibt keinen garantierten 24/7-Support und keine Zusage für eine bestimmte Reaktionszeit. Sicherheitsmeldungen werden nach Risiko, Reproduzierbarkeit und lokaler Reichweite priorisiert. Eine Meldung sollte betroffene Version, lokale Startart, erwartetes und tatsächliches Verhalten sowie einen sicheren Testnachweis enthalten; bitte keine echten Secrets oder externen Ziele mitsenden.

## Geltungsbereich

Im Geltungsbereich liegen Plattformcode, Challenge-Registry, Docker-Konfiguration und mitgelieferte Demo-Simulationen. Nicht im Geltungsbereich liegen Angriffe auf externe Systeme, nicht mitgelieferte Container, private Rechnerdaten und frei erfundene Produktionsszenarien.
