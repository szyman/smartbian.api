import RPi.GPIO as GPIO

GPIOPin = 17

GPIO.setwarnings(False)
GPIO.setmode(GPIO.BCM)
GPIO.setup(GPIOPin, GPIO.OUT)
statePin = GPIO.input(GPIOPin)

if (statePin is 1):
    GPIO.output(GPIOPin, False)
    print('Switched off')
else:
    GPIO.output(GPIOPin, True)
    print('Switched on')