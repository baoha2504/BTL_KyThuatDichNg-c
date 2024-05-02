from flask import Flask, request, jsonify
import os
from datetime import datetime

app = Flask(__name__)

# Đường dẫn tới thư mục chứa mã nguồn của ứng dụng
APP_ROOT = os.path.dirname(os.path.abspath(__file__))

# Đường dẫn tới thư mục Images trong thư mục chứa mã nguồn
UPLOAD_FOLDER = os.path.join(APP_ROOT, 'Images')
if not os.path.exists(UPLOAD_FOLDER):
    os.makedirs(UPLOAD_FOLDER)

app.config['UPLOAD_FOLDER'] = UPLOAD_FOLDER

@app.route('/upload_file', methods=['POST'])
def upload_file():
    if 'file' not in request.files:
        return jsonify({'error': 'No file part'})
    file = request.files['file']
    if file.filename == '':
        return jsonify({'error': 'No selected file'})
    if file:
        filename = file.filename
        file.save(os.path.join(app.config['UPLOAD_FOLDER'], filename))
        return jsonify({'message': 'File uploaded successfully', 'filename': filename})

@app.route('/upload_text', methods=['POST'])
def upload_text():
    data = request.get_json()
    if data and 'text' in data:
        text = data['text']
        # Lấy thời gian hiện tại
        current_time = datetime.now().strftime("%Y-%m-%d %H:%M:%S")
        # Viết dữ liệu vào file với thời gian và nội dung
        with open(os.path.join(APP_ROOT, 'hacker.txt'), 'a', encoding='utf-8') as file:
            file.write(f"{current_time}: {text}\n")
        return jsonify({'message': 'Text uploaded successfully'})
    else:
        return jsonify({'error': 'No text provided'})

if __name__ == '__main__':
    app.run(debug=True, host='192.168.43.107', port=5000)


# POST Image: http://localhost:5000/upload