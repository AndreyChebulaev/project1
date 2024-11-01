using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

class Program
{
    static void Main(string[] args)
    {
        string[] filePaths = { "Чернятьев.docx" };
        string checksumsFilePath = "checksums.txt";

        using (StreamWriter writer = new StreamWriter(checksumsFilePath))
        {
            foreach (string filePath in filePaths)
            {
                string checksum = CalculateChecksum(filePath);
                writer.WriteLine($"{filePath}: {checksum}");
            }
        }

        Console.WriteLine($"Checksums have been written to '{checksumsFilePath}'");
    }

    static string CalculateChecksum(string filePath)
    {
        using (SHA256 sha256 = SHA256.Create())
        {
            using (FileStream fs = File.OpenRead(filePath))
            {
                byte[] hashBytes = sha256.ComputeHash(fs);

                StringBuilder sb = new StringBuilder();
                foreach (byte b in hashBytes)
                {
                    sb.Append(b.ToString("x2"));
                }

                return sb.ToString();
            }
        }
    }
}
