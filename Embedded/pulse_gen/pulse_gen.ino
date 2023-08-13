#define input 2
#define output 4
#define delay_ms 1000

void setup() {
  pinMode(input,INPUT);
  pinMode(output,OUTPUT);
}

void loop() {
  if(digitalRead(input)){
    digitalWrite(output,LOW);
    delay(delay_ms);
    digitalWrite(output,HIGH);
    delay(delay_ms);
  }
  else{
    digitalWrite(output,LOW);
  }
}
