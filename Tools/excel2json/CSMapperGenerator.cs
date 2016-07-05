using System.Collections.Generic;
using System.IO;
using System.Text;

namespace excel2json
{
    class CSMapperGenerator
    {
        public static void SaveMapFile(List<string> mapList, string filePath, Encoding encoding)
        {
            //-- 创建代码字符串
            StringBuilder sb = new StringBuilder();

            string firstPartContent =
                "using System;\n" +
                "using System.Collections.Generic;\n" +
                "using Excel2Json;\n" +
                "\n" +
                "public class ConfigMapper\n" +
                "{\n" +
                "    private static Dictionary<string, Type> m_mapperDict;\n" +
                                "\n" +
                "    public static Dictionary<string, Type> GetMapperDict()\n" +
                "    {\n" +
                "        CheckDict();\n" +
                "        return m_mapperDict;\n" +
                "    }\n" +
            "\n" +
                "    private static void CheckDict()\n" +
                "    {\n" +
                "        if (m_mapperDict == null)\n" +
                "        {\n" +
                "            m_mapperDict = new Dictionary<string, Type>();\n ";

            string middlePartContent = null;
            foreach (var content in mapList)
            {
                middlePartContent += "            m_mapperDict.Add(" + '"' + content + '"' + ", typeof(" + content + "));\n";
            }

            string thirdPartContent =
                "        }\n" +
                "    }\n" +
                "}\n";

            sb.Append(firstPartContent);
            sb.Append(middlePartContent);
            sb.Append(thirdPartContent);

            //-- 保存文件
            using (FileStream file = new FileStream(filePath, FileMode.Create, FileAccess.Write))
            {
                using (TextWriter writer = new StreamWriter(file, encoding))
                    writer.Write(sb.ToString());
            }
        }
    }
}
