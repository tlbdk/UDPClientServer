using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Web.Script.Serialization;
using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Collections.Generic;

// Links: http://automagical.rationalmind.net/2009/02/12/aes-interoperability-between-net-and-iphone/

namespace UDPClient
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			Console.WriteLine("Starting UDP Client");
			var json = new JavaScriptSerializer().Serialize(new { id = 1, lat = 1.1, lon = 1.1 });

			byte[] payloadbytes = Deflate(Encoding.UTF8.GetBytes(json));

			// Encrypt bytes
			//payloadbytes = EncryptBytes(payloadbytes, "mylongkey");

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

		//TODO: Make sure IV is set to something else
		public static byte[] EncryptBytes(byte[] plainbytes, string key, byte[] iv = null)
		{
			iv = iv ?? new byte[] {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0};
			//Set up the encryption objects
			using (AesCryptoServiceProvider acsp = new AesCryptoServiceProvider())
			{
				acsp.BlockSize = 128;
				acsp.KeySize = 128;
				acsp.Mode = CipherMode.CBC;
				acsp.Padding = PaddingMode.PKCS7;

				acsp.GenerateIV();

				acsp.IV = iv;
				acsp.Key = GetKey(Encoding.Default.GetBytes(key), acsp);

				ICryptoTransform ict = acsp.CreateEncryptor();

				//Set up stream to contain the encryption
				MemoryStream ms = new MemoryStream();

				//Perform the encryption, storing output into the stream
				CryptoStream cs = new CryptoStream(ms, ict, CryptoStreamMode.Write);
				cs.Write(plainbytes, 0, plainbytes.Length);
				cs.FlushFinalBlock();

				return ms.ToArray(); //.ToArray() is important, don't mess with the buffer
			}
		}

		private static byte[] GetKey(byte[] suggestedKey, SymmetricAlgorithm p)
		{
			byte[] kRaw = suggestedKey;
			List<byte> kList = new List<byte>();

			for (int i = 0; i < p.LegalKeySizes[0].MinSize; i += 8)
			{
				kList.Add(kRaw[(i / 8) % kRaw.Length]);
			}
			byte[] k = kList.ToArray();
			return k;
		}
	}
}
