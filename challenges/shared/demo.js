const challenge = document.body.dataset.challenge;
const input = document.querySelector("#demo-input");
const result = document.querySelector("#result");
const action = document.querySelector("#demo-action");

const show = (message, safe = true) => {
  result.textContent = message;
  result.className = safe ? "safe" : "";
};

action.addEventListener("click", () => {
  const value = input.value.trim();
  if (challenge === "sql") {
    show(value.toLowerCase() === "ada" ? "Lokaler Treffer: Ada (Beispieldatensatz). Die Eingabe blieb ein gebundener Wert." : "Kein lokaler Treffer. Diese Demo interpretiert Eingaben nie als Abfragecode.");
  } else if (challenge === "xss") {
    show(`Sichere Textausgabe: ${value || "(leer)"}. Markup wird als Text angezeigt.`);
  } else if (challenge === "idor") {
    show(value === "own-profile" ? "Zugriff erlaubt: Eigentümerschaft stimmt mit der Demo-Identität überein." : "Zugriff verweigert: Objektbesitz wurde serverseitig simuliert geprüft.");
  } else if (challenge === "auth") {
    show("Kein Login ausgeführt. Diese Demo akzeptiert oder speichert keine Zugangsdaten; sie zeigt nur den sicheren Ablauf mit Hashing, Session-Prüfung und Limit.");
  } else if (challenge === "upload") {
    show(value.endsWith(".txt") ? "Simulation: erlaubter Texttyp, begrenzte Grösse, generierter Speichername ausserhalb Webroot." : "Simulation: Datei abgelehnt. Nur die lokale Allowlist ist erlaubt; es wird nichts hochgeladen oder ausgeführt.");
  } else if (challenge === "ssrf") {
    show(value === "catalogue" ? "Lokale Mock-Antwort: catalogued-item. Es wurde kein Netzwerkzugriff ausgeführt." : "Abgelehnt: Diese Demo erlaubt nur die feste Mock-Kennung «catalogue», keine URLs.");
  }
});
