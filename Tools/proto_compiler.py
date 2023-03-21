import os
import subprocess

proto_path = os.path.join(os.getcwd(), '')
proto_files = [f for f in os.listdir(proto_path) if f.endswith('.proto')]
proto_files = [os.path.join(proto_path, f) for f in proto_files]

for proto_file in proto_files:
    subprocess.run(['protoc', '--ts_out=import_style=commonjs,binary:.', os.path.basename(proto_file)])
