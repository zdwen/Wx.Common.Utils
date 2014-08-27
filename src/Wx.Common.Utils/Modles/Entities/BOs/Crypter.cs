using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;

namespace Wx.Common.Utils.Modles.Entities.BOs
{
    public class Crypter
    {
        public Encoding Encoding { get; set; }
        /// <summary>
        /// 指定密钥,密钥使用UTF8编码成byte[]后长度必须为16
        /// </summary>
        public string Key { get; set; }
        /// <summary>
        /// 指定向量,向量使用UTF8编码成byte[]后长度必须为16
        /// </summary>
        public string Iv { get; set; }

        public Crypter()
        {
            Encoding = Encoding.UTF8;
            Key = "TestKeyTestKey97";
            Iv = "TestKeyTestKey76";
        }

        /// <summary>
        /// 对字符串进行加密
        /// </summary>
        /// <param name="toEncrypt">要加密的字符串</param>
        /// <returns></returns>
        public string Encrypt(string toEncrypt)
        {
            if (string.IsNullOrEmpty(toEncrypt))
                return string.Empty;

            using (RijndaelManaged rm = new RijndaelManaged())
            using (ICryptoTransform encryptor = rm.CreateEncryptor(Encoding.GetBytes(Key), Encoding.GetBytes(Iv)))
            {
                byte[] toEncryptArray = Encoding.GetBytes(toEncrypt);
                byte[] resultArray = encryptor.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);
                return Convert.ToBase64String(resultArray, 0, resultArray.Length);
            }
        }

        /// <summary>
        /// 对字符串进行解密
        /// </summary>
        /// <param name="key">指定密钥,密钥使用UTF8编码成byte[]后长度必须为16 !</param>
        /// <param name="iv">指定向量,向量使用UTF8编码成byte[]后长度必须为16 !</param>
        /// <param name="toDecrypt">要解密的字符串</param>
        /// <returns></returns>
        public string Decrypt(string toDecrypt)
        {
            if (string.IsNullOrWhiteSpace(toDecrypt))
                return string.Empty;

            using (RijndaelManaged rm = new RijndaelManaged())
            using (ICryptoTransform decryptor = rm.CreateDecryptor(Encoding.GetBytes(Key), Encoding.GetBytes(Iv)))
            {
                byte[] toDecryptArray = Convert.FromBase64String(toDecrypt);//注意,不是Encoding了!
                byte[] resultArray = decryptor.TransformFinalBlock(toDecryptArray, 0, toDecryptArray.Length);
                return Encoding.GetString(resultArray);
            }
        }
    }
}
