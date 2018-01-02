load('api_timer.js');
load('api_events.js');
load('api_arduino_bme280.js');
load('api_config.js');
load('api_mqtt.js');
load('api_net.js');
load('api_sys.js');
load('api_gpio.js');

let led = Cfg.get('pins.led');
let button = Cfg.get('pins.button');
let deviceName = Cfg.get('device.id');
let topic = '/devices/' + deviceName + '/events';
print('Topic: ', topic);

let isConnected = false;


let sens_addr = 0x76;
let bme = Adafruit_BME280.createI2C(sens_addr);
if (bme.begin(sens_addr) === 0) {
  print('Cant find a sensor');
} else {
  print('Sensor Found');
}

let getInfo = function() {
  return JSON.stringify({
    free_ram: Sys.free_ram() / 1024,
    temp: bme.readTemperature(),
    hum: bme.readHumidity(),
    press: bme.readPressure()
  });
};

// Blink built-in LED every second
GPIO.set_mode(led, GPIO.MODE_OUTPUT);
Timer.set(1000 /* 1 sec */, Timer.REPEAT, function() {
  let value = GPIO.toggle(led);
  //print(getInfo());
}, null);

Timer.set(5000 /* 5 sec */, Timer.REPEAT, function() {
  let message = getInfo();
  let ok = MQTT.pub(topic, message, 1);
}, null);

MQTT.sub('/devices/' + Cfg.get('device.id')+ '/config', function(conn, topic, msg) {
  print('Topic:', topic, 'message:', msg);
}, null);

// Monitor network connectivity.
Event.addGroupHandler(Net.EVENT_GRP, function(ev, evdata, arg) {
  let evs = '???';
  if (ev === Net.STATUS_DISCONNECTED) {
    evs = 'DISCONNECTED';
  } else if (ev === Net.STATUS_CONNECTING) {
    evs = 'CONNECTING';
  } else if (ev === Net.STATUS_CONNECTED) {
    evs = 'CONNECTED';
  } else if (ev === Net.STATUS_GOT_IP) {
    evs = 'GOT_IP';
  }
  print('== Net event:', ev, evs);
}, null);

