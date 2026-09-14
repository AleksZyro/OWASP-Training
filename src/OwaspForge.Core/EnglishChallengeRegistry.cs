namespace OwaspForge.Core;

/// <summary>English learning content with the same stable challenge identifiers and rules.</summary>
public sealed class EnglishChallengeRegistry
{
    private static readonly IReadOnlyList<ChallengeDefinition> Definitions = new ChallengeRegistry().All.Select(Translate).ToArray();

    public IReadOnlyList<ChallengeDefinition> All => Definitions;

    public ChallengeDefinition? Find(string id) => Definitions.SingleOrDefault(item => StringComparer.Ordinal.Equals(item.Id, id));

    private static ChallengeDefinition Translate(ChallengeDefinition source)
    {
        var copy = source with
        {
            Title = Title(source.Id),
            Difficulty = source.Difficulty switch { "Einfach" => "Easy", "Mittel" => "Medium", _ => source.Difficulty },
            Duration = source.Duration.Replace("Min.", "min", StringComparison.Ordinal),
            LearningGoal = Goal(source.Id),
            Explanation = "The historical example is intentionally vulnerable for local learning only. The secure approach is evaluated on the server and never targets external systems.",
            Task = "Choose the server-side measure that applies the protection rule.",
            Impact = "Without this control, local sample data or the learning flow could be exposed to an avoidable risk.",
            ExpectedAfterFix = "The local sandbox accepts only the secure server-side behaviour described in this station.",
            Hints = ["Focus on a server-side control, not a client-side promise.", "Prefer allowlists, minimal permissions and safe defaults."],
            Course = source.Course switch { "Eingabe und Ausgabe" => "Input and Output", "Identität und Zugriffe" => "Identity and Access", "Sichere Architektur" => "Secure Architecture", _ => "Operations and Supply Chain" },
            SolutionExplanation = Solution(source.Id),
            Options = source.Options.Select(TranslateOption).ToArray(),
            Questions = source.Questions.Select((question, index) => new ChallengeQuestion(Prompt(index), question.Options.Select(TranslateOption).ToArray(), question.CorrectOption)).ToArray()
        };

        return copy;
    }

    private static ChallengeOption TranslateOption(ChallengeOption option) => new(option.Value, OptionLabel(option.Value), string.IsNullOrEmpty(option.Feedback) ? string.Empty : "This option does not reliably protect the station.");

    private static string Prompt(int index) => index switch
    {
        0 => "Choose the secure action for this local station.",
        1 => "Which additional control belongs to this protection?",
        2 => "Which decision must the server enforce?",
        3 => "How is this rule applied consistently?",
        4 => "Which measure strengthens the protection further?",
        _ => "How would you verify this protection rule?"
    };

    private static string Title(string id) => id switch
    {
        "sql-injection" => "SQL Injection",
        "xss" => "Cross-Site Scripting",
        "file-upload" => "Insecure File Upload",
        "authentication" => "Insecure Authentication",
        "session-csrf" => "Sessions and CSRF",
        "idor" => "Broken Access Control / IDOR",
        "ssrf" => "SSRF Basics",
        "cryptography-secrets" => "Cryptography and Secrets",
        "security-misconfiguration" => "Security Misconfiguration",
        "dependencies-sbom" => "Dependencies and SBOM",
        "supply-chain-integrity" => "Supply Chain Integrity",
        "logging-monitoring" => "Logging and Monitoring",
        _ => id
    };

