import speechpy
from scipy.io import wavfile
from keras.models import Sequential
from keras.layers import Dense, Conv2D, Flatten
from sklearn.model_selection import train_test_split

# Laden Sie die Audio-Datei
sample_rate, signal = wavfile.read('audio.wav')

# Extrahieren Sie die MFCCs
mfcc = speechpy.feature.mfcc(signal, sample_rate)

# Laden Sie die Trainingsdaten
X_train = load_training_data()  # Ersetzen Sie dies durch Ihre eigene Funktion
y_train = load_training_labels()  # Ersetzen Sie dies durch Ihre eigene Funktion

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
label = model.predict(mfcc)
