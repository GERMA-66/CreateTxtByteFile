using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreateByteFile
{
    class Program
    {
        static void Main(string[] args)
        {
            ParseXmlFile.instance.init();
            ZipMainManager.instance.createZipFipe(ParseXmlFile.instance.zipPath, ParseXmlFile.instance.zipTablePath, "table.byte");
            CreateClassFile.instance.deleteFolder(ParseXmlFile.instance.txtPath);
        }
    }
}
