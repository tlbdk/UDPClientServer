using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Web.Script.Serialization;
using System.IO;
using System.IO.Compression;

namespace UDPClient
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			Console.WriteLine("Starting UDP Client");
			var json = new JavaScriptSerializer().Serialize(new { id = 1, lat = 1.1, lon = 1.1 });

			byte[] payloadbytes = Deflate(Encoding.UTF8.GetBytes(json));

			// Create Network Order SizeBytes
			byte[] sizebytes = BitConverter.GetBytes((UInt32)payloadbytes.Length);
			if (BitConverter.IsLittleEndian)
				Array.Reverse(sizebytes);

			// Concat SizeBytes + Msg
			byte[] packet = new byte[1 + sizebytes.Length + payloadbytes.Length];
			packet[0] = 1; // Set version
			System.Buffer.BlockCopy(sizebytes, 0, packet, 1, sizebytes.Length);
			System.Buffer.BlockCopy(payloadbytes, 0, packet, sizebytes.Length + 1, payloadbytes.Length);

			// Create socket
			Socket sock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

			// Create endpoint to send packet toS
			IPEndPoint endpoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 11000);

			sock.SendTo(packet, endpoint);
		}

		public static byte[] Deflate(byte[] bytes)
		{
			var stream = new MemoryStream();
			var zipStream = new DeflateStream(stream, CompressionMode.Compress, true);
			zipStream.Write(bytes, 0, bytes.Length);
			zipStream.Close();
			return stream.ToArray();
		}

	}
}
