import os
import sys
import cv2
import numpy as np
import time
import pyrealsense2 as rs
from adafruit_servokit import ServoKit
from flask import Flask, request, jsonify, send_file, Response
import io
import threading
import subprocess
from PIL import Image
import multiprocessing

app = Flask(__name__)


isForward = True
RIGHT = 70
FORWARD = 90
LEFT = 110
STOP = 0.0
GO = 0.15 
BACKWARD = -0.1
kit = ServoKit(channels=16)
SERVO = 3
MOTOR = 7 

kit.continuous_servo[MOTOR].throttle = GO
kit.servo[SERVO].angle = FORWARD
kit.continuous_servo[MOTOR].throttle = STOP

pipe = rs.pipeline()
config = rs.config()
config.enable_stream(rs.stream.color, 640, 480, rs.format.bgr8, 30)
active = False

net = cv2.dnn.readNet("/home/jetson/Desktop/Car/Läuft/Yolo/darknet/yolov4-tiny.weights", "/home/jetson/Desktop/Car/Läuft/Yolo/darknet/cfg/yolov4-tiny.cfg")
net.setPreferableBackend(cv2.dnn.DNN_BACKEND_CUDA)
net.setPreferableTarget(cv2.dnn.DNN_TARGET_CUDA)

layer_names = net.getLayerNames()
output_layers = [layer_names[i - 1] for i in net.getUnconnectedOutLayers().flatten()]

with open("/home/jetson/Desktop/Car/Läuft/Yolo/darknet/data/coco.names", "r") as f:
    classes = [line.strip() for line in f]

#img_queue = multiprocessing.Queue()
img_queue = multiprocessing.Queue(maxsize=1)

def cam_process(queue):
    global active
    while True:
        img = Image.fromarray(cv2.cvtColor(color_image, cv2.COLOR_BGR2RGB))
        
        if queue.empty():
            queue.put(img)


def cam_process(queue):
    global active
    while True:
        if not active:
            pipe.start(config)
            active = True

        frames = pipe.wait_for_frames()
        color_frame = frames.get_color_frame()
        color_image = np.asanyarray(color_frame.get_data())

        height, width, channels = color_image.shape
        blob = cv2.dnn.blobFromImage(color_image, 0.00392, (416, 416), (0, 0, 0), True, crop=False)

        net.setInput(blob)
        outs = net.forward(output_layers)

        class_ids = []
        confidences = []
        boxes = []

        for out in outs:
            for detection in out:
                scores = detection[5:]
                class_id = np.argmax(scores)
                confidence = scores[class_id]

                if confidence > 0.5:
                    center_x = int(detection[0] * width)
                    center_y = int(detection[1] * height)
                    w = int(detection[2] * width)
                    h = int(detection[3] * height)
                    x = int(center_x - w / 2)
                    y = int(center_y - h / 2)

                    boxes.append([x, y, w, h])
                    confidences.append(float(confidence))
                    class_ids.append(class_id)

        indexes = cv2.dnn.NMSBoxes(boxes, confidences, 0.5, 0.4)

        for i in range(len(boxes)):
            if i in indexes:
                x, y, w, h = boxes[i]
                label = str(classes[class_ids[i]])
                cv2.rectangle(color_image, (x, y), (x + w, y + h), (0, 255, 0), 2)
                cv2.putText(color_image, label, (x, y - 10), cv2.FONT_HERSHEY_SIMPLEX, 0.5, (0, 255, 0), 2)

        img = Image.fromarray(cv2.cvtColor(color_image, cv2.COLOR_BGR2RGB))

        if queue.empty():
            queue.put(img)

        queue.put(img)
        
def exit():
    kit.continuous_servo[MOTOR].throttle = STOP
    kit.servo[SERVO].angle = FORWARD
    print('\nexiting program ', time.strftime('%d.%m.%Y %H:%M:%S', time.localtime()))
    cv2.destroyAllWindows()

# Funktionen für die verschiedenen Befehle
def right(angle=RIGHT):
    kit.servo[SERVO].angle = angle

def forward():
    kit.servo[SERVO].angle = FORWARD

def left(angle=LEFT):
    kit.servo[SERVO].angle = angle

def stop():
    kit.continuous_servo[MOTOR].throttle = STOP

def go(speed=GO):
    global isForward 
    isForward = True
    kit.continuous_servo[MOTOR].throttle = speed

def backward():
    global isForward
    if isForward:
        kit.continuous_servo[MOTOR].throttle = -1
        time.sleep(0.5)
        kit.continuous_servo[MOTOR].throttle = 0.0
        time.sleep(0.5)
        kit.continuous_servo[MOTOR].throttle = -0.3
        print("Backward +isForward=false")
        isForward = False
    else: 
        kit.continuous_servo[MOTOR].throttle = -0.3
        print("Backward")

# Funktionen und REST-Endpunkte...
@app.route('/right', methods=['POST'])
def api_right():
    angle = request.get_json().get('angle', RIGHT)
    print("ANGLE RIGHT: ", angle)
    right(angle)
    return jsonify({'result': 'success'})

@app.route('/forward', methods=['POST'])
def api_forward():
    forward()
    return jsonify({'result': 'success'})

@app.route('/left', methods=['POST'])
def api_left():
    angle = request.get_json().get('angle', LEFT)
    print("ANGLE Left: ", angle)
    left(angle)
    return jsonify({'result': 'success'})
    
@app.route('/stop', methods=['POST'])
def api_stop():
    stop()
    return jsonify({'result': 'success'})

@app.route('/go', methods=['POST'])
def api_go():
    speed = request.get_json().get('speed', GO)
    print("Speed Forward: " , speed)
    go(speed)
    return jsonify({'result': 'success'})

@app.route('/backward', methods=['POST'])
def api_backward():
    backward()
    return jsonify({'result': 'success'})

@app.route('/exit', methods=['POST'])
def api_exit():
    exit()
    return jsonify({'result': 'success'})

@app.route('/cam')
def cam():
    return Response(generate_frames(), mimetype='multipart/x-mixed-replace; boundary=frame')

def generate_frames():
    while True:
        img = img_queue.get()
        frame = cv2.imencode('.jpg', np.array(img))[1].tobytes()
        yield (b'--frame\r\n'
               b'Content-Type: image/jpeg\r\n\r\n' + frame + b'\r\n')

if __name__ == '__main__':
    cam_process = multiprocessing.Process(target=cam_process, args=(img_queue,))
    cam_process.start()
    app.run(debug=False, host='0.0.0.0', port=5000)