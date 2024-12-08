using System.IO;
using System.Security.Activation;
using System.Security.Cryptography;
using System;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;

internal static class Program
{

    // Custom encoding example with numbers, latine and cyrilic characters
    private static string Base128 = "0123456789QWERTYUIOPASDFGHJKLZXCVBNMqwertyuiopasdfghjklzxcvbnmЙЦУКЕЁНГШЩЗХЪФЫВАПРОЛДЖЭЯЧСМИТЬБЮйцукеёнгшщзхъфывапролджэячсмитьбю";

    // Input data
    private static byte[] HelloWorld = { 0x48, 0x65, 0x6c, 0x6c, 0x6f, 0x2c, 0x20, 0x77, 0x6f, 0x72, 0x6c, 0x64, 0x21 };

    private static void Main(string[] args)
    {
        byte[] macAddr =
        (
            from netInterface in NetworkInterface.GetAllNetworkInterfaces()
            where netInterface.OperationalStatus == OperationalStatus.Up
            select netInterface.GetPhysicalAddress().GetAddressBytes()
        ).FirstOrDefault();

        // Pass one: using the default encryptor without data.
        Console.WriteLine();
        Console.WriteLine("Default cryptography without data:");
        using (ActivationKey key = ActivationKey.CreateEncryptor(macAddr).Generate())
        {
            using (ActivationKeyDecryptor decryptor = key.CreateDecryptor(macAddr))
            {
                if (decryptor.Success && decryptor.Data.Length != 0)
                {
                    using (TextReader reader = decryptor.GetTextReader(null))
                    {
                        Console.WriteLine("The key content is: " + reader.ReadToEnd());
                    }
                }
                Console.WriteLine("Base10: \t" + key.ToString(PrintableEncoding.Decimal));
                Console.WriteLine("Base16: \t" + key.ToString(PrintableEncoding.Hexadecimal));
                Console.WriteLine("Base32: \t" + key);
                Console.WriteLine("Base64: \t" + key.ToString(PrintableEncoding.Base64));
                Console.WriteLine("Base128:\t" + key.ToString(ActivationKeyTextParser.CreateEncoding(Base128)));
                ActivationKey.DefaultManager.SaveToFile(key, "key1.bin", true);
                ActivationKey.DefaultManager.SaveToFile(key, "key1.txt");
            }
        }

        // Pass two: using the default encryptor with data.
        Console.WriteLine();
        Console.WriteLine("Default cryptography with data:");
        object[] objArray1 = { HelloWorld };
        using (ActivationKey key = ActivationKey.CreateEncryptor(new object[0]).Generate(objArray1))
        {
            using (ActivationKeyDecryptor decryptor = key.CreateDecryptor(new object[0]))
            {
                if (decryptor.Success && (decryptor.Data.Length != 0))
                {
                    using (TextReader reader = decryptor.GetTextReader(null))
                    {
                        Console.WriteLine("The key content is: " + reader.ReadToEnd());
                    }
                }
                Console.WriteLine("Base10: \t" + key.ToString(PrintableEncoding.Decimal));
                Console.WriteLine("Base16: \t" + key.ToString(PrintableEncoding.Hexadecimal));
                Console.WriteLine("Base32: \t" + key);
                Console.WriteLine("Base64: \t" + key.ToString(PrintableEncoding.Base64));
                Console.WriteLine("Base128:\t" + key.ToString(ActivationKeyTextParser.CreateEncoding(Base128)));
                ActivationKey.DefaultManager.SaveToFile(key, "key2.bin", true);
                ActivationKey.DefaultManager.SaveToFile(key, "key2.txt");
            }
        }

        // Pass three: using the AES encryptor and MD5 hash algorithm with data.
        Console.WriteLine();
        Console.WriteLine("Custom cryptography (AES+MD5) with data:");
        object[] objArray2 = { HelloWorld };
        using (ActivationKey key = ActivationKey.CreateEncryptor<AesManaged, MD5CryptoServiceProvider>(new object[0]).Generate(objArray2))
        {
            using (ActivationKeyDecryptor decryptor = key.CreateDecryptor<AesManaged, MD5CryptoServiceProvider>(new object[0]))
            {
                if (decryptor.Success && (decryptor.Data.Length != 0))
                {
                    using (TextReader reader = decryptor.GetTextReader(null))
                    {
                        Console.WriteLine("The key content is: " + reader.ReadToEnd());
                    }
                }
                Console.WriteLine("Base10: \t" + key.ToString(PrintableEncoding.Decimal));
                Console.WriteLine("Base16: \t" + key.ToString(PrintableEncoding.Hexadecimal));
                Console.WriteLine("Base32: \t" + key);
                Console.WriteLine("Base64: \t" + key.ToString(PrintableEncoding.Base64));
                Console.WriteLine("Base128:\t" + key.ToString(ActivationKeyTextParser.CreateEncoding(Base128)));
                ActivationKey.DefaultManager.SaveToFile(key, "key3.bin", true);
                ActivationKey.DefaultManager.SaveToFile(key, "key3.txt", false);
            }
        }
        Console.ReadKey();
    }
}


