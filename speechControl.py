import speechpy
from scipy.io import wavfile
from sklearn import svm
from sklearn.model_selection import train_test_split

# Load the audio file
sample_rate, signal = wavfile.read('audio.wav')

# Extract the MFCCs
mfcc = speechpy.feature.mfcc(signal, sample_rate)

# Load the training data
X_train = load_training_data()  # Replace with your own function
y_train = load_training_labels()  # Replace with your own function

# Split the data into training and validation sets
X_train, X_val, y_train, y_val = train_test_split(
    X_train, y_train, test_size=0.2)

# Train the SVM model
clf = svm.SVC()
clf.fit(X_train, y_train)

# Evaluate the model on the validation set
accuracy = clf.score(X_val, y_val)

# Load the audio file
sample_rate, signal = wavfile.read('test_audio.wav')

# Extract the MFCCs
mfcc = speechpy.feature.mfcc(signal, sample_rate)

# Predict the label using the SVM model
label = clf.predict(mfcc)
