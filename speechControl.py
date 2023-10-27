"""import speechpy
from scipy.io import wavfile
from keras.models import Sequential
from keras.layers import Dense, Conv2D, Flatten
from sklearn.model_selection import train_test_split
"""

import speech_recognition as sr
import socket

# Verbindung zum Race Car herstellen
race_car_ip = '192.168.0.100'  # IP-Adresse des Race Car
race_car_port = 5000  # Portnummer des Race Car
race_car_sock = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
race_car_sock.connect((race_car_ip, race_car_port))


# Funktion zur Umwandlung von Audio in Text
def audio_to_text():
    recognizer = sr.Recognizer()
    with sr.Microphone() as source:
        audio_data = recognizer.listen(source)
        text = recognizer.recognize_google(audio_data)
    return text


# Schleife zum kontinuierlichen Empfangen von Sprachbefehlen
while True:
    # Audio in Text umwandeln
    audio_text = audio_to_text()

    # Text an das Race Car senden
    race_car_sock.sendall(audio_text.encode())

"""
# Funktion zur Umwandlung von Audio in Text
def audio_to_text(filename):
    recognizer = sr.Recognizer()
    with sr.AudioFile(filename) as source:
        audio_data = recognizer.record(source)
        text = recognizer.recognize_google(audio_data)
    return text


# Laden Sie die Audio-Datei
sample_rate, signal = wavfile.read('audio.wav')

# Extrahieren Sie die MFCCs
mfcc = speechpy.feature.mfcc(signal, sample_rate)

# Laden Sie die Trainingsdaten
# X_train = load_training_data()  # Ersetzen Sie dies durch Ihre eigene Funktion
# y_train = load_training_labels()  # Ersetzen Sie dies durch Ihre eigene Funktion

# Teilen Sie die Daten in Trainings- und Validierungssätze auf
X_train, X_val, y_train, y_val = train_test_split(
    X_train, y_train, test_size=0.2)

# Erstellen Sie das CNN-Modell
model = Sequential()
model.add(Conv2D(32, kernel_size=(3, 3), activation='relu',
                 input_shape=(mfcc.shape[0], mfcc.shape[1], 1)))
model.add(Conv2D(64, kernel_size=(3, 3), activation='relu'))
model.add(Flatten())
model.add(Dense(128, activation='relu'))
model.add(Dense(3, activation='softmax'))

# Kompilieren Sie das Modell
model.compile(loss='categorical_crossentropy',
              optimizer='adam', metrics=['accuracy'])

# Trainieren Sie das Modell
model.fit(X_train, y_train, validation_data=(X_val, y_val), epochs=10)

# Laden Sie die Test-Audio-Datei
sample_rate, signal = wavfile.read('test_audio.wav')

# Extrahieren Sie die MFCCs aus der Test-Audio-Datei
mfcc = speechpy.feature.mfcc(signal, sample_rate)

# Verwenden Sie das trainierte Modell, um das Label vorherzusagen
label = model.predict(mfcc) """
