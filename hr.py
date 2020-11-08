import cv2 as cv
import io
import time
import numpy as np
import socket
from matplotlib import pyplot as plt
import argparse
import sys
from scipy import signal

class HeartBeat(object):

    def __init__(self):
        self.buffer_size = 100
        self.buffer = []
        self.fps = 0
    
def begin(opt):
    UDP_IP = "127.0.0.1"
    UDP_PORT = 5065
    t0 = time.time()

    sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)

    last = []

    if opt.input:
        print(opt.input)
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
    buffer_size = 20
    times = []
    heartbeat_values = [0]*heartbeat_count
    heartbeat_times = [time.time()]*heartbeat_count
    bpm = 0
    bpms = []

    fig = plt.figure()
    ax = fig.add_subplot(111)
    sock.sendto(("hello").encode(), (UDP_IP, UDP_PORT))
    count = cap.get(cv.CAP_PROP_FPS)

    while(cap.isOpened()):
        ret, frame = cap.read()
        if not ret:
            break
        count += 1

        img = frame.copy()
        # One way of doing this is to use grayscale images pixel values
        # Alternatively, use single channel
        gray = cv.cvtColor(frame, cv.COLOR_BGR2GRAY)    # Display the frame
        detected_faces = face_cascade.detectMultiScale(gray)

        if len(detected_faces) == 0:
            continue

        # Initialize box of size 0
        bestx, besty, bestw, besth = 0, 0, 0, 0

        # Because there can be multiple faces detected, make sure that the right one is selected
        # Simple solution is to take the largest one
        for (x, y, w, h) in detected_faces:
            if (w * h) > (bestw * besth):
                bestx, besty, bestw, besth = x, y, w, h


        # Take center 60 percent of x coordinates
        bestx += int(.20 * bestw)
        bestw = int(bestw * .6)

        # Take center 80 percent of y coordinates
        besty += int(.1 * besth)
        besth = int(besth * .8)

        even_croppier_h = int(besth * .25)

            # Condition to 
            #if 


        crop_img = img[besty:besty + even_croppier_h, bestx:bestx + bestw]

        # Reminder that opencv reads images in BGR format, so R is at index 2
        extracted_r = np.mean(crop_img[:,:,2])
        extracted_g = np.mean(crop_img[:,:,1])
        extracted_b = np.mean(crop_img[:,:,0])

        times.append(time.time() - t0)
        buff.append(extracted_g)

        if (abs(extracted_g - np.mean(buff)) > 10 and len(buff) >= buffer_size):
            pass

        # Make sure that buff isn't longer than buffer size
        if len(buff) > buffer_size:
            buff = buff[-buffer_size:]
            times = times[-buffer_size:]

        processed = np.array(buff)

        if len(buff) == buffer_size:
            spaced_times = np.linspace(times[0], times[-1], len(buff))

            processed = signal.detrend(processed)
            interpolated = np.interp(spaced_times, times, processed)
            interpolated = np.hamming(len(buff)) * interpolated

            normalized = interpolated/np.linalg.norm(interpolated)

            rawfft = np.fft.rfft(normalized*30)

            fps = float(len(buff)) / (times[-1] - times[0])

            freqs = float(fps) / len(buff) * np.arange(len(buff) / 2 + 1)
            freqs *= 60.
            fft = np.abs(rawfft)**2

            # Add floor and ceiling to hr at 50/180 bpm
            idx = np.where((freqs > 50) & (freqs < 180))

            pruned = fft[idx]
            pfreq = freqs[idx]

            bpm = freqs[np.argmax(pruned)]
            bpms.append(bpm)
            ax.plot(pfreq, pruned)

            print(bpm)
            

        heartbeat_values = heartbeat_values[1:] + [np.average(crop_img)]
        heartbeat_times = heartbeat_times[1:] + [time.time()]

        fig.canvas.draw()
        plot_img_np = np.fromstring(fig.canvas.tostring_rgb(),
                dtype=np.uint8, sep='')
        plot_img_np = plot_img_np.reshape(fig.canvas.get_width_height()[::-1] + (3,))
        plt.cla()
        #cv.rectangle(frame, (x, y), (x+w, y+h), (255,0,0), 3)

        if (not (count%30)):
            print("sending", bpm)
            sock.sendto((str(np.average(heartbeat_values))).encode(), (UDP_IP, UDP_PORT))

        cv.rectangle(frame, (bestx, besty), (bestx + bestw, besty + besth), (0,255,0), 2)
        if (opt.view):
            cv.imshow('Main', frame)
            cv.imshow('Crop', crop_img)
            cv.imshow('Graph', plot_img_np)

        key = cv.waitKey(100)
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
