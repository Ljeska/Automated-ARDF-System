const int pirPin = 2; // Pin za PIR senzor
const int vPin = 11; // Pin za LED RGB
const int redPin = 12; // Pin za crvenu diodu
const int bluePin = 13; // Pin za plavu diodu
const int buzzerPin = 6; // Pin za buzzer
const int buzzerGnd = 7; // Pin za GND buzzera
bool buzzerActive = false; // Stanje buzzera
bool commandReceived = false; // Da li je primljena naredba
unsigned long buzzerStartTime = 0; // Vrijeme početka trajanja buzzera

void setup() {
  pinMode(pirPin, INPUT);
  pinMode(vPin, OUTPUT);
  pinMode(redPin, OUTPUT);
  pinMode(bluePin, OUTPUT);
  pinMode(buzzerPin, OUTPUT);
  pinMode(buzzerGnd, OUTPUT);
  digitalWrite(buzzerGnd, LOW);
  digitalWrite(redPin, LOW); // Isključimo crvenu diodu
  digitalWrite(bluePin, LOW); // Isključimo plavu diodu
  
  Serial.begin(9600);  // Pokretanje serijske komunikacije
}

void loop() {
  // Provjera da li je detektiran pokret i da li je primljena naredba
  if (digitalRead(pirPin) == HIGH && commandReceived) {
    // Ako je detektiran pokret, isključujemo zelenu diodu i uključujemo buzzer
    digitalWrite(vPin, LOW);
    tone(buzzerPin, 3500);
    buzzerActive = true;
    buzzerStartTime = millis(); // Postavljanje vremena početka trajanja buzzera
    commandReceived = false;

    // Slanje potvrde nazad skripti da je predajnik pronađen
    Serial.println("Predajnik A pronadjen");
  } else {
    // Ako nije detektiran pokret, provjeravamo naredbu sa računara
    if (Serial.available() > 0) {
      char receivedChar = Serial.read();
      
      if (receivedChar == 'A') {
        // Uključujemo zelenu diodu i isključujemo buzzer
        digitalWrite(vPin, HIGH);
        noTone(buzzerPin);
        buzzerActive = true;
        commandReceived = true;
      } else if (receivedChar == 'X') {
        // Ako je primljena naredba 'X', isključujemo zelenu diodu i buzzer
        digitalWrite(vPin, LOW);
        noTone(buzzerPin);
        buzzerActive = false;
        commandReceived = false;
      }
    }
  }

  // Provjeravamo vrijeme proteklo od početka trajanja buzzera
  if (buzzerActive && millis() - buzzerStartTime >= 3000) {
    // Ako je prošlo više od 3 sekunde, isključujemo buzzer
    noTone(buzzerPin);
    buzzerActive = false;
  }
}
