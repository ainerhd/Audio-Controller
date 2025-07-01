// Anzahl der analogen Eingänge (einstellbar)
const int numInputs = 5;  // 1 bis n Eingänge
const int inputPins[] = {A0, A1, A2, A3, A4};  // Entsprechend der Anzahl anpassen

unsigned long sendInterval = 100; // Zeit in Millisekunden zwischen den Sendungen
unsigned long lastSendTime = 0;

void setup() {
  Serial.begin(9600); // Serielle Verbindung starten
  for (int i = 0; i < numInputs; i++) {
    pinMode(inputPins[i], INPUT); // Pins als Eingänge definieren
  }
  delay(1000); // Sicherstellen, dass die Verbindung vollständig hergestellt ist
}

void loop() {
  // Prüfen, ob eine Nachricht empfangen wurde
  if (Serial.available() > 0) {
    handleSerialMessage();
  }

  unsigned long now = millis();
  if (now - lastSendTime >= sendInterval) {
    sendAnalogValues();
    lastSendTime = now;
  }
}

// Funktion zur Verarbeitung von seriellen Nachrichten
void handleSerialMessage() {
  String command = Serial.readStringUntil('\n'); // Nachricht lesen
  command.trim(); // Leerzeichen entfernen

  if (command == "HELLO_MIXER") {
    Serial.println("MIXER_READY"); // Antwort senden
  } else if (command.startsWith("RATE ")) {
    unsigned long rate = command.substring(5).toInt();
    if (rate > 0) {
      sendInterval = rate;
      Serial.print("RATE_SET ");
      Serial.println(sendInterval);
    }
  }
}

// Funktion zum Senden der analogen Werte
void sendAnalogValues() {
  for (int i = 0; i < numInputs; i++) {
    int sensorValue = analogRead(inputPins[i]); // Wert vom aktuellen Pin lesen
    Serial.print(sensorValue); // Wert senden
    if (i < numInputs - 1) {
      Serial.print(" | "); // Trennzeichen
    }
  }
  Serial.println(); // Zeilenumbruch am Ende der Ausgabe
}
