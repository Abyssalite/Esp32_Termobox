#include <U8g2lib.h>
#include <DHT22.h>      
#include <string>
#include <WiFi.h>
#include <ESPAsyncWebServer.h>
#include <AsyncTCP.h>
#include <ArduinoJson.h>

#define TR0 10000   // Ω
#define R   10000
#define B   3970  
#define VCC 3.3    //Supply  voltage
#define THERMISTOR A0

#define UPDATE_PERIOD 1000
#define FUNCTION_PERIOD 10
#define RECONNECT_PERIOD 1000
#define NOTIFY_PERIOD 1000

#define BUTTON1 9
#define BUTTON2 10
#define BUTTON3 2
#define BUTTON4 21
#define FAN1 4
#define FAN2 3
#define TEC  7
#define FAN_FREQ 8000
#define TEC_FREQ 10000
#define PWM_RESOLUTION 10
#define DEBOUND_BUTTON 120 // 120ms debounce
#define DEBOUND_INTERRUPT 100 // 100ms debounce

char* ssid = "Devices";
char* password = "0948844329";

AsyncWebServer server(80);
AsyncWebSocket ws("/ws");
JsonDocument telemetryJson;

// OLED constructor
U8G2_SSD1306_72X40_ER_F_HW_I2C u8g2(U8G2_R0, U8X8_PIN_NONE, 6, 5);

DHT22 dht22(20);
unsigned long updateTimer = 0;
unsigned long functionTimer = 0;
unsigned long reconnectTimer = 0;
unsigned long lastNotify = 0;

bool connected = false;
int count = 0;
char* status = "";

uint8_t fan1Speed = 20;
uint8_t fan2Speed = 20;
uint8_t tecPower = 10;
float setTemp = 1.0f;
float currentTemp = 0.0f;
float currentHumidity = 0.0f;
float thermTemp = 0.0f;
const float T0 = 30 + 273.15; 

volatile bool canSetButton = true;
volatile bool canSetInterrupt = true;
static unsigned long buttonPress = 0;
static unsigned long interruptPress = 0;

volatile uint8_t mode[2] = {0};
volatile uint8_t modeIndex = 0;

void notifyClients() {
  telemetryJson["fan1Speed"] = fan1Speed;
  telemetryJson["fan2Speed"] = fan2Speed;
  telemetryJson["tecPower"] = tecPower;
  telemetryJson["setTemp"] = setTemp;
  telemetryJson["currentTemp"] = currentTemp;
  telemetryJson["currentHumidity"] = currentHumidity;
  telemetryJson["thermTemp"] = thermTemp;
  telemetryJson["mode"] = mode[modeIndex];
  telemetryJson["modeIndex"] = modeIndex;

  String jsonString;
  serializeJson(telemetryJson, jsonString);
  ws.textAll(jsonString);
}

void onWsEvent(AsyncWebSocket *server, AsyncWebSocketClient *client, AwsEventType type, void *arg, uint8_t *data, size_t len) {
  if (type == WS_EVT_CONNECT) 
    status = "WS connected";
  
  else if (type == WS_EVT_DISCONNECT) 
    status = "WS disconnect";

  else if (type == WS_EVT_DATA) {
    // Handle commands from Avalonia app
    String message = String((char*)data).substring(0, len);

    /*if (message.startsWith("SET_TEMP:")) {
      setTemp = message.substring(9).toInt();
      // constrain if needed
    } 
    else if (message.startsWith("SET_FAN:")) {
      fan1Speed = message.substring(8).toInt();
    }
    */
  }
}

bool debounceInterrupt(uint8_t btn, uint8_t isHigh) {
  if (isHigh) return false;
  if (canSetInterrupt) {
    interruptPress = millis();
    canSetInterrupt = false;
  }
  if (millis() - interruptPress > DEBOUND_INTERRUPT) {
    canSetInterrupt = true;
    interruptPress = 0;
    return true;
  }
  return false;
}

bool debounceButtons(uint8_t btn, uint8_t isHigh) {
  if (isHigh) return false;
  if (canSetButton) {
    buttonPress = millis();
    canSetButton = false;
  }
  if (millis() - buttonPress > DEBOUND_BUTTON) {
    canSetButton = true;
    buttonPress = 0;
    return true;
  }
  return false;
}

void readThermisor() {
  int raw = analogRead(THERMISTOR);

  if (raw <= 0 || raw >= 4095) {
    thermTemp = 99;
    return;
  }

  float thermVoltage = (VCC / 4095.0) * raw;
  float thermResistance = R * (thermVoltage / (VCC - thermVoltage));

  thermTemp = 1.0 / ((1.0 / T0) + (log(thermResistance / TR0) / B));
  thermTemp = thermTemp - 273.15;
}

