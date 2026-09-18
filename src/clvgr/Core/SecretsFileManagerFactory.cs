using System.Security.Cryptography;
using static clvgr.Core.Encryption;

namespace clvgr.Core;

internal class SecretsFileManagerFactory(ICryptorFactory cryptorFactory)
{
    public SecretsFileManager.Constructor Preconstruct(string path)
    {
        byte[] salt = new byte[Constants.SaltLength];
        FileStream secretsFileStream = File.Open(path, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);

        if (secretsFileStream.Length > 0)
        {
            if (secretsFileStream.Length < Constants.SaltLength)
            {
                throw new Exception("The file is corrupted");
            }

            secretsFileStream.ReadExactly(salt, 0, Constants.SaltLength);
        }
        else
        {
            RandomNumberGenerator.Fill(salt);
        }

        return new SecretsFileManager.Constructor(cryptorFactory, salt, secretsFileStream);
    }
}
