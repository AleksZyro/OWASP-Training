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
            ("concat", "Eingabe vor dem SQL-Text escapen", "Escaping ist kontextabhängig und kann bei späteren Änderungen der Abfrage umgangen werden."), ("parameter", "Parametrisierte Abfrage mit gebundenem Wert", ""), ("filter", "Nur Sonderzeichen sperren", "Eine Blocklist kann gültige Eingaben beschädigen und unbekannte Angriffsvarianten übersehen.")),
        Create("xss", "Cross-Site Scripting", "A03: Injection", "Einfach", "15 Min.", 100,
            "Du verstehst, dass Ausgaben kontextgerecht encodiert werden müssen.",
            "Die historische Anzeige ist intentionally vulnerable, weil sie Text als HTML interpretiert. Die sichere Ansicht behandelt den Beitrag als Text.",
            "Wähle die sichere Ausgabe für einen lokalen Kommentar.", "Fremder Code könnte im Browserkontext der Lernseite laufen.",
            "Der Kommentar erscheint als Text; Markup wird nicht ausgeführt.", "https://owasp.org/www-community/attacks/xss/", "Razor encodiert normale Textausgaben automatisch.", "Verwende keine HTML-Roh-Ausgabe für Benutzereingaben.",
            ("raw", "Text als HTML-Rohinhalt ausgeben", "Rohes HTML erlaubt, dass Eingaben als Markup oder Script interpretiert werden."), ("encode", "Kontextgerecht HTML-encodieren", ""), ("client", "Nur im Browser filtern", "Sicherheitsprüfungen im Browser können umgangen werden; die Ausgabe muss serverseitig sicher sein.")),
        Create("idor", "Broken Access Control / IDOR", "A01: Broken Access Control", "Mittel", "25 Min.", 150,
            "Du prüfst Zugriffe auf jedes einzelne Objekt serverseitig.",
            "Die historische Demo vertraut auf eine Objekt-ID aus der URL und ist intentionally vulnerable. Die sichere Variante vergleicht Eigentümer und aktuelle Identität auf dem Server.",
            "Wähle die Prüfung vor dem Lesen eines lokalen Profils.", "Lokale Beispieldaten einer anderen Person wären sichtbar.",
            "Nur die berechtigte Demobenutzerin erhält ihr eigenes Profil; andere IDs ergeben eine Ablehnung.", "https://owasp.org/Top10/A01_2021-Broken_Access_Control/", "Eine ID ist kein Berechtigungsnachweis.", "Prüfe Besitz oder Rolle auf dem Server, nicht im Link.",
            ("hidden", "ID in einem versteckten Feld speichern", "Versteckte Felder können von jedem Client verändert werden und sind keine Berechtigungsprüfung."), ("owner", "Aktuelle Identität gegen Objektbesitz prüfen", ""), ("route", "Objekt-ID nur anders benennen", "Eine anders benannte ID ändert nichts daran, dass der Server den Besitz prüfen muss.")),
        Create("authentication", "Unsichere Authentifizierung", "A07: Identification and Authentication Failures", "Mittel", "30 Min.", 150,
            "Du kombinierst Passwort-Hashing, Session-Prüfung und Rate Limits.",
            "Die historische Login-Logik ist intentionally vulnerable: sie vergleicht einen Demo-String direkt. Die sichere Variante verwendet einen Passwort-Hasher, serverseitige Session-Prüfung und Limits für Fehlversuche.",
            "Wähle das vollständige lokale Schutzpaket.", "Konten könnten durch triviale Prüfungen oder viele Versuche gefährdet sein.",
            "Passwörter werden nie im Klartext gespeichert, Sitzungen werden serverseitig geprüft und Fehlversuche begrenzt.", "https://owasp.org/Top10/A07_2021-Identification_and_Authentication_Failures/", "Ein Hash ist kein Verschlüsselungsschlüssel.", "Rate Limits ergänzen, aber ersetzen keine sichere Passwortspeicherung.",
            ("plain", "Passwort direkt vergleichen und im Speicher behalten", "Klartextpasswörter und direkte Vergleiche gefährden Konten bei Leaks; ausserdem fehlt der Schutz gegen viele Versuche."), ("secure-auth", "Hashing, serverseitige Session-Prüfung und Rate Limit", ""), ("captcha", "Nur ein Captcha hinzufügen", "Ein Captcha allein schützt weder gespeicherte Passwörter noch Sitzungen und ist kein vollständiges Authentifizierungskonzept.")),
        Create("file-upload", "Unsicherer Datei-Upload", "A04: Insecure Design", "Mittel", "25 Min.", 150,
            "Du validierst Typ, Grösse und Dateiname und speicherst ausserhalb statischer Inhalte.",
            "Die historische Upload-Variante ist intentionally vulnerable, weil sie Namen und Pfad direkt übernimmt. Die sichere Variante vergibt einen serverseitigen Namen und speichert nur erlaubte, kleine Dateien ausserhalb des Webroots.",
            "Wähle die sichere lokale Upload-Regel.", "Unerwartete Dateien könnten zugänglich werden oder Speicher erschöpfen.",
            "Nur erlaubte, begrenzte Dateien werden unter einem serverseitigen Namen ausserhalb des Webroots gespeichert; sie werden nie ausgeführt.", "https://owasp.org/www-community/vulnerabilities/Unrestricted_File_Upload", "Dateiendungen allein beweisen keinen Inhaltstyp.", "Verwende eine Allowlist, Grössenlimit und einen generierten Namen.",
            ("extension", "Nur die Dateiendung prüfen und im Webroot speichern", "Dateiendungen sind leicht manipulierbar; Dateien im Webroot können ausserdem direkt erreichbar oder ausführbar sein."), ("upload-policy", "Allowlist, Grössenlimit, generierter Name, Speicher ausserhalb Webroot", ""), ("rename", "Die hochgeladene Datei nur umbenennen", "Ein neuer Name ersetzt weder Inhaltsprüfung noch Grössenlimit und verhindert keinen riskanten Dateityp.")),
        Create("ssrf", "SSRF-Grundlagen", "A10: Server-Side Request Forgery", "Mittel", "20 Min.", 150,
            "Du begrenzt serverseitige Abrufe auf explizite lokale Mock-Ziele.",
            "Die historische Abrufidee ist intentionally vulnerable, weil sie eine beliebige Adresse akzeptiert. OWASP Forge ruft nie eine vom Client angegebene URL auf: die sichere Demo arbeitet nur mit einer festen lokalen Mock-Kennung.",
            "Wähle die sichere Abrufregel.", "Ein Server könnte sonst interne oder externe Ziele im falschen Kontext anfragen.",
            "Nur eine registrierte Mock-Ressource wird verarbeitet; beliebige URLs werden abgelehnt und Container haben kein Netzwerk.", "https://owasp.org/Top10/A10_2021-Server-Side_Request_Forgery_%28SSRF%29/", "Eine Blocklist kann unbekannte Ziele übersehen.", "Nimm eine feste Allowlist von Namen, nicht URLs vom Client.",
            ("blocklist", "Einige Hostnamen auf einer Blocklist ablehnen", "Blocklists übersehen unbekannte Schreibweisen, alternative IP-Darstellungen und neue interne Ziele."), ("allowlist", "Nur feste lokale Mock-Kennungen aus einer Allowlist akzeptieren", ""), ("redirect", "Weiterleitungen erst nach dem Abruf prüfen", "Wenn der erste Abruf bereits frei wählbar ist, kommt die Prüfung zu spät."))
    ];

    public IReadOnlyList<ChallengeDefinition> All => Definitions;

    public ChallengeDefinition? Find(string id) => Definitions.SingleOrDefault(challenge => StringComparer.Ordinal.Equals(challenge.Id, id));

    private static ChallengeDefinition Create(string id, string title, string category, string difficulty, string duration, int points, string goal, string explanation, string task, string impact, string expected, string url, string hintOne, string hintTwo, params (string Value, string Label, string Feedback)[] options) =>
        new(id, title, category, difficulty, duration, points, goal, explanation, task, impact, expected, url, [hintOne, hintTwo], options.Select(option => new ChallengeOption(option.Value, option.Label, option.Feedback)).ToArray(), options[1].Value)
        {
            Questions = QuestionPrompts(id, task).Select(prompt => new ChallengeQuestion(prompt)).ToArray()
        };

    private static IReadOnlyList<string> QuestionPrompts(string id, string task) => id switch
    {
        "sql-injection" => [task, "Welche Trennung verhindert, dass Eingaben als SQL-Code interpretiert werden?", "Was muss der Server bei jeder Suchanfrage unabhängig vom Browser erzwingen?"],
        "xss" => [task, "Wie wird ein lokaler Kommentar sicher in einer HTML-Seite dargestellt?", "Welche Ausgabeentscheidung verhindert, dass Markup als Code ausgeführt wird?"],
        "idor" => [task, "Welche Prüfung entscheidet, ob das angeforderte Profil gelesen werden darf?", "Warum reicht eine Objekt-ID aus der URL niemals als Berechtigungsnachweis?"],
        "authentication" => [task, "Welche Kombination schützt ein lokales Konto gegen Passwort- und Sessionmissbrauch?", "Welche Kontrolle begrenzt wiederholte fehlgeschlagene Anmeldeversuche?"],
        "file-upload" => [task, "Welche Regeln gelten vor dem Speichern einer lokalen Datei?", "Wo sollte eine nicht ausführbare Upload-Datei sicher abgelegt werden?"],
        "ssrf" => [task, "Wie begrenzt der Server Abrufe auf die vorgesehene Mock-Umgebung?", "Warum ist eine feste Kennungs-Allowlist sicherer als eine URL vom Client?"],
        _ => [task]
    };
}
