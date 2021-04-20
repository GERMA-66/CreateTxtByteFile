/*
 ************************************
 * 
 * 描 述:
 * Created By WZH
 * Created Time: %time%
 * 
 ************************************
 */
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace CreateByteFile
{
    struct ConfigInfo
    {
        /* excel名字 */
        public string key;
        /* 生成的txt文本名字和配置表类名 */
        public string value;
    }

    enum AttrType
    {
        Int,
        String,
        Int_Arr,
        String_Arr,
        Bool,
    }
    struct ConfigAttribute
    {
        // 字段名字
        public List<string> nameArr;
        // 使用字段 c - 表示客户端 cs、s - 表示服务器
        public List<string> usedArr;
        // 类型
        public List<string> typeArr;
        // 数据
        public List<List<string>> dataArr;
        //  服务器是否使用
        public bool getIsClinet(int index)
        {
            return usedArr[index] == "cs" || usedArr[index] == "c";
        }
        /* 获取类型 */
        public string getType(int index)
        {
            if (this.typeArr[index] == "string[]")
            {
                return AttrType.String_Arr.ToString();
            }
            else if (this.typeArr[index] == "int[]")
            {
                return AttrType.Int_Arr.ToString();
            }
            else if (this.typeArr[index] == "int")
            {
                return AttrType.Int.ToString();
            }
            else if (this.typeArr[index] == "bool")
            {
                return AttrType.Bool.ToString();
            }
            else
            {
                return AttrType.String.ToString();
            }
        }
    }

    class ParseXmlFile
    {
        private static ParseXmlFile _instance;

        public static ParseXmlFile instance
        {
            get { if (_instance == null) _instance = new ParseXmlFile(); return _instance; }
        }

        private XmlDocument _document;

        /* 表路径 */
        public string excelPath = "";
        /* txt路径 */
        public string txtPath = "";
        /* 代码第三方库路径 */
        public string codePath = "";
        /* .d.ts文件路径 */
        public string codeLibPath = "";
        /* 代码路径 */
        public string codeBasePath = "";
        /* 可执行文件路径 */
        public string exePath = "";
        /* 类路径 */
        public string classPath = "";
        /* zip文件路径(根路径) */
        public string zipPath = "";
        /* 要压缩表的文件夹名字 */
        public string zipTablePath = "";
        /* 配置表信息 */
        public List<ConfigInfo> configInfoArr;

        public List<ConfigAttribute> configAttributeArr;

        // 初始化
        public void init()
        {
            configInfoArr = new List<ConfigInfo>();
            configAttributeArr = new List<ConfigAttribute>();


            this.readXmlFile();
            for (int i = 0; i < configInfoArr.Count; i++)
            {
                var startYime = System.DateTime.Now;
                Console.WriteLine(" \n ----- 开始导出  " + configInfoArr[i].key + "   " + startYime + "\n");

                this.getExcelFile(configInfoArr[i].key, configInfoArr[i].value);
                CreateClassFile.instance.writeCodeFile(configInfoArr[i].key, configInfoArr[i].value);
                CreateClassFile.instance.createClassFile(configInfoArr[i].key, configInfoArr[i].value);

                var endTime = System.DateTime.Now;
                Console.WriteLine("\n ----- " + configInfoArr[i].key + "导出完成 " + endTime + "\n");
            }
            this.autoExeFile();
            Console.WriteLine("               ----- 导出表数据成功,按任意键退出 -----              ");
            Console.ReadKey(true);
        }

        // 读取并解析xml文件
        protected void readXmlFile()
        {
            this._document = new XmlDocument();
            this._document.Load("./settingConfig.xml");
            XmlElement root = this._document.DocumentElement;
            this.excelPath = this.getPath(root, "/setting/pathSettings/ExcelPath");
            this.txtPath = this.getPath(root, "/setting/pathSettings/TxtPath");
            this.codePath = this.getPath(root, "/setting/pathSettings/CodePath");
            this.codeBasePath = this.getPath(root, "/setting/pathSettings/CodeBasePath");
            this.codeLibPath = this.getPath(root, "/setting/pathSettings/CodeLibPath");
            this.classPath = this.getPath(root, "/setting/pathSettings/CodeClassPath");
            this.exePath = this.getPath(root, "/setting/pathSettings/ExePath");
            this.zipPath = this.getPath(root, "/setting/pathSettings/ZipPath");
            this.zipTablePath = this.getPath(root, "/setting/pathSettings/ZipTablePath");

            XmlNode cfgInfo = root.SelectSingleNode("excelSettings");
            XmlNodeList cfgInfoList = cfgInfo.ChildNodes;
            foreach (XmlNode node in cfgInfoList)
            {
                if (node.Name == "add")
                {
                    XmlElement elem = (XmlElement)node;
                    string key = elem.GetAttribute("key").ToString();
                    string value = elem.GetAttribute("value").ToString();
                    ConfigInfo configInfo = new ConfigInfo();
                    configInfo.key = key;
                    configInfo.value = value;
                    configInfoArr.Add(configInfo);
                }
            }
            Console.WriteLine("\n\n      ------- 解析xml文件成功 -------      \n");
        }

        // 获取excel文件并存到列表里面
        private void getExcelFile(string excelName, string codeName)
        {
            this.createFolder(this.excelPath);
            /* .xls文件 */
            string xlsSet = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + this.excelPath + excelName + ";" + "Extended Properties='Excel 8.0;HDR=No;IMEX=1'";
            /* .xlsx文件 */
            string xlsxSet = "Provider=Microsoft.ACE.OLEDB.12.0;" + "Data Source=" + this.excelPath + excelName + ";" + ";Extended Properties=\"Excel 12.0;HDR=YES;IMEX=1\"";
            DataSet ds = new DataSet();
            OleDbDataAdapter oada = new OleDbDataAdapter("select * from [Sheet1$]", xlsxSet);
            try
            {
                oada.Fill(ds);
            }
            catch (Exception e)
            {
                Console.WriteLine("\n" + excelName + "获取失败" + "\n");
                return;
            }
            int rows = ds.Tables[0].Rows.Count;
            int cols = ds.Tables[0].Columns.Count;
            configAttributeArr.Clear();

            var time1 = System.DateTime.Now;
            Console.WriteLine(" ----- 读取配置文件 " + excelName + "      " + time1);
            for (int i = 0; i < rows; i++)
            {
                if (ds.Tables[0].Rows[i].ItemArray[0].ToString() == "")
                {
                    break;
                }

                if (i >= 3)
                {
                    ConfigAttribute configAttribute = new ConfigAttribute();
                    configAttribute.nameArr = new List<string>();
                    configAttribute.usedArr = new List<string>();
                    configAttribute.typeArr = new List<string>();
                    configAttribute.dataArr = new List<List<string>>();
                    List<string> list = new List<string>();
                    for (int j = 0; j < cols; j++)
                    {
                        string used = ds.Tables[0].Rows[2][j].ToString();
                        if (used == "s") continue;
                        configAttribute.usedArr.Add(used);
                        string name = ds.Tables[0].Rows[0][j].ToString();
                        configAttribute.nameArr.Add(name);
                        string type = ds.Tables[0].Rows[1][j].ToString();
                        configAttribute.typeArr.Add(type);
                        string data = ds.Tables[0].Rows[i][j].ToString();
                        list.Add(data);
                    }
                    configAttribute.dataArr.Add(list);
                    configAttributeArr.Add(configAttribute);
                }
            }
            
            CreateClassFile.instance.createTxtFile(codeName);

        }


        // 获取路径
        private string getPath(XmlElement root, string nodeName)
        {
            XmlNodeList nodeList = root.SelectNodes(nodeName);
            return nodeList[0].InnerText;
        }

        // 创建文件夹
        private void createFolder(string path)
        {
            //检查是否存在文件夹
            if (System.IO.Directory.Exists(path) == false)
            {
                //创建pic文件夹
                System.IO.Directory.CreateDirectory(path);
            }
        }

        private void autoExeFile()
        {
            // 创建bat文件
            string codePath1 = this.exePath + "buildLib.bat";
            if (!File.Exists(codePath1))
            {
                StreamWriter codeSw1 = new StreamWriter(codePath1, false, Encoding.UTF8);
                string info = "@echo off\n\negret build game_config\n\n@echo off\n\negret build -e";
                codeSw1.WriteLine(info);
                codeSw1.Flush();
                codeSw1.Close();
            }

            Process proc = new Process();
            string targetDir = string.Format(@"" + this.exePath);

            proc.StartInfo.WorkingDirectory = targetDir;
            proc.StartInfo.FileName = "buildLib.bat";
            proc.StartInfo.Arguments = string.Format("10");

            proc.Start();
            proc.WaitForExit();
        }
    }
}
