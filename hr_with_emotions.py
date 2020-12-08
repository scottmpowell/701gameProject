import cv2 as cv
import io
import time
import numpy as np
import socket
import argparse
import sys
from scipy import signal

# Emotions imports
from tensorflow.keras.models import Sequential
from tensorflow.keras.layers import Dense, Dropout, Flatten
from tensorflow.keras.layers import Conv2D
from tensorflow.keras.optimizers import Adam
from tensorflow.keras.layers import MaxPooling2D
from tensorflow.keras.preprocessing.image import ImageDataGenerator
import os


class HeartBeat(object):

    def __init__(self):
        self.BUFFER_SIZE = 10
        self.buffer = []
        self.fps = 0
    
def begin(opt):

    # dictionary which assigns each label an emotion (alphabetical order)
    emotion_dict = {0: "Angry", 1: "Disgusted", 2: "Fearful", 3: "Happy", 4: "Neutral", 5: "Sad", 6: "Surprised"}
    os.environ['TF_CPP_MIN_LOG_LEVEL'] = '2'
    UDP_IP = "127.0.0.1"
    UDP_PORT = 5065
    t0 = time.time()

    sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)

    last = []


    if opt.input:
        try:
            cap = cv.VideoCapture(opt.input)
        except:
            print("Unable to open video")
            sys.exit(0)
    else:
        try:
            cap = cv.VideoCapture(0)
        except:
            print("No camera source found")

    cap.set(cv.CAP_PROP_FRAME_WIDTH, 1920)
    cap.set(cv.CAP_PROP_FRAME_HEIGHT, 1080)
    cap.set(cv.CAP_PROP_FPS, 30)


    # Setup Haar cascade
    face_cascade = cv.CascadeClassifier(cv.data.haarcascades + "haarcascade_frontalface_default.xml")

    # Create the emotion-detection model
    model = Sequential()

    model.add(Conv2D(32, kernel_size=(3, 3), activation='relu', input_shape=(48,48,1)))
    model.add(Conv2D(64, kernel_size=(3, 3), activation='relu'))
    model.add(MaxPooling2D(pool_size=(2, 2)))
    model.add(Dropout(0.25))

    model.add(Conv2D(128, kernel_size=(3, 3), activation='relu'))
    model.add(MaxPooling2D(pool_size=(2, 2)))
    model.add(Conv2D(128, kernel_size=(3, 3), activation='relu'))
    model.add(MaxPooling2D(pool_size=(2, 2)))
    model.add(Dropout(0.25))

    model.add(Flatten())
    model.add(Dense(1024, activation='relu'))
    model.add(Dropout(0.5))
    model.add(Dense(7, activation='softmax'))

    model.load_weights('model.h5')

    # prevents openCL usage and unnecessary logging messages
    cv.ocl.setUseOpenCL(False)
    
    heartbeat_count = 60
    buff = []
    BUFFER_SIZE = 100 # Approximately 3 seconds
    BPMS_SIZE = 450 # Approximately 15 seconds
    times = []
    heartbeat_values = [0]*heartbeat_count
    heartbeat_times = [time.time()]*heartbeat_count
    bpm = 0
    bpms = []


    # important variables to keep track of 
    last_good_bpm = 0
    bpm_avg = 0
    elevated_threshold = .1 # What percent hr must be over average to cause spike


    # Emotion
    emotion_index = -1 # Index of emotion to send to Unity.

    count = cap.get(cv.CAP_PROP_FPS)

    while(cap.isOpened()):
        ret, frame = cap.read()
        if not ret:
            break
        count += 1

        img = frame.copy()
        gray = cv.cvtColor(frame, cv.COLOR_BGR2GRAY)    # Display the frame
        detected_faces = face_cascade.detectMultiScale(gray)

        # If no face detection, go to next iteration
        # Causes lag when viewed, but doesn't affect game
        if len(detected_faces) == 0:
            continue

        # Initialize box of size 0
        bestx, besty, bestw, besth = 0, 0, 0, 0

        # Because there can be multiple faces detected, make sure that the right one is selected
        # Simple solution is to take the largest one
        # TODO
        # Handle multiple faces and take closest to previous face
        for (x, y, w, h) in detected_faces:
            if (w * h) > (bestw * besth):
                bestx, besty, bestw, besth = x, y, w, h




        # Take center 60 percent of x coordinates
        hr_bestx = bestx + int(.20 * bestw)
        hr_bestw = int(bestw * .6)

        # Take center 80 percent of y coordinates
        hr_besty = besty + int(.1 * besth)
        hr_besth = int(besth * .8)

        # Only take top 25% of inner 80%
        foreheadh = int(besth * .25)


        # Emotion detection
        crop_img = img[besty:besty + foreheadh, bestx:bestx + bestw]
        emotion_roi = gray[besty:besty + besth, bestx:bestx + bestw]
        emotion_cropped = np.expand_dims(np.expand_dims(cv.resize(emotion_roi, (48, 48)), -1), 0)
        prediction = model.predict(emotion_cropped)
        maxindex = int(np.argmax(prediction))

        # Set emotion to be worst emotion of second
        if maxindex > 5 or maxindex < 3 or emotion_index == -1:
            emotion_index = maxindex

        cv.putText(frame, emotion_dict[maxindex], (bestx+10, besty-10), cv.FONT_HERSHEY_SIMPLEX, 1, (255, 255, 255), 2, cv.LINE_AA)



        # Reminder that opencv reads images in BGR format, so R is at index 2
        # We only need one channel
        #extracted_r = np.mean(crop_img[:,:,2])
        extracted_g = np.mean(crop_img[:,:,1])
        #extracted_b = np.mean(crop_img[:,:,0])


        # If sudden change occurs (bpm change of 10 in a single second, use previous value
        if (abs(extracted_g - np.mean(buff)) > 10 and len(buff) >= BUFFER_SIZE):
            extracted_g = buff[-1]

        times.append(time.time() - t0)
        buff.append(extracted_g)

        # Make sure that buff isn't longer than buffer size
        if len(buff) > BUFFER_SIZE:
            buff = buff[-BUFFER_SIZE:]
            times = times[-BUFFER_SIZE:]

        if len(bpms) > BPMS_SIZE:
            bpms = bpms[-BPMS_SIZE:]

        processed = np.array(buff)

        if len(buff) == BUFFER_SIZE:
            spaced_times = np.linspace(times[0], times[-1], len(buff))

            processed = signal.detrend(processed)
            interpolated = np.interp(spaced_times, times, processed)
            interpolated = np.hamming(BUFFER_SIZE) * interpolated

            normalized = interpolated / np.linalg.norm(interpolated)

            rawfft = np.fft.rfft(normalized*10)

            # This should make it so we use the true fps
            fps = float(BUFFER_SIZE) / (times[-1] - times[0])

            fft = np.abs(rawfft)**2

            freqs = float(fps) / len(buff) * np.arange(BUFFER_SIZE / 2 + 1)
            freqs = 60. * freqs
            
            # Add floor and ceiling to hr at 55/150 bpm
            idx = np.where((freqs >= 55) & (freqs < 150))


            pruned = fft[idx]
            pfreq = freqs[idx]

            # TODO
            # If the max is really far away from both the average and the last good bpm AND there is a value close to the last_good that is only a little bit off from the max, use that instead
            bpm = pfreq[np.argmax(pruned)]
            bpms.append(bpm)


            # Handle Heart Rate average
            long_average = np.mean(bpms)
            short_average = np.mean(bpms[-BUFFER_SIZE:])
            percent_elevation = (short_average - long_average) / long_average

            if percent_elevation >= .1:
                percent_elevation = .1

            elif percent_elevation <= 0:
                percent_elevation = 0


            

        cv.rectangle(frame, (bestx, besty), (bestx+bestw, besty+besth), (255,0,0), 3)

        if (len(buff) == BUFFER_SIZE and not (count%30)):
            print(short_average, "bpm")
            message = str(str(percent_elevation / .1) + " " + emotion_dict[emotion_index] + " " + str(emotion_index) + " " + str(short_average))
            print(message)
            sock.sendto(message.encode(), (UDP_IP, UDP_PORT))
            emotion_index = -1

        if (opt.view):
            cv.imshow('Main', frame)
            cv.imshow('Crop', crop_img)

        key = cv.waitKey(1)
        if key == ord('q'):
            break
    cap.release()
    cv.destroyAllWindows()



if __name__ == "__main__":

    # Construct argument parser

    parser = argparse.ArgumentParser()

    parser.add_argument('-v', '--view', action='store_true', help='display video of detection')
    parser.add_argument('-i', '--input', type=str, default="", help='change detection to work on saved video')

    opt = parser.parse_args()
    begin(opt)
