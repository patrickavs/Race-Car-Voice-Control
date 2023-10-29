import pyrealsense2 as rs
import numpy as np
import cv2
import socket

# IP-Adresse und Port der VR-Brille
vr_ip = '192.168.0.100'
vr_port = 5000

# Konfiguration der RealSense-Kamera
pipeline = rs.pipeline()
config = rs.config()
config.enable_stream(rs.stream.color, 640, 480, rs.format.bgr8, 30)

# Starten der RealSense-Kamera
pipeline.start(config)

# Schleife zum kontinuierlichen Senden des Kamerabilds an die VR-Brille
while True:
    # Erfassen des Kamerabilds
    frames = pipeline.wait_for_frames()
    color_frame = frames.get_color_frame()
    color_image = np.asanyarray(color_frame.get_data())

    # Senden des Kamerabilds an die VR-Brille
    with socket.socket(socket.AF_INET, socket.SOCK_STREAM) as s:
        s.connect((vr_ip, vr_port))
        s.sendall(cv2.imencode('.jpg', color_image)[1].tobytes())
