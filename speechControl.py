import socket
import speech_recognition as sr
import requests
import re

# Verbindung zum Race Car herstellen
""" race_car_ip = '192.168.0.100'  # IP-Adresse des Race Car
race_car_port = 5000  # Portnummer des Race Car
race_car_sock = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
race_car_sock.connect((race_car_ip, race_car_port)) """


# Umwandlung von Audio in Text
def audio_to_text():
    recognizer = sr.Recognizer()
    with sr.Microphone() as source:
        audio_data = recognizer.listen(source)
        text = recognizer.recognize_google(audio_data)
    return text


# URL des Flask-Servers des Race Cars
race_car_url = 'http://10.42.0.1:5000'

# Empfangen von Sprachbefehlen
while True:
    # Audio in Text
    audio_text = audio_to_text()

    # Schlüsselwörter und reguläre Ausdrücke zum Extrahieren von Informationen
    forward_pattern = re.compile(r'move forward (\d+) meters')
    backward_pattern = re.compile(r'move backward (\d+) meters')
    left_pattern = re.compile(r'turn left (\d+) degrees')
    right_pattern = re.compile(r'turn right (\d+) degrees')

    # Sprachbefehle an den Flask-Server des Race Cars senden
    if forward_pattern.match(audio_text):
        distance = int(forward_pattern.match(audio_text).group(1))
        requests.post(race_car_url, data=f'forward {distance}')
    elif backward_pattern.match(audio_text):
        distance = int(backward_pattern.match(audio_text).group(1))
        requests.post(race_car_url, data=f'backward {distance}')
    elif left_pattern.match(audio_text):
        degrees = int(left_pattern.match(audio_text).group(1))
        requests.post(race_car_url, data=f'left {degrees}')
    elif right_pattern.match(audio_text):
        degrees = int(right_pattern.match(audio_text).group(1))
        requests.post(race_car_url, data=f'right {degrees}')