void tecPowerController() {
  float dt = currentTemp - setTemp; 
   if (dt < 0) return;

  tecPower = 10 + 80.0 / (1 + exp(-0.6 * (dt - 9))); 
  tecPower = constrain(tecPower, 10, 90);
}


void showTemp() {
  u8g2.clearBuffer();

  u8g2.setFont(u8g2_font_5x8_tf);
  u8g2.setCursor(4, 8);
  u8g2.print("Co, Ho / Hum");

  u8g2.setFont(u8g2_font_7x14_tf);
  u8g2.setCursor(4, 22);
  u8g2.print(currentTemp, 1);
  u8g2.print(",");
  u8g2.print(thermTemp, 1);

  u8g2.setCursor(4, 38);
  u8g2.print(currentHumidity, 1);
  u8g2.print(" %RH");

  u8g2.sendBuffer();
}

void showTempInfo(std::string text, uint8_t num, float data, float data2 = 0) {
    u8g2.clearBuffer();
    u8g2.setFont(u8g2_font_5x8_tf);
    u8g2.setCursor(4, 8);
    u8g2.print(text.c_str());   
    u8g2.print(' ');
    u8g2.print(num);

    u8g2.setFont(u8g2_font_7x14_tf);
    u8g2.setCursor(4, 22);
    u8g2.print(data, 1);
    if (data2) {
      u8g2.print('/');
      u8g2.print(data2, 1);
    }
    u8g2.sendBuffer();
}

void showFanInfo(std::string text, uint8_t num, uint8_t data) {
    u8g2.clearBuffer();
    u8g2.setFont(u8g2_font_5x8_tf);
    u8g2.setCursor(4, 8);
    u8g2.print(text.c_str());   
    u8g2.print(' ');
    u8g2.print(num);

    u8g2.setFont(u8g2_font_7x14_tf);
    u8g2.setCursor(4, 22);
    u8g2.print(data);
    u8g2.sendBuffer();
}

void showError(int err) {
  u8g2.clearBuffer();
  u8g2.setFont(u8g2_font_6x10_tf);
  u8g2.setCursor(4, 16);
  u8g2.print("Sensor Error");
  u8g2.setCursor(4, 30);
  u8g2.print(err);
  u8g2.sendBuffer();
}

void IRAM_ATTR button1ISR() {
  if (debounceInterrupt(BUTTON1, digitalRead(BUTTON1))) {
      switch (mode[modeIndex]) {
      case 0: {
        mode[modeIndex] = 1;
        break;
      }
      case 1: {
        mode[modeIndex] = 2;
        break;
      }    
      case 2: {
        mode[modeIndex] = 3;
        break;
      }
      case 3: {
        mode[modeIndex] = 4;
        break;
      }
      case 4: {
        mode[modeIndex] = 5;
        break;
      }
      case 5: {
        mode[modeIndex] = 0;
        break;
      }
    }
  } 
}

void IRAM_ATTR button2ISR() {
  if (debounceInterrupt(BUTTON2, digitalRead(BUTTON2))) {
      modeIndex++;
      if (modeIndex > 1) modeIndex = 0;
  } 
}

void setFan(int8_t value, uint8_t fan, uint8_t* fanSpeed) {
    *fanSpeed += value;
    uint8_t tmp = constrain(*fanSpeed, 1, 100);
    *fanSpeed = tmp;
    ledcWrite(fan, map(tmp, 0, 100, 0, 1024));
}

void setup(void) {
  pinMode(BUTTON1, INPUT_PULLUP);
  pinMode(BUTTON2, INPUT_PULLUP);
  pinMode(BUTTON3, INPUT_PULLUP);
  pinMode(BUTTON4, INPUT_PULLUP);

  pinMode(FAN1, OUTPUT);
  pinMode(FAN2, OUTPUT);
  pinMode(FAN2, OUTPUT);

  attachInterrupt(digitalPinToInterrupt(BUTTON1), button1ISR, FALLING);
  attachInterrupt(digitalPinToInterrupt(BUTTON2), button2ISR, FALLING);

  ledcAttach(TEC, TEC_FREQ, PWM_RESOLUTION);
  ledcAttach(FAN1, FAN_FREQ, PWM_RESOLUTION);
  ledcAttach(FAN2, FAN_FREQ, PWM_RESOLUTION);

  WiFi.mode(WIFI_STA); 
  ws.onEvent(onWsEvent);
  server.addHandler(&ws);
  server.begin();

  u8g2.begin();
  u8g2.enableUTF8Print();

  u8g2.clearBuffer();
  u8g2.setFont(u8g2_font_6x10_tf);
  u8g2.setCursor(4, 12);
  u8g2.print("Starting...");
  u8g2.sendBuffer();
}

