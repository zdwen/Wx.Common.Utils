using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SevenZip;
using System.IO;

namespace Wx.Common.Utils
{
    public class WxCompressor
    {
        /// <summary>
        /// 【zdwen 2014-8-27-231153】压缩完成。
        /// </summary>
        public event Action CompressFinished;
        /// <summary>
        /// 【zdwen 2014-8-27-231146】压缩过程中的进度事件。
        /// </summary>
        public event Action<byte> Compressing;

        SevenZipCompressor _compressor;

        /// <summary>
        /// 【zdwen 2014-4-17-215552】默认唯一构造函数。
        /// </summary>
        /// <param name="libraryPath">【zdwen 2014-4-17-215629】需要是从codeplex上面下载下来的那个7z.dll(7z64.dll)。
        /// 如果是没有这个东东，在初始化Compressor的时候并不会报异常，当你真正开始压缩的时候会报错提示说：
        /// Can not load 7-zip library or internal COM error! Message: DLL file does not exist.
        /// </param>
        public WxCompressor(string libraryPath)
        {
            #region InitLibraryPath
            if (!File.Exists(libraryPath))
                throw new Exception(string.Format("【zdwen 2014-4-17-210425】指定的LibraryPath的路径：{0}不存在！！请检查。", libraryPath));

            if (string.IsNullOrWhiteSpace(libraryPath))
                throw new Exception("【zdwen 2014-4-17-205538】自定义异常，必须制定LibraryPath，且是从CodePlex上面down下来的那个7z.dll(7z64.dll)。");

            SevenZipCompressor.SetLibraryPath(libraryPath);
            #endregion

            _compressor = new SevenZipCompressor()
            {
                ArchiveFormat = OutArchiveFormat.SevenZip,
                CompressionLevel = CompressionLevel.Ultra,
                CompressionMethod = CompressionMethod.Default,
            };

            _compressor.CompressionFinished += new EventHandler<EventArgs>(compressor_CompressionFinished);
            _compressor.Compressing += new EventHandler<ProgressEventArgs>(compressor_Compressing);
        }

        /// <summary>
        /// 【zdwen 2014-4-17-210936】文件压缩
        /// </summary>
        /// <param name="destFileFullPath">目标文件名</param>
        /// <param name="password">密码</param>
        /// <param name="srcFileFullPath">源文件全路径</param>
        /// <param name="volumeSize">压缩卷大小（单位是MB）</param>
        public void CompressFile(string srcFileFullPath, string destFileFullPath, string password, int? volumeSize = null)
        {
            if (volumeSize.HasValue)
                _compressor.VolumeSize = volumeSize.Value * 1024 * 1024;

            if (string.IsNullOrWhiteSpace(password))
                _compressor.CompressFiles(destFileFullPath, srcFileFullPath);
            else
                _compressor.CompressFilesEncrypted(destFileFullPath, password, srcFileFullPath);
        }

        void compressor_Compressing(object sender, ProgressEventArgs e)
        {
            ///【zdwen 2014-8-27-231136】e.PercentDone其实是小于等于100的整数，可以拿来直接构造百分比。
            if (Compressing != null)
                Compressing.Invoke(e.PercentDone);
        }

        void compressor_CompressionFinished(object sender, EventArgs e)
        {
            if (CompressFinished != null)
                CompressFinished.Invoke();
        }
    }
}
