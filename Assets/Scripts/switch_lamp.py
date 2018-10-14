import RPi.GPIO as GPIO

GPIOPin = 7

GPIO.setwarnings(False)
GPIO.setmode(GPIO.BCM)
GPIO.setup(GPIOPin, GPIO.IN)
statePin = GPIO.input(GPIOPin)

GPIO.setup(GPIOPin, GPIO.OUT)
if (statePin is 1):
    GPIO.output(GPIOPin, False)
    print('Switched on')
else:
    GPIO.output(GPIOPin, True)
    print('Switched off')