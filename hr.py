import cv2 as cv
import io
import time
import numpy as np
import socket
from matplotlib import pyplot as plt
import argparse
import sys

def begin(opt):
    UDP_IP = "127.0.0.1"
    UDP_PORT = 5065

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
    heartbeat_count = 128
    heartbeat_values = [0]*heartbeat_count
    heartbeat_times = [time.time()]*heartbeat_count

    fig = plt.figure()
    ax = fig.add_subplot(111)
    sock.sendto(("hello").encode(), (UDP_IP, UDP_PORT))

    while(True):
        ret, frame = cap.read()
        img = frame.copy()
        gray = cv.cvtColor(frame, cv.COLOR_BGR2GRAY)    # Display the frame
        detected_faces = face_cascade.detectMultiScale(gray)

        for (x, y, w, h) in detected_faces:
            cv.rectangle(frame, (x, y), (x + w, y + h), (0,255,0), 2)


        crop_img = img[y:y + h, x:x + w]

        heartbeat_values = heartbeat_values[1:] + [np.average(crop_img)]
        heartbeat_times = heartbeat_times[1:] + [time.time()]

        ax.plot(heartbeat_times, heartbeat_values)
        fig.canvas.draw()
        plot_img_np = np.fromstring(fig.canvas.tostring_rgb(),
                dtype=np.uint8, sep='')
        plot_img_np = plot_img_np.reshape(fig.canvas.get_width_height()[::-1] + (3,))
        plt.cla()
        #cv.rectangle(frame, (x, y), (x+w, y+h), (255,0,0), 3)

        #sock.sendto((heartbeat_values).encode(), (UDP_IP, UDP_PORT))

        if (opt.view):
            cv.imshow('Main', frame)
            cv.imshow('Crop', crop_img)
            cv.imshow('Graph', plot_img_np)

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
