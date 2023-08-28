#include <HX711_ADC.h>
#if defined(ESP8266)|| defined(ESP32) || defined(AVR)
#include <EEPROM.h>
#endif
#include <ESP8266WiFi.h>
#include <ESP8266HTTPClient.h>

#include "appsettings.h"

const int trig = 5;
// Replace with your WiFi credentials
const char* ssid = _ssid;
const char* password = _password;

const int HX711_sck = 2; //mcu > HX711 sck pin
const int HX711_dout = 4; //mcu > HX711 dout pin
HX711_ADC LoadCell(HX711_dout, HX711_sck);

bool ignition = false;
unsigned long t = 0;
double reading, c = 0;
const int calVal_eepromAdress = 0;

void updateServer(String url){
  WiFiClient client;
  HTTPClient httpclient;
  httpclient.addHeader("x-api-key",String(_key));
  httpclient.begin(client, url);
  // Start the HTTP request with WiFi client and URL
  int httpResponseCode = httpclient.GET(); // Send the GET request
  httpclient.end(); 
}

void setup() {
  pinMode(trig,OUTPUT);
  Serial.begin(115200); 
  delay(10);
  Serial.println();
  Serial.println("Starting...");
  LoadCell.begin();
  //LoadCell.setReverseOutput(); //uncomment to turn a negative output value to positive
  float calibrationValue; // calibration value (see example file "Calibration.ino")
  calibrationValue = 44.50; // uncomment this if you want to set the calibration value in the sketch
  #if defined(ESP8266)|| defined(ESP32)
  //EEPROM.begin(512); // uncomment this if you use ESP8266/ESP32 and want to fetch the calibration value from eeprom
  #endif
  //EEPROM.get(calVal_eepromAdress, calibrationValue); // uncomment this if you want to fetch the calibration value from eeprom
  unsigned long stabilizingtime = 2000; // preciscion right after power-up can be improved by adding a few seconds of stabilizing time
  boolean _tare = true; //set this to false if you don't want tare to be performed in the next step
  LoadCell.start(stabilizingtime, _tare);
  if (LoadCell.getTareTimeoutFlag()) {
    Serial.println("Timeout, check MCU - HX711 wiring and pin designations");
    while (1);
  }
  else {
    LoadCell.setCalFactor(calibrationValue); // set calibration value (float)
    Serial.println("HX711 startup is complete");
  }
  // Connect to WiFi
  WiFi.begin(ssid, password);
  Serial.print("Connecting to WiFi");
  while (WiFi.status() != WL_CONNECTED) {
    delay(500);
    Serial.print(".");
  }
  Serial.println("\nConnected to WiFi");
}

void loop() {
  
  static boolean newDataReady = 0;
  const int serialPrintInterval = 500; //increase value to slow down serial print activity

  // check for new data/start next conversion:
  if (LoadCell.update()) newDataReady = true;

  // get smoothed value from the dataset:
  if (newDataReady) {
    if (millis() > t + serialPrintInterval) {
       reading = LoadCell.getData();
      Serial.print("Load_cell output val: ");
      Serial.println(reading);
      newDataReady = 0;
      t = millis();
    }
  }
  
  // Set up WiFi client and HTTPClient
  WiFiClient client;
  HTTPClient httpclient;
  httpclient.addHeader("x-api-key",String(_key));
  
  if(reading < 0) reading = 0.0; // avoid negative readings!

  String url;
  if(ignition == false){
    url = String(_endpoint1);
  }
  else{
    url =  String(_endpoint2) + String(reading);
  }

  // Start the HTTP request with WiFi client and URL
  httpclient.begin(client, url);

  int httpResponseCode = httpclient.GET(); // Send the GET request

  if (httpResponseCode > 0) {
    Serial.print("HTTP Response code: ");
    Serial.println(httpResponseCode);
    
    String payload = httpclient.getString(); // Get the response payload
    Serial.println("Response payload:");
    Serial.println(payload);

    if(ignition == false && payload == "true"){
      ignition = true;
      digitalWrite(trig,HIGH);
    }

    if(ignition == true){
      c += 1;
    }
    if(c >= 1000 && reading < 40){
      ignition = false;
      digitalWrite(trig,LOW);
      c = 0;
      updateServer(String(_endpoint3));
    }
    
  } else {
    Serial.print("Error in HTTP request. HTTP Response code: ");
    Serial.println(httpResponseCode);
  }

  httpclient.end(); // Close the connection

//  delay(1000); // Delay for 5 seconds before making the next request
}