    private static string Goal(string id) => id switch
    {
        "sql-injection" => "Learn how parameterized queries keep values separate from SQL code.",
        "xss" => "Learn how context-aware encoding keeps untrusted text from executing.",
        "file-upload" => "Learn to validate uploads and keep them outside the web root.",
        "authentication" => "Learn to combine password hashing, sessions and rate limiting.",
        "session-csrf" => "Learn to protect authenticated actions with secure sessions and anti-forgery checks.",
        "idor" => "Learn to authorize every object access on the server.",
        "ssrf" => "Learn to restrict server-side requests to fixed local mock targets.",
        "cryptography-secrets" => "Learn to protect local secrets and use established cryptography.",
        "security-misconfiguration" => "Learn secure defaults for errors, headers and local service binding.",
        "dependencies-sbom" => "Learn to inventory, assess and maintain project dependencies.",
        "supply-chain-integrity" => "Learn to verify artifact origin and protect release controls.",
        "logging-monitoring" => "Learn to record security events without logging secrets.",
        _ => "Learn a secure server-side control."
    };

    private static string Solution(string id) => id switch
    {
        "sql-injection" => "Parameters keep query structure separate from values; minimal database permissions and tests add defence in depth.",
        "xss" => "Context-aware output encoding is the primary defence. A Content Security Policy is a useful additional layer.",
        "file-upload" => "Allowlisted content, limits, generated names and storage outside the web root keep uploads non-executable.",
        "authentication" => "Password hashing, secure server sessions, renewal and rate limits protect different stages of sign-in.",
        "session-csrf" => "Secure cookie attributes, server-side session invalidation and anti-forgery validation protect state changes together.",
        "idor" => "The server compares identity, role and ownership for every object access; an object ID is never authorization.",
        "ssrf" => "A fixed mock allowlist and disabled container networking prevent user input from choosing a network path.",
        "cryptography-secrets" => "Keep keys outside source control, hash passwords and use established cryptographic libraries and defaults.",
        "security-misconfiguration" => "Safe defaults include neutral error pages, central security headers, localhost-only binding and reviewed configuration.",
        "dependencies-sbom" => "An inventory makes components visible; assess advisories, test changes and remove unused dependencies.",
        "supply-chain-integrity" => "Verifiable origin, integrity checks and controlled releases protect the build and delivery chain.",
        "logging-monitoring" => "Structured protected logs support investigation without exposing passwords, tokens or other secrets.",
        _ => "The protection rule is validated on the server and stored locally."
    };

    private static string OptionLabel(string value) => value switch
    {
        "parameter" => "Use bound parameters and type-safe values",
        "concat" => "Build code with string concatenation",
        "filter" => "Rely only on a blocklist",
        "encode" => "Encode output for its context",
        "raw" => "Render user input as raw HTML",
        "client" => "Rely only on browser-side filtering",
        "upload-policy" => "Apply the complete server-side upload policy",
        "extension" => "Trust the file extension or public web root",
        "rename" => "Only rename the file",
        "secure-auth" => "Use the complete server-side authentication control",
        "plain" => "Compare or retain a password directly",
        "captcha" => "Add only a CAPTCHA",
        "csrf-session" => "Validate anti-forgery and secure session controls",
        "client-token" => "Validate the token only in the browser",
        "referer" => "Trust one request header alone",
        "owner" => "Authorize ownership or role on the server",
        "hidden" => "Trust a hidden field",
        "route" => "Only rename or obscure the route",
        "allowlist" => "Use a fixed server-side allowlist",
        "blocklist" => "Block only known targets",
        "redirect" => "Validate only after the request",
        "secret-store" => "Use established cryptography and separate key storage",
        "hardcode" => "Keep the secret in source code",
        "custom" => "Invent a custom cryptographic approach",
        "secure-default" => "Use reviewed secure defaults",
        "debug" => "Expose debug details",
        "hide" => "Hide only the browser message",
        "inventory" => "Inventory, assess and test components",
        "ignore" => "Leave components unreviewed",
        "latest" => "Update everything without testing",
        "verify" => "Verify origin and integrity before use",
        "trust" => "Trust an artifact by name alone",
        "after" => "Verify only after use",
        "structured" => "Log relevant events without secrets",
        "secrets" => "Log passwords or tokens for diagnosis",
        "none" => "Do not record security events",
        _ => "Use the secure server-side control"
    };
}
