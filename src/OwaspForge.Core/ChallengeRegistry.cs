namespace OwaspForge.Core;

public sealed class ChallengeRegistry
{
    private static readonly IReadOnlyList<ChallengeDefinition> Definitions =
    [
        Create("sql-injection", "SQL Injection", "A03: Injection", "Einfach", "20 Min.", 100,
            "Du erkennst, warum Daten nie in SQL-Text zusammengesetzt werden dürfen.",
            "Die historische Demo markiert eine String-Verkettung als intentionally vulnerable. Die sichere Variante trennt Abfragecode und Werte durch Parameter.",
            "Wähle die Massnahme, die eine lokale Suchabfrage sicher macht.", "Angreifende könnten lokale Beispieldaten ausserhalb der vorgesehenen Suche sehen.",
            "Die Suche behandelt Eingaben nur als Wert und liefert keine unerwarteten Datensätze.",
            "https://owasp.org/www-community/attacks/SQL_Injection", "Parameter haben Typ und Wert getrennt vom SQL-Code.", "Auch eine Validierung ersetzt keine parametrisierte Abfrage.",
            ("concat", "Eingabe vor dem SQL-Text escapen"), ("parameter", "Parametrisierte Abfrage mit gebundenem Wert"), ("filter", "Nur Sonderzeichen sperren")),
        Create("xss", "Cross-Site Scripting", "A03: Injection", "Einfach", "15 Min.", 100,
            "Du verstehst, dass Ausgaben kontextgerecht encodiert werden müssen.",
            "Die historische Anzeige ist intentionally vulnerable, weil sie Text als HTML interpretiert. Die sichere Ansicht behandelt den Beitrag als Text.",
            "Wähle die sichere Ausgabe für einen lokalen Kommentar.", "Fremder Code könnte im Browserkontext der Lernseite laufen.",
            "Der Kommentar erscheint als Text; Markup wird nicht ausgeführt.", "https://owasp.org/www-community/attacks/xss/", "Razor encodiert normale Textausgaben automatisch.", "Verwende keine HTML-Roh-Ausgabe für Benutzereingaben.",
            ("raw", "Text als HTML-Rohinhalt ausgeben"), ("encode", "Kontextgerecht HTML-encodieren"), ("client", "Nur im Browser filtern")),
        Create("idor", "Broken Access Control / IDOR", "A01: Broken Access Control", "Mittel", "25 Min.", 150,
            "Du prüfst Zugriffe auf jedes einzelne Objekt serverseitig.",
            "Die historische Demo vertraut auf eine Objekt-ID aus der URL und ist intentionally vulnerable. Die sichere Variante vergleicht Eigentümer und aktuelle Identität auf dem Server.",
            "Wähle die Prüfung vor dem Lesen eines lokalen Profils.", "Lokale Beispieldaten einer anderen Person wären sichtbar.",
            "Nur die berechtigte Demobenutzerin erhält ihr eigenes Profil; andere IDs ergeben eine Ablehnung.", "https://owasp.org/Top10/A01_2021-Broken_Access_Control/", "Eine ID ist kein Berechtigungsnachweis.", "Prüfe Besitz oder Rolle auf dem Server, nicht im Link.",
            ("hidden", "ID in einem versteckten Feld speichern"), ("owner", "Aktuelle Identität gegen Objektbesitz prüfen"), ("route", "Objekt-ID nur anders benennen")),
        Create("authentication", "Unsichere Authentifizierung", "A07: Identification and Authentication Failures", "Mittel", "30 Min.", 150,
            "Du kombinierst Passwort-Hashing, Session-Prüfung und Rate Limits.",
            "Die historische Login-Logik ist intentionally vulnerable: sie vergleicht einen Demo-String direkt. Die sichere Variante verwendet einen Passwort-Hasher, serverseitige Session-Prüfung und Limits für Fehlversuche.",
            "Wähle das vollständige lokale Schutzpaket.", "Konten könnten durch triviale Prüfungen oder viele Versuche gefährdet sein.",
            "Passwörter werden nie im Klartext gespeichert, Sitzungen werden serverseitig geprüft und Fehlversuche begrenzt.", "https://owasp.org/Top10/A07_2021-Identification_and_Authentication_Failures/", "Ein Hash ist kein Verschlüsselungsschlüssel.", "Rate Limits ergänzen, aber ersetzen keine sichere Passwortspeicherung.",
            ("plain", "Passwort direkt vergleichen und im Speicher behalten"), ("secure-auth", "Hashing, serverseitige Session-Prüfung und Rate Limit"), ("captcha", "Nur ein Captcha hinzufügen")),
        Create("file-upload", "Unsicherer Datei-Upload", "A04: Insecure Design", "Mittel", "25 Min.", 150,
            "Du validierst Typ, Grösse und Dateiname und speicherst ausserhalb statischer Inhalte.",
            "Die historische Upload-Variante ist intentionally vulnerable, weil sie Namen und Pfad direkt übernimmt. Die sichere Variante vergibt einen serverseitigen Namen und speichert nur erlaubte, kleine Dateien ausserhalb des Webroots.",
            "Wähle die sichere lokale Upload-Regel.", "Unerwartete Dateien könnten zugänglich werden oder Speicher erschöpfen.",
            "Nur erlaubte, begrenzte Dateien werden unter einem serverseitigen Namen ausserhalb des Webroots gespeichert; sie werden nie ausgeführt.", "https://owasp.org/www-community/vulnerabilities/Unrestricted_File_Upload", "Dateiendungen allein beweisen keinen Inhaltstyp.", "Verwende eine Allowlist, Grössenlimit und einen generierten Namen.",
            ("extension", "Nur die Dateiendung prüfen und im Webroot speichern"), ("upload-policy", "Allowlist, Grössenlimit, generierter Name, Speicher ausserhalb Webroot"), ("rename", "Die hochgeladene Datei nur umbenennen")),
        Create("ssrf", "SSRF-Grundlagen", "A10: Server-Side Request Forgery", "Mittel", "20 Min.", 150,
            "Du begrenzt serverseitige Abrufe auf explizite lokale Mock-Ziele.",
            "Die historische Abrufidee ist intentionally vulnerable, weil sie eine beliebige Adresse akzeptiert. OWASP Forge ruft nie eine vom Client angegebene URL auf: die sichere Demo arbeitet nur mit einer festen lokalen Mock-Kennung.",
            "Wähle die sichere Abrufregel.", "Ein Server könnte sonst interne oder externe Ziele im falschen Kontext anfragen.",
            "Nur eine registrierte Mock-Ressource wird verarbeitet; beliebige URLs werden abgelehnt und Container haben kein Netzwerk.", "https://owasp.org/Top10/A10_2021-Server-Side_Request_Forgery_%28SSRF%29/", "Eine Blocklist kann unbekannte Ziele übersehen.", "Nimm eine feste Allowlist von Namen, nicht URLs vom Client.",
            ("blocklist", "Einige Hostnamen auf einer Blocklist ablehnen"), ("allowlist", "Nur feste lokale Mock-Kennungen aus einer Allowlist akzeptieren"), ("redirect", "Weiterleitungen erst nach dem Abruf prüfen"))
    ];

    public IReadOnlyList<ChallengeDefinition> All => Definitions;

    public ChallengeDefinition? Find(string id) => Definitions.SingleOrDefault(challenge => StringComparer.Ordinal.Equals(challenge.Id, id));

    private static ChallengeDefinition Create(string id, string title, string category, string difficulty, string duration, int points, string goal, string explanation, string task, string impact, string expected, string url, string hintOne, string hintTwo, params (string Value, string Label)[] options) =>
        new(id, title, category, difficulty, duration, points, goal, explanation, task, impact, expected, url, [hintOne, hintTwo], options.Select(option => new ChallengeOption(option.Value, option.Label)).ToArray(), options[1].Value);
}
