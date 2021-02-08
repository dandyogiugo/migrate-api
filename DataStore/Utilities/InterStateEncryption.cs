using Newtonsoft.Json;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace DataStore.Utilities
{
    public static class InterStateEncryption
    {
        private static int PROVIDER_RSA_FULL = 1;
        private static string KEY_CONTAINER_NAME = "FolioAPIKeyContainer";
        private static int KEY_SIZE = 2048;
        public static string GetSignature(string text)
        {
            GenerateKeyPair();
            byte[] signatureBytes = SignText(text);
            if (signatureBytes != null)
            {
                return Convert.ToBase64String(signatureBytes);
            }
            return null;
        }

        public static bool VerifySignature(string text, string signature)
        {
            byte[] textBytes = Encoding.UTF8.GetBytes(text);
            byte[] signatureBytes = Convert.FromBase64String(signature);
            return VerifySignedText(textBytes, signatureBytes);
        }

        public static byte[] SignText(string text)
        {
            try
            {
                RSACryptoServiceProvider rsa = GetRSACryptoServiceProviderFromContainer();
                // Hash and sign the text. Pass a new instance of SHA512
                // to specify the hashing algorithm.
                return rsa.SignData(Encoding.UTF8.GetBytes(text), SHA512.Create());
            }
            catch (CryptographicException e)
            {
                Console.WriteLine(e.Message);
                return null;
            }
        }

        public static bool VerifySignedText(byte[] text, byte[] signature)
        {
            try
            {
                RSACryptoServiceProvider rsa = GetRSACryptoServiceProviderFromContainer();

                // Verify the signed text using the signature. Pass a new instance of SHA512
                // to specify the hashing algorithm.
                return rsa.VerifyData(text, SHA512.Create(), signature);
            }
            catch (CryptographicException e)
            {
               // Console.WriteLine(e.Message);

                return false;
            }
        }

        private static void GenerateKeyPair()
        {
            CspParameters cspParams = new CspParameters(PROVIDER_RSA_FULL);
            cspParams.KeyContainerName = KEY_CONTAINER_NAME;
            cspParams.Flags = CspProviderFlags.UseMachineKeyStore;
            cspParams.ProviderName = "Microsoft Strong Cryptographic Provider";
            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider(KEY_SIZE, cspParams);

            // Console.WriteLine("The RSA key pair with a key size of {0} bits was added to the container: \"{1}\".", rsa.KeySize, KEY_CONTAINER_NAME);
            // Important! The key pair needs to be loaded from the key container
            // for the correct key information that was stored to be displayed.

            rsa = GetRSACryptoServiceProviderFromContainer();
            var pub_key = rsa.ToXmlString(false); // export public key
            var priv_key = rsa.ToXmlString(true); // export private key
            // Display the key information to the console.
            //Console.WriteLine($"Key information retrieved from container : \n {rsa.ToXmlString(true)}");
        }
        private static void DeleteKeyPairFromContainer()
        {
            RSACryptoServiceProvider rsa = GetRSACryptoServiceProviderFromContainer();
            rsa.PersistKeyInCsp = false;
            // Call Clear to release resources and delete the key from the container.
            rsa.Clear();
            //Console.WriteLine("The RSA key pair was deleted from the container: \"{0}\".", KEY_CONTAINER_NAME);
        }
        private static RSACryptoServiceProvider GetRSACryptoServiceProviderFromContainer()
        {
            // Create the CspParameters object and set the key container
            // name used to store the RSA key pair.
            CspParameters cspParams = new CspParameters();
            cspParams.KeyContainerName = KEY_CONTAINER_NAME;
            // Create a new instance of RSACryptoServiceProvider that accesses
            // the key container.
            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider(KEY_SIZE, cspParams);
            return rsa;
        }
    }
}
