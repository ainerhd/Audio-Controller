// Anzahl der analogen Eing�nge (einstellbar)
const int numInputs = 5;  // 1 bis n Eing�nge, hier Beispiel: 5

// Array mit den analogen Pins
const int inputPins[] = {A0, A1, A2, A3, A4};  // Entsprechend der Anzahl anpassen

void setup() {
  Serial.begin(9600); // Serielle Kommunikation starten
  for (int i = 0; i < numInputs; i++) {
    pinMode(inputPins[i], INPUT); // Pins als Eing�nge definieren
  }
}

void loop() {
  // Pr�fen, ob eine serielle Nachricht eingegangen ist
  if (Serial.available() > 0) {
    String command = Serial.readStringUntil('\n'); // Nachricht lesen
    command.trim(); // F�hrende und nachfolgende Leerzeichen entfernen

    // Auf spezifische Nachricht reagieren
    if (command == "HELLO_ARDUINO") {
      Serial.println("ARDUINO_READY"); // Antwort senden
      return; // Zur�ckkehren, um analoge Ausgabe f�r diese Iteration zu �berspringen
    }
  }

  // Analoge Eing�nge auslesen und senden
  for (int i = 0; i < numInputs; i++) {
    int sensorValue = analogRead(inputPins[i]); // Wert vom aktuellen Pin lesen
    Serial.print(sensorValue);                 // Wert senden
    if (i < numInputs - 1) {
      Serial.print(" | "); // Trennzeichen zwischen Werten
    }
  }
  Serial.println(); // Zeilenumbruch am Ende der Ausgabe
  delay(100);       // Kleine Pause, um nicht zu h�ufig zu senden
}
