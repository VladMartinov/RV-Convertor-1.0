using System;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace RV_Convertor.Convertor
{

    class TxtToRtf : IActionWTFile
    {

        /* Save the modified text to a new file */
        private static void Save_RTF_file(string pFilePath, string pRTFText)
        {
            try
            {
                using (RichTextBox RTB = new RichTextBox())
                {
                    /* Add the modified text to the file */
                    RTB.Rtf = pRTFText;

                    /* Save the file */
                    RTB.SaveFile(pFilePath, RichTextBoxStreamType.RichText);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /* Convert the main text of a txt file to rtf format */
        private static string ConvertToRtf(string text)
        {
            /* Using standart template from WiKi */
            StringBuilder sb = new StringBuilder(@"{\rtf1\ansi\ansicpg1250\deff0{\fonttbl\f0\fswiss Helvetica;}\f0\pard ");

            /* We run through the entire text and translate it into rtf format */
            foreach (char character in text)
            {
                if (character <= 0x7f)
                {
                    /* Escaping rtf characters */
                    switch (character)
                    {
                        case '\\':
                        case '{':
                        case '}':
                            sb.Append('\\');
                            break;
                        case '\r':
                            sb.Append("\\par");
                            break;
                    }

                    sb.Append(character);
                }
                /* Converting special characters */
                else
                {
                    sb.Append("\\u" + Convert.ToUInt32(character) + "?");
                }
            }
            sb.Append("}");

            return sb.ToString();
        }

        /* IActionWTFile function */
        public string StartConvert(string inputFilePath)
        {
            /* New path to new file */
            string new_path = Path.GetFileName(inputFilePath);
            new_path = Path.GetDirectoryName(inputFilePath) + "\\" + new_path.Remove(new_path.Length - 4) + "_conv" + ".rtf";

            /* Get first info about Encoding */
            Encoding encoding;
            Stream fs = new FileStream(inputFilePath, FileMode.Open);
            using (StreamReader sr = new StreamReader(fs, true))
            {
                encoding = sr.CurrentEncoding;
            }

            /* Get second (additional) info about Encoding */
            var bom = new byte[4];
            using (var file = new FileStream(inputFilePath, FileMode.Open, FileAccess.Read))
            {
                file.Read(bom, 0, 4);
            }

            /* Analyze the BOM */
            if (bom[0] == 0x2b && bom[1] == 0x2f && bom[2] == 0x76) encoding = Encoding.UTF7;
            else if (bom[0] == 0xef && bom[1] == 0xbb && bom[2] == 0xbf) encoding = Encoding.UTF8;
            else if (bom[0] == 0xff && bom[1] == 0xfe && bom[2] == 0 && bom[3] == 0) encoding = Encoding.UTF32; //UTF-32LE
            else if (bom[0] == 0xff && bom[1] == 0xfe) encoding = Encoding.Unicode; //UTF-16LE
            else if (bom[0] == 0xfe && bom[1] == 0xff) encoding = Encoding.BigEndianUnicode; //UTF-16BE
            else if (bom[0] == 0 && bom[1] == 0 && bom[2] == 0xfe && bom[3] == 0xff) encoding = new UTF32Encoding(true, true);  //UTF-32BE
            
            /* Read all text from a file */
            string contents;
            using (StreamReader reader = new StreamReader(inputFilePath, encoding))
            {
                contents = reader.ReadToEnd();
            }

            /* If Encoding = ANSI (windows-1251, windows-1252, ...) */
            if (contents.Contains("�"))
                using (StreamReader reader = new StreamReader(inputFilePath, Encoding.Default))
                    contents = reader.ReadToEnd();

            /* Convert text to RTF-format and call the function to create an output file in RTF-format */
            string contentsRtf = ConvertToRtf(contents);
            Save_RTF_file(new_path, contentsRtf);

            return new_path;
        }

    }

}