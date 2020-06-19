using System.IO;
using System.Security.Cryptography;
using System.Text;
using System;

namespace SqlSync.Common.Helpers
{
	public sealed class SecurityUtils
	{
		private static readonly byte[] AesKey = {
		                                        	190, 58, 227, 223, 204, 92, 189, 112, 216, 82, 97, 167, 179, 2, 181, 76, 11,
		                                        	141, 113, 196, 151, 8, 45, 40, 50, 134, 255, 63, 144, 13, 60, 251
		                                        };

		private static readonly byte[] AesIV = {230, 127, 120, 128, 94, 250, 148, 200, 210, 37, 148, 200, 210, 37, 148, 246};

		/// <summary>
		/// Implements the simple password binary encoding
		/// </summary>
		/// <param name="password">password to crypt.</param>
		/// <returns>a byte[] value representing crypted password</returns>
		public static byte[] CryptPasswordBinary(string password)
		{
			return Encoding.UTF8.GetBytes(password.ToCharArray(), 0, password.Length);
		}

		/// <summary>
		/// Implements the password crypting procedure using SHA1 hash.
		/// </summary>
		/// <param name="password">password to crypt.</param>
		/// <returns>a byte[] value representing crypted password</returns>
		public static byte[] CryptPasswordSecure(string password)
		{
			// convert unicode string into array of bytes.
			Encoding encoding = Encoding.UTF8;
			byte[] bytes = encoding.GetBytes(password);

			// generate the hash value for the byte array
			SHA1 sha = new SHA1CryptoServiceProvider();
			byte[] hashBytes = sha.ComputeHash(bytes);

			return hashBytes;
		}

		public static byte[] Encrypt(string source)
		{
			var provider = new AesCryptoServiceProvider();
			ICryptoTransform transform = provider.CreateEncryptor(AesKey, AesIV);
			using (var stream = new MemoryStream())
			{
				using (var cs = new CryptoStream(stream, transform, CryptoStreamMode.Write))
				{
					byte[] data = Encoding.UTF8.GetBytes(source);
					cs.Write(data, 0, data.Length);
					cs.FlushFinalBlock();
					return stream.ToArray();
				}
			}
		}

		public static string Decrypt(byte[] source)
		{
			var provider = new AesCryptoServiceProvider();
			ICryptoTransform transform = provider.CreateDecryptor(AesKey, AesIV);
			using (var stream = new MemoryStream())
			{
				using (var cs = new CryptoStream(stream, transform, CryptoStreamMode.Read))
				{
					stream.Write(source, 0, source.Length);
					stream.Position = 0;
					return new StreamReader(cs).ReadToEnd();
				}
			}
		}

        public static string GenerateConfirmationNumber(int id)
        {
            string res = String.Empty;

            int cnt = Chars.Length;

            while (id > 0)
            {
                var k = id % cnt;
                id = id / cnt;
                res += Chars[k];
            }

            Random r = new Random(unchecked((int)DateTime.Now.Ticks));

            while (res.Length < 4)
            {
                res = PadChars[r.Next(PadChars.Length)] + res;
            }

            
            res = Chars[r.Next(cnt)] + res;
            res += Chars[r.Next(cnt)];

            return res;

            //return id.ToString("X");
        }

		public static string GenerateConfirmationNumber2(int id)
		{
			string res = String.Empty;

			// 1. Transpose digits of id to get another number
			string idStr = id.ToString().PadLeft(TranspositionLength, '0');

			string transposeString = String.Empty;
			for (int i = 0; i < TranspositionLength; i++)
			{
				transposeString += idStr[Transposition[i]];
			}

			long transpId = Int64.Parse(transposeString);

			// 2. Convert this number into alpha-numeric string;
			int cnt = Chars.Length;
			// Convert our integer to number notation of Chars alphabet
			while (transpId > 0)
			{
				var k = transpId % cnt;
				transpId = transpId / cnt;
				res += Chars[k];
			}
			Random r = new Random(unchecked((int)DateTime.Now.Ticks));
			// Pad to length of 6 with some random characters from padChar array
			while (res.Length < 6)
			{
				res = PadChars[r.Next(PadChars.Length)] + res;
			}

			// and one mpore transposition
			res = res[res.Length - 1] + res.Substring(1, res.Length - 2) + res[0];

			if (res.Length > 6)
			{ 
			}

			return res;
		}

		private const int TranspositionLength = 10;

        private static readonly char[] PadChars = { 'A', 'E', 'I', '0', '3', '7' };
		private static readonly char[] Chars = { 'B', 'C', 'D', 'F', 'G', 'H', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z', '1', '2', '4', '5', '6', '8', '9' };
		private static readonly char[] AllChars = { 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z', '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };
		private static readonly int[] Transposition = new int[TranspositionLength] { 0, 1, 9, 8, 7, 6, 5, 4, 3, 2};
    }
}