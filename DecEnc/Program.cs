using System;
using System.IO;
using System.Text;

namespace DecEnc
{
    class Program
    {
        private static string Encrypt(string data, ushort passcode)
        {
            StringBuilder ret = new StringBuilder();
            for (int i = 0; i < data.Length; i++) {
                int newVal = (data[i] * 1822) + ushort.MaxValue / 2 + passcode;
                ret.Append(newVal + " ");
            }
            return ret.ToString().Trim();
        }

        private static string Decrypt(string cipher, ushort passcode)
        {
            StringBuilder sb = new StringBuilder();
            string[] numbers = cipher.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string number in numbers)
            {
                int num = int.Parse(number);
                char val = (char)((num - passcode - (ushort.MaxValue / 2)) / 1822);
                sb.Append(val);
            }
            return sb.ToString();
        }

        public static string[] DecryptString(string cipher, string passcode = "")
        {
            string[] spl = cipher.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
            string[] parts = spl[0].Split(new string[] { "::" }, StringSplitOptions.None);
            if (parts[0] == "#DEC_ENC")
            {
                ushort code = (ushort)passcode.GetHashCode();
                ushort defcode = (ushort)"".GetHashCode();
                string headerMsg = Decrypt(parts[1], defcode);
                string signature = Decrypt(parts[2], defcode);
                string check = Decrypt(parts[3], code);
                string extension = "";
                if (parts.Length > 4) extension = Decrypt(parts[4], defcode);
                if (check == "ENCODED")
                {
                    string ctx = Decrypt(spl[1], code);
                    return new string[] { headerMsg, signature, ctx, extension, "1" };
                }
                else return new string[] { headerMsg, signature, "", extension, "0" };
            }
            return new string[] { "", "", "", "", "" };
        }

        public static string[] ForceDecryptString(string cipher)
        {
            int magicNumber = 158485; // In fact it is ('E' * 1822) + ushort.MaxValue / 2
                                      // Think more about this number and you will know how does this function work.
            string[] spl = cipher.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
            string[] parts = spl[0].Split(new string[] { "::" }, StringSplitOptions.None);
            if (parts[0] == "#DEC_ENC")
            {
                ushort defcode = (ushort)"".GetHashCode();
                ushort code = (ushort)(int.Parse(parts[3].Split(' ')[0]) - magicNumber);
                string headerMsg = Decrypt(parts[1], defcode);
                string signature = Decrypt(parts[2], defcode);
                string check = Decrypt(parts[3], code);
                string extension = "";
                if (parts.Length > 4) extension = Decrypt(parts[4], defcode);
                if (check == "ENCODED")
                {
                    string ctx = Decrypt(spl[1], code);
                    return new string[] { headerMsg, signature, ctx, extension, "1" };
                }
                else return new string[] { headerMsg, signature, "", extension, "0" };
            }
            return new string[] { "", "", "", "", "" };
        }

        public static string EncryptString(string data, string header, string signature, string passcode = "", string extension = null)
        {
            var code = (ushort)passcode.GetHashCode();
            var defcode = (ushort)"".GetHashCode();
            var sb = new StringBuilder();
            sb.Append("#DEC_ENC::" + Encrypt(header, defcode) + "::" + Encrypt(signature, defcode) +
                      "::" + Encrypt("ENCODED", code));
            if (extension != null)
                sb.Append("::" + Encrypt(extension, defcode));
            sb.Append("\r\n" + Encrypt(data, code));
            return sb.ToString();
        }

        static void Main(string[] args)
        {
            bool isEncrypt = true;
            bool isForced = false;
            bool showInfo = false;
            string inFile = null;
            string pass = "";
            string signature = "127.0.0.1";
            string header = "Encrypted by DecEnc Tool (Author: Micrafast)";
            try
            {
                for (int i = 0; i < args.Length; i++)
                {
                    var arg = args[i];
                    if (arg == "/input" || arg == "/in" || arg == "/i")
                        inFile = args[i + 1];
                    if (arg == "/decrypt" || arg == "/d")
                        isEncrypt = false;
                    if (arg == "/force" || arg == "/f")
                        isForced = true;
                    if (arg == "/password" || arg == "/pass" || arg == "/p")
                        pass = args[i + 1];
                    if (arg == "/signature" || arg == "/sig" || arg == "/s")
                        signature = args[i + 1];
                    if (arg == "/header" || arg == "/h")
                        header = args[i + 1];
                    if (arg == "/info" || arg == "/inf")
                        showInfo = true;
                }
            }
#pragma warning disable CS0168
            catch (Exception e)
#pragma warning restore CS0168
            {
                Console.WriteLine("Invalid argument!");
                return;
            }
            if (inFile == null)
            {
                Console.WriteLine("Input file cannot be empty!");
                return;
            }
            if(isEncrypt)
            {
                var sr = new StreamReader(new FileStream(inFile, FileMode.Open));
                string rawData = sr.ReadToEnd();
                sr.Close();
                string encrypted = EncryptString(rawData, header, signature, pass, GetExt(inFile));
                var outFile = GetOutFileName(inFile, "dec");
                var sw = new StreamWriter(new FileStream(outFile, FileMode.Create));
                sw.Write(encrypted);
                sw.Flush();
                sw.Close();
            }
            else
            {
                var sr = new StreamReader(new FileStream(inFile, FileMode.Open));
                string data = sr.ReadToEnd();
                string[] decrypted;
                if (isForced)
                    decrypted = ForceDecryptString(data);
                else
                    decrypted = DecryptString(data, pass);
                if (decrypted[4] == "")
                {
                    Console.WriteLine("Error: Input file is not a Hacknet-DEC encrypted file.");
                    return;
                }
                else if (decrypted[4] == "0")
                {
                    Console.WriteLine("Error: Password wrong.");
                    return;
                }
                else
                {
                    if (showInfo)
                    {
                        Console.WriteLine("Header: " + decrypted[0]);
                        Console.WriteLine("Signature: " + decrypted[1]);
                        Console.WriteLine("Extension: " + decrypted[3]);
                    }
                    string outFile = GetOutFileName(inFile, decrypted[3]);
                    var sw = new StreamWriter(new FileStream(outFile, FileMode.Create));
                    sw.Write(decrypted[2]);
                    sw.Flush();
                    sw.Close();
                }
            }
        }

        public static string GetExt(string file)
        {
            string[] strs = file.Split('.');
            if (strs.Length >= 2) return strs[strs.Length - 1];
            return null;
        }

        public static string GetOutFileName(string inFile, string extension)
        {
            string[] strs = inFile.Split('.');
            if (strs.Length >= 2)
            {
                strs[strs.Length - 1] = extension.TrimStart(new char[] { '.' });
                return string.Join(".", strs);
            }
            else
            {
                var ext = extension.TrimStart(new char[] { '.' });
                return inFile + '.' + ext;
            }
        }
    }
}