void loop(void) {
  unsigned long now = millis();

  if (now - functionTimer >= FUNCTION_PERIOD) {
    functionTimer = now;
    
    switch (mode[modeIndex]) {
      case 0: {
        if (modeIndex == 0) {
          showTemp();
        }
        
        if (modeIndex == 1) {
          u8g2.clearBuffer();
          u8g2.setFont(u8g2_font_6x10_tf);
          u8g2.setCursor(4, 16);
          u8g2.print("Setting");
          u8g2.sendBuffer();
        }

        break;
      }

      case 1: {
        if (modeIndex == 0) {
          showTempInfo("Crt. Temp", 1, currentTemp, setTemp);
        }

        if (modeIndex == 1) {
          if (debounceButtons(BUTTON3, digitalRead(BUTTON3))) {
            setTemp+=1;
          } 
          if (debounceButtons(BUTTON4, digitalRead(BUTTON4))) {
            setTemp-=1;
          } 
          setTemp = constrain(setTemp, -10, 30);
          showTempInfo("Set Temp", 1, setTemp);
        }
        
        break;
      }

      case 2: {
        if (modeIndex == 0) {
          showFanInfo("Crt. TEC Powr", 1, tecPower);
        }

        if (modeIndex == 1) {
          if (debounceButtons(BUTTON3, digitalRead(BUTTON3))) {
            setFan(1, TEC, &tecPower);
          } 
          if (debounceButtons(BUTTON4, digitalRead(BUTTON4))) {
            setFan(-1, TEC, &tecPower);
          } 
          showFanInfo("Set TEC Powr", 1, tecPower);
        }

        break;
      }    

      case 3: {
        if (modeIndex == 0) {
          showFanInfo("Crt. Fan 1 Spd", 1, fan1Speed);
        }

        if (modeIndex == 1) {
          if (debounceButtons(BUTTON3, digitalRead(BUTTON3))) {
            setFan(1, FAN1, &fan1Speed);
          } 
          if (debounceButtons(BUTTON4, digitalRead(BUTTON4))) {
            setFan(-1, FAN1, &fan1Speed);
          } 
          showFanInfo("Set Fan 1 Spd", 1, fan1Speed);
        }

        break;
      }    

      case 4: {
        if (modeIndex == 0) {
          showFanInfo("Crt. Fan 2 Spd", 2, fan2Speed);
        }

        if (modeIndex == 1) {
          if (debounceButtons(BUTTON3, digitalRead(BUTTON3))) {
            setFan(1, FAN2, &fan2Speed);
          } 
          if (debounceButtons(BUTTON4, digitalRead(BUTTON4))) {
            setFan(-1, FAN2, &fan2Speed);
          } 
          showFanInfo("Set Fan 2 Spd", 2, fan2Speed);
        }

        break;
      }

      case 5: {
        if (modeIndex == 0) {
          u8g2.clearBuffer();

          u8g2.setFont(u8g2_font_5x8_tf);
          u8g2.setCursor(4, 8);
          u8g2.print("Connected to");

          u8g2.setFont(u8g2_font_5x8_tf);
          u8g2.setCursor(4, 18);
          u8g2.print(WiFi.localIP());

          u8g2.setFont(u8g2_font_5x8_tf);
          u8g2.setCursor(4, 28);
          u8g2.print(status);

          u8g2.sendBuffer();
        }

        if (modeIndex == 1) {
          if (now - reconnectTimer >= RECONNECT_PERIOD) {
            reconnectTimer = now;

            if (!connected) {
              WiFi.begin(ssid, password);
              connected = !connected;
            }
            
            if (WiFi.status() != WL_CONNECTED) {
              u8g2.clearBuffer();
              u8g2.setFont(u8g2_font_6x10_tf);
              u8g2.setCursor(4, 16);
              u8g2.print("Connecting");

              u8g2.setFont(u8g2_font_6x10_tf);
              u8g2.setCursor(4, 26);
              char dot = (count % 2 == 0) ? '/' : '\\';
              u8g2.print(dot);

              u8g2.sendBuffer();  
              count++;
              return;
              
            } else {
              u8g2.clearBuffer();
              u8g2.setFont(u8g2_font_6x10_tf);
              u8g2.setCursor(4, 16);
              u8g2.print("Connected");
              u8g2.sendBuffer();  
            }
          }
        }

        break;
      }
    }
  }

  if (now - updateTimer >= UPDATE_PERIOD) {
    updateTimer = now;
    uint8_t err = dht22.getLastError();

    currentTemp = (err != 0) ? 99 : dht22.getTemperature();
    currentHumidity = (err != 0) ? 99 :  dht22.getHumidity();

    readThermisor();
    //tecPowerController();
  }

  if (now - lastNotify >= NOTIFY_PERIOD) {
    notifyClients();
    lastNotify = now;
  }

  ws.cleanupClients();
}