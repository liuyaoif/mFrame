using System;
using System.IO;
using System.Data;
using System.Text;
using System.Collections.Generic;

namespace excel2json
{
    /// <summary>
    /// 根据表头，生成C#类定义数据结构
    /// 表头使用三行定义：字段名称、字段类型、注释
    /// </summary>
    class CSDefineGenerator
    {
        private struct FieldDef
        {
            public string name;
            public string type;
            public string comment;
            public string platform;
        }

        List<FieldDef> m_fieldList;

        public string ClassComment
        {
            get;
            set;
        }

        public CSDefineGenerator(DataTable sheet, int headRowCount)
        {
            //-- First Row as Column Name
            if (sheet.Rows.Count < headRowCount)
                return;

            m_fieldList = new List<FieldDef>();
            DataRow typeRow = sheet.Rows[0];
            DataRow platformRow = sheet.Rows[1];
            DataRow commentRow = sheet.Rows[2];

            foreach (DataColumn column in sheet.Columns)
            {
                if (typeRow[column].ToString() == "")
                {
                    continue;
                }

                FieldDef field;
                field.name = column.ToString();

                string typeStr = typeRow[column].ToString();

                if (typeStr == "bool[]")//bool数组改为int数组
                {
                    typeStr = "int[]";
                }
                field.type = typeStr;

                string platform = (platformRow[column].ToString() == "") ? "all" : platformRow[column].ToString();
                field.platform = platform;
                field.comment = commentRow[column].ToString();

                m_fieldList.Add(field);
            }
        }

        public void SaveToFile(string filePath, Encoding encoding)
        {
            if (m_fieldList == null)
                throw new Exception("CSDefineGenerator内部数据为空。");

            string defName = Path.GetFileNameWithoutExtension(filePath);

            //-- 创建代码字符串
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("// Auto Generated Code By excel2json");
            sb.AppendLine();

            //excel源文件名称
            if (this.ClassComment != null)
            {
                sb.AppendLine(this.ClassComment);
            }

            //Namespace
            sb.AppendFormat("namespace Excel2Json");
            sb.AppendLine();
            sb.AppendLine("{");

            //Class name
            //sb.AppendFormat("\tpublic class {0}\r\n{{", defName);
            sb.AppendFormat("\tpublic struct {0}{1}\r", defName, " : IConfigData");
            sb.AppendLine("\t{");

            sb.AppendLine("\t\tpublic int GetId() {return ID;} ");

            foreach (FieldDef field in m_fieldList)
            {
                if (field.platform == "all" ||
                    field.platform == "c")
                {
                    string srt = "\t/*" + field.comment + "*/\n" + "\t\tpublic " + field.type + " " + field.name + ";";// + "{set; get;}";
                    sb.AppendLine(srt);
                }

                sb.AppendLine();
            }
            sb.AppendLine("\t}");
            sb.AppendLine();
            sb.Append('}');
            sb.AppendLine();
            sb.AppendLine("// End of Auto Generated Code");

            //-- 保存文件
            using (FileStream file = new FileStream(filePath, FileMode.Create, FileAccess.Write))
            {
                using (TextWriter writer = new StreamWriter(file, encoding))
                    writer.Write(sb.ToString());
            }
        }
    }
}
