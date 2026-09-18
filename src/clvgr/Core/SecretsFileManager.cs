using Google.Protobuf;
using System.Collections.ObjectModel;
using System.Security.Cryptography;
using static clvgr.Core.Encryption;

namespace clvgr.Core;

internal class SecretsFileManager : IDisposable
{
    private readonly FileStream _secretsFileStream;
    private byte[]? _startCheckSum;

    private SecretsFileManager(byte[] salt, FileStream secretsFileStream, ICryptor cryptor)
    {
        _secretsFileStream = secretsFileStream;
        Salt = salt;
        Cryptor = cryptor;

        ObservableCollection<Resource>? resources = null;

        if (secretsFileStream.Position == secretsFileStream.Length)
        {
            resources = [];
        }
        else
        {
            byte[] data = new byte[secretsFileStream.Length - secretsFileStream.Position];
            secretsFileStream.ReadExactly(data);

            cryptor.Decrypt(data, plaintext =>
            {
                var secretsFilePayload = SecretsFilePayload.Parser.ParseFrom(plaintext);

                _startCheckSum = SHA256.HashData(plaintext);

                resources = [with(secretsFilePayload.Secrets)];
            });
        }

        if (resources is null)
        {
            throw new InvalidOperationException($"{nameof(SecretsFileManager)} construction failed.");
        }

        Resources = resources;
    }

    public ICryptor Cryptor { get; }
    public ObservableCollection<Resource> Resources { get; }
    public byte[] Salt { get; }
    public string SecretsFileName => _secretsFileStream.Name;

    public bool IsModified()
    {
        var secretsFilePayload = new SecretsFilePayload();
        secretsFilePayload.Secrets.AddRange(Resources);

        byte[] currentCheckSum = SHA256.HashData(secretsFilePayload.ToByteArray());

        return !currentCheckSum.SequenceEqual(_startCheckSum);
    }

    public void Persist()
    {
        var secretsFilePayload = new SecretsFilePayload();
        secretsFilePayload.Secrets.AddRange(Resources);

        byte[] encryptedData = Cryptor.Encrypt(secretsFilePayload.ToByteArray());

        _secretsFileStream.Seek(0, SeekOrigin.Begin);
        _secretsFileStream.Write(Salt.Concat(encryptedData).ToArray());
        _secretsFileStream.SetLength(_secretsFileStream.Position);
        _secretsFileStream.Flush(/*true???*/);
    }

    public void Dispose()
    {
        long len = _secretsFileStream.Length;
        string path = _secretsFileStream.Name;

        _secretsFileStream.Close();

        if (len is 0) File.Delete(path);
    }

    public class Constructor(ICryptorFactory cryptorFactory, byte[] salt, FileStream secretsFileStream)
    {
        public byte[] Salt => salt;

        public SecretsFileManager Construct(ReadOnlySpan<byte> passwordPlaintext)
            => new(salt, secretsFileStream, cryptorFactory.Create(passwordPlaintext, salt));

        public void DestroyIfEmpty()
        {
            long len = secretsFileStream.Length;
            string path = secretsFileStream.Name;

            secretsFileStream.Close();

            if (len is 0) File.Delete(path);
        }
    }
}
