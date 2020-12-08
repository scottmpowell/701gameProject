import cv2 as cv
import io
import time
import numpy as np
import socket
from matplotlib import pyplot as plt
import argparse
import sys
import platform
from scipy import signal


class HeartBeat(object):

    def __init__(self):
        self.BUFFER_SIZE = 10
        self.buffer = []
        self.fps = 0
    
def begin(opt):

    # dictionary which assigns each label an emotion (alphabetical order)
    UDP_IP = "127.0.0.1"
    UDP_PORT = 5065
    t0 = time.time()

    sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)

    last = []
    if platform.system() == 'darwin': # MacOS
        use('agg')


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


        crop_img = img[besty:besty + foreheadh, bestx:bestx + bestw]
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
            message = str(str(percent_elevation / .1) + " " + "EMOTIONLESS" + " " + "4" + " " + str(short_average))
            print(message)
            sock.sendto(message.encode(), (UDP_IP, UDP_PORT))

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
