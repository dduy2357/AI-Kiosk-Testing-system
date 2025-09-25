from flask import Flask, request, jsonify
from deepface import DeepFace
import cv2
import numpy as np
import requests
import mediapipe as mp
from flasgger import Swagger, swag_from

app = Flask(__name__)
swagger = Swagger(app)

# Emotion → (Valence, Arousal)
EMOTION_VA_MAPPING = {
    'happy':    (0.8, 0.6),
    'angry':    (-0.6, 0.7),
    'sad':      (-0.7, 0.3),
    'fear':     (-0.6, 0.8),
    'disgust':  (-0.4, 0.6),
    'surprise': (0.4, 0.9),
    'neutral':  (0.0, 0.2)
}

# For eye direction
LEFT_EYE = list(range(33, 133))
RIGHT_EYE = list(range(362, 463))
mp_face_mesh = mp.solutions.face_mesh
face_mesh = mp_face_mesh.FaceMesh(static_image_mode=True, max_num_faces=1, refine_landmarks=True)

def detect_eye_direction(image):
    """Detects whether person is glancing left/right/center using eye landmarks."""
    img_rgb = cv2.cvtColor(image, cv2.COLOR_BGR2RGB)
    results = face_mesh.process(img_rgb)

    if not results.multi_face_landmarks:
        return "unknown"

    h, w, _ = image.shape
    landmarks = results.multi_face_landmarks[0].landmark

    left_eye_x = np.mean([landmarks[i].x for i in LEFT_EYE])
    right_eye_x = np.mean([landmarks[i].x for i in RIGHT_EYE])
    eye_center_x = (left_eye_x + right_eye_x) / 2

    if eye_center_x < 0.45:
        return "looking right"
    elif eye_center_x > 0.55:
        return "looking left"
    else:
        return "looking center"

def infer_state(valence, arousal):
    if valence > 0.5 and 0.5 <= arousal <= 0.8:
        return "Confident"
    elif valence < -0.3 and arousal > 0.6:
        return "Anxious"
    elif valence < 0 and arousal > 0.5:
        return "Stressed"
    elif valence > 0.4 and arousal < 0.4:
        return "Relaxed"
    elif 0 <= valence <= 0.5 and 0.4 <= arousal <= 0.6:
        return "Focused"
    elif -0.1 <= valence <= 0.1 and arousal < 0.3:
        return "Distracted"
    else:
        return "Mixed"

def detect_face_orientation(img):
    with mp_face_mesh.FaceMesh(static_image_mode=True, max_num_faces=1, refine_landmarks=True) as face_mesh_check:
        results = face_mesh_check.process(cv2.cvtColor(img, cv2.COLOR_BGR2RGB))
        if not results.multi_face_landmarks:
            return "NoFace"

        face_landmarks = results.multi_face_landmarks[0]
        keypoints = {i: face_landmarks.landmark[i] for i in [33, 263, 1]}  # left eye, right eye, nose

        left_eye = keypoints[33]
        right_eye = keypoints[263]
        nose_tip = keypoints[1]

        eye_dx = abs(left_eye.x - right_eye.x)
        eye_dy = abs(left_eye.y - right_eye.y)
        tilt_ratio = eye_dy / eye_dx if eye_dx != 0 else 0

        if tilt_ratio > 0.3:
            return "Tilted"
        elif abs(nose_tip.x - 0.5) > 0.2:
            return "TurnedSideways"
        else:
            return "Frontal"

