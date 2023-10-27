import speech_recognition as sr
import socket
import requests

# Verbindung zum Race Car herstellen
race_car_ip = '192.168.0.100'  # IP-Adresse des Race Car
race_car_port = 5000  # Portnummer des Race Car
race_car_sock = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
race_car_sock.connect((race_car_ip, race_car_port))


# Umwandlung von Audio in Text
def audio_to_text():
    recognizer = sr.Recognizer()
    with sr.Microphone() as source:
        audio_data = recognizer.listen(source)
        text = recognizer.recognize_google(audio_data)
    return text

# URL des Flask-Servers des Race Cars
race_car_url = 'http://192.168.0.100:5000'

# Empfangen von Sprachbefehlen
while True:
    # Audio in Text
    audio_text = audio_to_text()

    # Text an das Race Car senden
    race_car_sock.sendall(audio_text.encode())