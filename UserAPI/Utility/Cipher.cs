using System;
using System.Security.Cryptography;
using System.Text;

namespace UserAPI.Utility
{
    public class Cipher
    {
        private static TripleDESCryptoServiceProvider Provider(string key)
        {
            //[pqa] prepare provider
            TripleDESCryptoServiceProvider tripleDES = new TripleDESCryptoServiceProvider();
            tripleDES.Key = UTF8Encoding.UTF8.GetBytes(key);
            tripleDES.Mode = CipherMode.ECB;
            tripleDES.Padding = PaddingMode.PKCS7;

            return tripleDES;
        }

        public static string Encrypt(string input, string key)
        {
            //[pqa] the key that will go to the encryption algorithm should be 128 bit (16 chars)
            //[pqa] the logic below will ensure that the key have 128 bit.
            if (key.Length < 16)
            {
                //[pqa] increase the string if less than 128 bit
                key += "this_is_the_default_key";
            }

            //[pqa] trim to 128 bit
            key = key.Substring(0, 16);

            try
            {
                //[pqa] try to encrypt
                byte[] inputArray = UTF8Encoding.UTF8.GetBytes(input);
                TripleDESCryptoServiceProvider tripleDES = Provider(key);
                ICryptoTransform cTransform = tripleDES.CreateEncryptor();
                byte[] resultArray = cTransform.TransformFinalBlock(inputArray, 0, inputArray.Length);
                tripleDES.Clear();
                return Convert.ToBase64String(resultArray, 0, resultArray.Length);
            }
            catch (Exception e)
            {
                return null;
            }
        }
        public static string Decrypt(string input, string key)
        {
            //[pqa] the key that will go to the encryption algorithm should be 128 bit (16 chars). 
            //[pqa] the logic below will ensure that the key have 128 bit.
            if (key.Length < 16)
            {
                //[pqa] increase the string if less than 128 bit
                key += "this_is_the_default_key";
            }

            //[pqa] trim to 128 bit
            key = key.Substring(0, 16);

            try
            {
                //[pqa] try to decrypt
                byte[] inputArray = Convert.FromBase64String(input);
                TripleDESCryptoServiceProvider tripleDES = Provider(key);
                ICryptoTransform cTransform = tripleDES.CreateDecryptor();
                byte[] resultArray = cTransform.TransformFinalBlock(inputArray, 0, inputArray.Length);
                tripleDES.Clear();
                return UTF8Encoding.UTF8.GetString(resultArray);
            }
            catch (Exception e)
            {
                return null;
            }
        }
    }
}
