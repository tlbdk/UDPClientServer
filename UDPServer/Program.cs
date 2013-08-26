using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.IO;
using System.IO.Compression;

namespace UDPServer
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			Console.WriteLine("Starting UDP Server");
			// Create the socket and bind to all interfaces on the machine
			Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
			IPEndPoint ip = new IPEndPoint(IPAddress.Any, 11000);
			socket.Bind(ip);

			// Create a object to hold the sender information
			IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
			EndPoint Remote = (EndPoint)(sender);

			// Loop forever
			while(true)
			{
				var receiveddata = new byte[1024];
				int receiveddatalength = socket.ReceiveFrom(receiveddata, ref Remote);

				// Get the version and size bytes
				byte version = receiveddata[0];
				byte[] sizebytes = new byte[4];
				System.Buffer.BlockCopy(receiveddata, 1, sizebytes, 0, 4);

				// Check if we are on a little endian platform
				if (BitConverter.IsLittleEndian)
					Array.Reverse(sizebytes);

				// Convert to bytes to uint
				UInt32 payloadsize = BitConverter.ToUInt32(sizebytes, 0);

				// Extract received data
				byte[] data = Inflate(receiveddata, 1 + 4, (int)payloadsize);

				// Print received data 
				Console.WriteLine ("Got message of version {0} and total length {1}", version, receiveddatalength);
				Console.WriteLine(Encoding.UTF8.GetString(data));

				// Anwser back with same packet
				//socket.SendTo(data, receivedDataLength, SocketFlags.None, Remote);
			}
		}

		public static byte[] Inflate(byte[] bytes, int index, int length)
		{
			var stream = new MemoryStream();
			var zipStream = new DeflateStream(new MemoryStream(bytes, index, length), CompressionMode.Decompress, true);
			var buffer = new byte[4096];
			while (true)
			{
				var size = zipStream.Read(buffer, 0, buffer.Length);
				if (size > 0) stream.Write(buffer, 0, size);
				else break;
			}
			zipStream.Close();
			return stream.ToArray();
		}
	}
}
