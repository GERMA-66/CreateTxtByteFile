/*
 ************************************
 * 
 * 描 述:Zip
 * Created By WZH
 * Created Time: 2020-10-23 17:19
 * 
 ************************************
 */
using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreateByteFile
{
     class ZipMainManager
    {
        private static ZipMainManager _instance;

        public string rootPaht = "";
        public string zipName = "";
        private ZipArchive _archive;
        private FileStream _fs;

        public static ZipMainManager instance
        {
            get { if (_instance == null) _instance = new ZipMainManager(); return _instance; }
        }

        /*
         * @parme _rootPath 根目录
         * @parme zipFileName 要压缩文件的文件夹名字
         * @parme _zipName 生成压缩包名字
         */
        public void createZipFipe(string _rootPath, string zipFileName, string _zipName)
        {
            this.rootPaht = _rootPath;
            this.zipName = _zipName;
            //检查是否存在文件夹
            if (Directory.Exists(_rootPath) == false)
            {
                //创建pic文件夹
                Directory.CreateDirectory(_rootPath + zipFileName);
            }
            if (!File.Exists(_rootPath + _zipName))
            {
                ZipFile.CreateFromDirectory(_rootPath + zipFileName, _rootPath + _zipName);
            }
        }

        public bool hasFile(string fileName)
        {

            ZipArchive archive = ZipFile.Open(this.rootPaht + this.zipName, ZipArchiveMode.Update);
            ZipArchiveEntry entry = archive.GetEntry(fileName);
            return entry != null;
        }

        public void writeFile(string filePath, string fileName)
        {
            //if (this._fs == null)
            //{
            //    Console.WriteLine(this.rootPaht + this.zipName);
            //    this._fs = new FileStream(this.rootPaht + this.zipName, FileMode.Open);
            //}
            //if (this._archive == null)
            //{
            //    this._archive = new ZipArchive(this._fs, ZipArchiveMode.Update);
            //}
            //ZipArchiveEntry readmeEntry = this._archive.CreateEntry("Readme.txt");//filePath + fileName
            //using (StreamWriter writer = new StreamWriter(readmeEntry.Open()))
            //{
            //    writer.WriteLine("Information about this package.");
            //    writer.WriteLine("========================");
            //}

            using (FileStream zipToOpen = new FileStream(this.rootPaht + this.zipName, FileMode.Open))
            {
                using (ZipArchive archive = new ZipArchive(zipToOpen, ZipArchiveMode.Update))
                {
                    ZipArchiveEntry readmeEntry = archive.CreateEntry("Readme.txt");
                    using (StreamWriter writer = new StreamWriter(readmeEntry.Open()))
                    {
                        writer.WriteLine("Information about this package.");
                        writer.WriteLine("========================");
                    }
                }
            }
        }
    }
}
