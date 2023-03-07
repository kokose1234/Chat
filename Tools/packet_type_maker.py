from os import walk
from os.path import join
from posixpath import splitext
from pathlib import Path


def save_packet_header(names, packet_type):
    from io import StringIO
    import binascii
    import shutil

    output_str = StringIO()
    output_str.write('namespace Chat.Common.Net.Packet.Header\n{\n    public enum !Header : uint\n    {\n        NullPacket = 0,'.replace('!', packet_type))
    for name in filter(lambda x: x.startswith(packet_type), names):
        crc = binascii.crc32(name.encode('utf8'))
        output_str.write(f'\n        {name} = {crc},')
    output_str.write('\n    }\n}')

    with open(f'../src/Chat.Common/Net/Packet/Header/{packet_type}Header.cs', 'w') as fd:
        output_str.seek(0)
        shutil.copyfileobj(output_str, fd)


proto_path = '../src/Chat.Common/Protos'
file_list = []

for root, dirs, files in walk(proto_path):
    for file in files:
        file_list.append(join(root, file))

file_names = [Path(splitext(f)[0]).stem for f in file_list]

save_packet_header(file_names, 'Server')
save_packet_header(file_names, 'Client')