@app.route('/analyze', methods=['POST'])
@swag_from({
    'summary': 'Detect face, analyze emotion, infer mental state, and detect eye direction',
    'consumes': ['multipart/form-data'],
    'parameters': [{
        'name': 'image',
        'in': 'formData',
        'type': 'file',
        'required': True,
        'description': 'Face image for analysis'
    }],
    'responses': {
        200: {'description': 'Emotion and inferred state results'}
    }
})
def analyze():
    file = request.files.get('image')
    if not file:
        return jsonify({"status": "error", "message": "No file uploaded"}), 400

    try:
        img_array = np.frombuffer(file.read(), np.uint8)
        img = cv2.imdecode(img_array, cv2.IMREAD_COLOR)

        # Check face orientation
        orientation = detect_face_orientation(img)
        if orientation == "NoFace":
            return jsonify({
                "status": "error",
                "message": "No face detected. Make sure your face is clearly visible in the image."
            }), 400
        elif orientation != "Frontal":
            return jsonify({
                "status": "error",
                "message": f"Face orientation not frontal: {orientation}. Please face the camera directly.",
                "suggestions": [
                    "Face the camera directly",
                    "Avoid turning your head sideways or tilting",
                    "Ensure your face is well-lit and within the frame"
                ]
            }), 400

        # Detect eye direction
        glance = detect_eye_direction(img)
        if glance in ["looking left", "looking right"]:
            return jsonify({
                "status": "error",
                "message": f"Eye direction not center: {glance}. Please look directly at the camera.",
                "suggestions": [
                    "Look directly into the camera",
                    "Avoid looking to the side",
                    "Make sure both eyes are visible"
                ]
            }), 400

        # Analyze with DeepFace
        results = DeepFace.analyze(img, actions=['emotion'], enforce_detection=True)
        faces = results if isinstance(results, list) else [results]

        if len(faces) == 0:
            return jsonify({"status": "success", "result": "NotDetected"})
        if len(faces) > 1:
            return jsonify({
                "status": "warning",
                "result": "MultipleFacesDetected",
                "count": len(faces)
            })

        face = faces[0]
        emotions = face.get("emotion", {})
        region = face.get("region", {})
        dominant = face.get("dominant_emotion", "")

        v_sum = a_sum = weight_sum = 0
        for emo, score in emotions.items():
            if emo in EMOTION_VA_MAPPING:
                v, a = EMOTION_VA_MAPPING[emo]
                v_sum += v * score
                a_sum += a * score
                weight_sum += score

        valence = v_sum / weight_sum if weight_sum else 0
        arousal = a_sum / weight_sum if weight_sum else 0
        state = infer_state(valence, arousal)

        return jsonify({
            "status": "success",
            "result": "Detected",
            "region": region,
            "dominant_emotion": dominant,
            "emotions": emotions,
            "avg_valence": round(valence, 3),
            "avg_arousal": round(arousal, 3),
            "inferred_state": state,
            "glance": glance
        })

    except ValueError as ve:
        if "Face could not be detected" in str(ve):
            return jsonify({
                "status": "error",
                "message": "DeepFace could not detect a face. Make sure your face is fully visible and not rotated or obscured."
            }), 400
        return jsonify({"status": "error", "message": str(ve)}), 400
    except Exception as e:
        return jsonify({"status": "error", "message": str(e)}), 500

@app.route('/verify-face', methods=['POST'])
@swag_from({
    'summary': 'Verify face between uploaded file and image from URL',
    'consumes': ['multipart/form-data'],
    'parameters': [
        {
            'name': 'image_file',
            'in': 'formData',
            'type': 'file',
            'required': True,
            'description': 'Image to verify (e.g. webcam capture)'
        },
        {
            'name': 'image_url',
            'in': 'formData',
            'type': 'string',
            'required': True,
            'description': 'URL of the reference image (e.g. profile)'
        }
    ],
    'responses': {
        200: {'description': 'Face verification result'}
    }
})
def verify_face():
    file = request.files.get('image_file')
    image_url = request.form.get('image_url')

    if not file or not image_url:
        return jsonify({"status": "error", "message": "Both image_file and image_url are required"}), 400

    try:
        img1 = cv2.imdecode(np.frombuffer(file.read(), np.uint8), cv2.IMREAD_COLOR)

        response = requests.get(image_url)
        if response.status_code != 200:
            return jsonify({"status": "error", "message": "Failed to fetch image from URL"}), 400

        img2 = cv2.imdecode(np.frombuffer(response.content, np.uint8), cv2.IMREAD_COLOR)

        result = DeepFace.verify(img1, img2, enforce_detection=True)

        return jsonify({
            "status": "success",
            "verified": result["verified"],
            "distance": round(result["distance"], 4),
            "threshold": round(result["threshold"], 4),
            "model": result["model"],
            "similarity_metric": result["similarity_metric"]
        })

    except Exception as e:
        return jsonify({"status": "error", "message": str(e)}), 500

if __name__ == '__main__':
    app.run(host='0.0.0.0', port=5000)
