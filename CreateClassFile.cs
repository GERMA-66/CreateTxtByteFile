/*
 ************************************
 * 
 * 描 述: 创建类文件和Txt
 * Created By WZH
 * Created Time: 2020-10-24 17:15
 * 
 ************************************
 */
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreateByteFile
{
    class CreateClassFile
    {
        private static CreateClassFile _instance;

        public static CreateClassFile instance
        {
            get { if (_instance == null) _instance = new CreateClassFile(); return _instance; }
        }


        /* 创建txt文件 */
        public void createTxtFile(string codeName)
        {
            this.createFolder(ParseXmlFile.instance.txtPath);
            string newTxtPath = ParseXmlFile.instance.txtPath + codeName + ".txt";
            StreamWriter txtSw = new StreamWriter(newTxtPath, false, Encoding.UTF8);
            List<ConfigAttribute> attrList = ParseXmlFile.instance.configAttributeArr;
            for (int i = 0; i < attrList.Count; i++)
            {
                string txtData = "";
                for (int j = 0; j < attrList[i].dataArr.Count; j++)
                {
                    if (!attrList[i].getIsClinet(j)) continue;
                    for (int k = 0; k < attrList[i].dataArr[j].Count; k++)
                    {
                        txtData += attrList[i].dataArr[j][k];
                        txtData += "\t";
                    }
                    txtSw.WriteLine(txtData);
                }
            }
            txtSw.Flush();
            txtSw.Close();
            var time3 = System.DateTime.Now;
            Console.WriteLine("  " + codeName + "的 \"TXT文件\" 写入完成 " + time3);
        }

        /* 创建代码文件 */
        public void writeCodeFile(string excelName, string codeName)
        {
            this.createFolder(ParseXmlFile.instance.codeBasePath);
            string codeBasePath = ParseXmlFile.instance.codeBasePath + "$" + codeName + ".ts";
            StreamWriter codeSw = new StreamWriter(codeBasePath, false, Encoding.UTF8);
            string classDesc = "\n\t/**\n\t * " + excelName.Split('.')[0] + "\n\t */";
            string codeStartData = "module game_config {" + classDesc + "\n\texport class $" + codeName + " {\n";
            string codeInfoData = "";
            string notes = "\n\t\t/**\n\t\t * 读取txt文件填充数据, 返回配置ID\n\t\t */";
            string functionData = notes + "\n\t\tpublic fillData(row:Array<string>):any {\n\t\t\tvar filedArr:Array<string>;\n\n";
            List<ConfigAttribute> arr = ParseXmlFile.instance.configAttributeArr;
            for (int i = 0; i < arr.Count; i++)
            {
                codeInfoData = "";
                List<string> typeArr = arr[i].typeArr;
                List<string> nameArr = arr[i].nameArr;
                for (int j = 0; j < typeArr.Count; j++)
                {
                    string name = "";
                    string type = "";
                    if (!arr[i].getIsClinet(j)) continue;
                    if (arr[i].getType(j) == AttrType.Int.ToString())
                    {
                        type = "number";
                    }
                    else if (arr[i].getType(j) == AttrType.Int_Arr.ToString())
                    {
                        type = "Array<number>";
                    }
                    else if (arr[i].getType(j) == AttrType.String_Arr.ToString())
                    {
                        type = "Array<string>";
                    }
                    else if (arr[i].getType(j) == AttrType.Bool.ToString())
                    {
                        type = "boolean";
                    }
                    else
                    {
                        type = "string";
                    }
                    name = arr[i].nameArr[j];
                    codeInfoData += "\t\tpublic " + name + ": " + type + ";\n";
                }
            }
            functionData += this.createCode(arr[0]);
            functionData += "\n\t\t\treturn this." + arr[0].nameArr[0] + ";\n\t\t}";
            string endData = "\n\n\t}\n}";
            codeSw.WriteLine(codeStartData + codeInfoData + functionData + endData);
            codeSw.Flush();
            codeSw.Close();

            var time4 = System.DateTime.Now;
            Console.WriteLine("  " + excelName + "的 \"TS文件\" 写入完成 " + time4);

            this.createJsonFile();
        }
        public string createCode(ConfigAttribute info)
        {
            string functionData = "";
            int idx = 0;
            for (int i = 0; i < info.nameArr.Count; i++)
            {
                string name = info.nameArr[i];
                string data = "";
                if (!info.getIsClinet(i)) continue;
                if (info.getType(i) == AttrType.Int.ToString())
                {
                    data = "\t\t\tthis." + name + " = +row[" + idx + "];\n";
                }
                else if (info.getType(i) == AttrType.Int_Arr.ToString())
                {
                    data = "\t\t\tif (row[" + idx + "] == \"\") {\n\t\t\t\tthis." +
                        name + " = new Array<number>();\n\t\t\t} else {\n\t\t\t\tfiledArr = row[" + idx + "].split(\',\');\n\t\t\t\tthis." +
                        name + " = new Array<number>();\n\t\t\t\tfor (var i = 0;i < filedArr.length;i ++) {\n\t\t\t\t\tthis." +
                        name + "[i] = +filedArr[i];\n\t\t\t\t}\n\t\t\t}\n";
                }
                else if (info.getType(i) == AttrType.String_Arr.ToString())
                {
                    data = "\t\t\tif (row[" + idx + "] == \"\") {\n\t\t\t\tthis." +
                        name + " = new Array<string>();\n\t\t\t} else {\n\t\t\t\tfiledArr = row[" + idx + "].split(\',\');\n\t\t\t\tthis." +
                        name + " = new Array<string>();\n\t\t\t\tfor (var i = 0;i < filedArr.length;i ++) {\n\t\t\t\t\tthis." +
                        name + "[i] = filedArr[i];\n\t\t\t\t}\n\t\t\t}\n";
                }
                else if (info.getType(i) == AttrType.Bool.ToString())
                {
                    data = "\t\t\tthis." + name + " = row[" + idx + "] == \"1\" ? true : false;\n";
                }
                else
                {
                    data = "\t\t\tthis." + name + " = row[" + idx + "];\n";
                }
                functionData += data;
                idx++;
            }
            return functionData;
        }

        // 创建类文件
        public void createClassFile(string excelName, string codeName)
        {
            this.createFolder(ParseXmlFile.instance.classPath);
            string classPath = ParseXmlFile.instance.classPath + codeName + ".ts";
            if (File.Exists(classPath))
            {
                return;
            }
            StreamWriter codeSw = new StreamWriter(classPath, false, Encoding.UTF8);
            string classDesc = "\n/**\n * " + excelName.Split('.')[0] + "\n */";
            string codeStartData = classDesc + "\nclass " + codeName + " extends game_config.$" + codeName + " implements ITxtTable {\n";
            string endData = "\n\n}";
            codeSw.WriteLine(codeStartData + endData);
            codeSw.Flush();
            codeSw.Close();
        }
        // 创建json文件(还有第三方库所需文件夹)
        private void createJsonFile()
        {
            // 创建package.json文件
            this.createFolder(ParseXmlFile.instance.codePath);
            this.createFolder(ParseXmlFile.instance.codeLibPath);
            string codePath = ParseXmlFile.instance.codePath + "package.json";
            if (!File.Exists(codePath))
            {
                StreamWriter codeSw = new StreamWriter(codePath, false, Encoding.UTF8);
                string codeStartData = "{\n\t\"name\": \"game_config\",\n\t\"compilerVersion\": \"5.3.10\"";
                string endData = "\n}";
                codeSw.WriteLine(codeStartData + endData);
                codeSw.Flush();
                codeSw.Close();
            }

            // 创建tsconfig.json文件
            string codePath1 = ParseXmlFile.instance.codePath + "tsconfig.json";
            if (!File.Exists(codePath1))
            {
                StreamWriter codeSw1 = new StreamWriter(codePath1, false, Encoding.UTF8);
                string codeStartData1 = "{\n\t\"compilerOptions\": {\n\t\t\"target\": \"es5\",\n\t\t\"noImplicitAny\": false,\n\t\t\"sourceMap\": false,\n\t\t\"declaration\": true,\n\t\t\"outFile\": \"../libs/game_config/game_config.js\"\n\t},\n\t\"include\": [\n\t\t\"src\"\n\t]";
                string endData1 = "\n}";
                codeSw1.WriteLine(codeStartData1 + endData1);
                codeSw1.Flush();
                codeSw1.Close();
            }
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

        public void deleteFolder(string path)
        {
            if (Directory.Exists(path))
            {
                string[] filePathList = Directory.GetFiles(path);
                foreach (string filePath in filePathList)
                {
                    File.Delete(filePath);
                }
            }
        }
    }
}
