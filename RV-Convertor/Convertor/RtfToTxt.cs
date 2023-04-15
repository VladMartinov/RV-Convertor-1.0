using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace RV_Convertor.Convertor
{
    class RtfToTxt : IActionWTFile
    {
        /* Support class (ignor or how many chars need to skip) */
        private class StackEntry
        {
            public int NumberOfCharactersToSkip { get; set; }
            public bool Ignorable { get; set; }

            public StackEntry(int numberOfCharactersToSkip, bool ignorable)
            {
                NumberOfCharactersToSkip = numberOfCharactersToSkip;    /* How many skip */
                Ignorable = ignorable;      /* Need ignor? */
            }
        }

        /* Regex of start RTF-file                            |    1-st group and 2-nd       | |   3-rd group | |3-th gr | |5-th| |6-th |       '.' takes into account \n     Ignore the registr   */
        private static readonly Regex _rtfRegex = new Regex(@"\\([a-z]{1,32})(-?\d{1,10})?[ ]?|\\'([0-9a-f]{2})|\\([^a-z])|([{}])|[\r\n]+|(.)", RegexOptions.Singleline | RegexOptions.IgnoreCase);

        /* 
         * All special words in RTF-format 
         * More info: https://biblioscape.com/rtf15_spec.htm
         */
        private static readonly List<string> destinations = new List<string> {
        "aftncn","aftnsep","aftnsepc","annotation","atnauthor","atndate","atnicn","atnid",
        "atnparent","atnref","atntime","atrfend","atrfstart","author","background",
        "bkmkend","bkmkstart","blipuid","buptim","category","colorschememapping",
        "colortbl","comment","company","creatim","datafield","datastore","defchp","defpap",
        "do","doccomm","docvar","dptxbxtext","ebcend","ebcstart","factoidname","falt",
        "fchars","ffdeftext","ffentrymcr","ffexitmcr","ffformat","ffhelptext","ffl",
        "ffname","ffstattext","field","file","filetbl","fldinst","fldrslt","fldtype",
        "fname","fontemb","fontfile","fonttbl","footer","footerf","footerl","footerr",
        "footnote","formfield","ftncn","ftnsep","ftnsepc","g","generator","gridtbl",
        "header","headerf","headerl","headerr","hl","hlfr","hlinkbase","hlloc","hlsrc",
        "hsv","htmltag","info","keycode","keywords","latentstyles","lchars","levelnumbers",
        "leveltext","lfolevel","linkval","list","listlevel","listname","listoverride",
        "listoverridetable","listpicture","liststylename","listtable","listtext",
        "lsdlockedexcept","macc","maccPr","mailmerge","maln","malnScr","manager","margPr",
        "mbar","mbarPr","mbaseJc","mbegChr","mborderBox","mborderBoxPr","mbox","mboxPr",
        "mchr","mcount","mctrlPr","md","mdeg","mdegHide","mden","mdiff","mdPr","me",
        "mendChr","meqArr","meqArrPr","mf","mfName","mfPr","mfunc","mfuncPr","mgroupChr",
        "mgroupChrPr","mgrow","mhideBot","mhideLeft","mhideRight","mhideTop","mhtmltag",
        "mlim","mlimloc","mlimlow","mlimlowPr","mlimupp","mlimuppPr","mm","mmaddfieldname",
        "mmath","mmathPict","mmathPr","mmaxdist","mmc","mmcJc","mmconnectstr",
        "mmconnectstrdata","mmcPr","mmcs","mmdatasource","mmheadersource","mmmailsubject",
        "mmodso","mmodsofilter","mmodsofldmpdata","mmodsomappedname","mmodsoname",
        "mmodsorecipdata","mmodsosort","mmodsosrc","mmodsotable","mmodsoudl",
        "mmodsoudldata","mmodsouniquetag","mmPr","mmquery","mmr","mnary","mnaryPr",
        "mnoBreak","mnum","mobjDist","moMath","moMathPara","moMathParaPr","mopEmu",
        "mphant","mphantPr","mplcHide","mpos","mr","mrad","mradPr","mrPr","msepChr",
        "mshow","mshp","msPre","msPrePr","msSub","msSubPr","msSubSup","msSubSupPr","msSup",
        "msSupPr","mstrikeBLTR","mstrikeH","mstrikeTLBR","mstrikeV","msub","msubHide",
        "msup","msupHide","mtransp","mtype","mvertJc","mvfmf","mvfml","mvtof","mvtol",
        "mzeroAsc","mzeroDesc","mzeroWid","nesttableprops","nextfile","nonesttables",
        "objalias","objclass","objdata","object","objname","objsect","objtime","oldcprops",
        "oldpprops","oldsprops","oldtprops","oleclsid","operator","panose","password",
        "passwordhash","pgp","pgptbl","picprop","pict","pn","pnseclvl","pntext","pntxta",
        "pntxtb","printim","private","propname","protend","protstart","protusertbl","pxe",
        "result","revtbl","revtim","rsidtbl","rxe","shp","shpgrp","shpinst",
        "shppict","shprslt","shptxt","sn","sp","staticval","stylesheet","subject","sv",
        "svb","tc","template","themedata","title","txe","ud","upr","userprops",
        "wgrffmtfilter","windowcaption","writereservation","writereservhash","xe","xform",
        "xmlattrname","xmlattrvalue","xmlclose","xmlname","xmlnstbl",
        "xmlopen"   };

        /* All special characters in RTF-format */
        private static readonly Dictionary<string, string> specialCharacters = new Dictionary<string, string> {
        { "par", "\n" },
        { "sect", "\n\n" },
        { "page", "\n\n" },
        { "line", "\n" },
        { "tab", "\t" },
        { "emdash", "\u2014" },
        { "endash", "\u2013" },
        { "emspace", "\u2003" },
        { "enspace", "\u2002" },
        { "qmspace", "\u2005" },
        { "bullet", "\u2022" },
        { "lquote", "\u2018" },
        { "rquote", "\u2019" },
        { "ldblquote", "\u201C" },
        { "rdblquote", "\u201D" },  };

        /* Text conversion method */
        private static string StripRichTextFormat(string inputRtf)
        {
            /* If file empty */
            if (inputRtf == null)
            {
                return null;
            }

            string returnString;

            /* Initializing the stack */
            var stack = new Stack<StackEntry>();

            /* Whether this group (and all inside it) are "ignorable" */
            bool ignorable = false;

            /* Number of ASCII characters to skip after a unicode character */
            int ucskip = 1;

            /* Number of ASCII characters left to skip */
            int curskip = 0;

            /* Output buffer */
            var outList = new List<string>();

            /* Search for an RTF format header */
            MatchCollection matches = _rtfRegex.Matches(inputRtf);

            /* Is the pattern detected or not? */
            if (matches.Count > 0)
            {

                /*  */
                foreach (Match match in matches)
                {
                    /* We find the word? */
                    string word = match.Groups[1].Value;
                    /* We find the argument? */
                    string arg = match.Groups[2].Value;
                    /* We find the hex? */
                    string hex = match.Groups[3].Value;
                    /* We find the character (a-z)? */
                    string character = match.Groups[4].Value;
                    /* We find the brace ( '{' or '}' )? */
                    string brace = match.Groups[5].Value;
                    /* We find the tcharacter (\r \n)? */
                    string tchar = match.Groups[6].Value;

                    /* If we find a brace */
                    if (!String.IsNullOrEmpty(brace))
                    {
                        curskip = 0;
                        if (brace == "{")
                        {
                            /* Push state */
                            stack.Push(new StackEntry(ucskip, ignorable));
                        }
                        else if (brace == "}")
                        {
                            /* Pop state */
                            StackEntry entry = stack.Pop();
                            ucskip = entry.NumberOfCharactersToSkip;
                            ignorable = entry.Ignorable;
                        }
                    }
                    else if (!String.IsNullOrEmpty(character)) /* Check if this a letter */
                    {
                        curskip = 0;
                        if (character == "~")
                        {
                            if (!ignorable)
                            {
                                outList.Add("\xA0");
                            }
                        }
                        else if ("{}\\".Contains(character))
                        {
                            if (!ignorable)
                            {
                                outList.Add(character);
                            }
                        }
                        else if (character == "*")
                        {
                            ignorable = true;
                        }
                    }
                    else if (!String.IsNullOrEmpty(word))       /* If we find a word (Ex.: \foo) */
                    {
                        curskip = 0;
                        if (destinations.Contains(word))
                        {
                            ignorable = true;
                        }
                        else if (ignorable)
                        {
                        }
                        else if (specialCharacters.ContainsKey(word))
                        {
                            outList.Add(specialCharacters[word]);
                        }
                        else if (word == "uc")   /* UTF-coder */
                        {
                            ucskip = Int32.Parse(arg);
                        }
                        else if (word == "u")
                        {
                            int c = Int32.Parse(arg);
                            if (c < 0)
                            {
                                c += 0x10000;
                            }
                            /* 
                             * (Validating UTF32) A valid UTF32 value is between 0x000000 and 0x10ffff (inclusive) and should not have replacement code point values (0x00d800~0x00dfff). 
                             * More info: https://www.oreilly.com/library/view/rtf-pocket-guide/9781449302047/ch01.html#unicode_in_rtf (Use Ctrl+f and search \uc)
                             */
                            if (c >= 0x000000 && c <= 0x10ffff && (c < 0x00d800 || c > 0x00dfff))
                                outList.Add(Char.ConvertFromUtf32(c));
                            else outList.Add("?"); /* If this is not UTF32 */
                            curskip = ucskip;
                        }
                    }
                    else if (!String.IsNullOrEmpty(hex)) /* If we find a hex-code (Ex.: \'xx) */
                    {
                        if (curskip > 0)
                        {
                            curskip -= 1;
                        }
                        else if (!ignorable)
                        {
                            int c = Int32.Parse(hex, System.Globalization.NumberStyles.HexNumber);
                            if ((c > 191 && c < 256) || (c == 168 || c == 184))   /* Is this a russian symbol? More info: http://blog.kislenko.net/show.php?id=2045 */
                                outList.Add(Encoding.GetEncoding(1251).GetString(new byte[] { (byte)c })[0].ToString());
                            else outList.Add(Char.ConvertFromUtf32(c));
                        }
                    }
                    else if (!String.IsNullOrEmpty(tchar)) /* If we find a char (\n \r) */
                    {
                        if (curskip > 0)
                        {
                            curskip -= 1;
                        }
                        else if (!ignorable)
                        {
                            outList.Add(tchar);
                        }
                    }
                }
            }
            else
            {
                /* Didn't match the regex in the string */
                returnString = inputRtf;
            }

            /* Result string text */
            returnString = String.Join(String.Empty, outList.ToArray());

            return returnString;
        }

        /* IActionWTFile function */
        public string StartConvert(string inputFilePath)
        {
            string contents = File.ReadAllText(inputFilePath);    /* Reed all text in the RTF-file */
            string contentsTxt = RtfToTxt.StripRichTextFormat(contents);    /* Convert to txt format */

            string new_path = Path.GetFileName(inputFilePath);
            new_path = Path.GetDirectoryName(inputFilePath) + "\\" + new_path.Remove(new_path.Length - 4) + "_conv" + ".txt";
            File.WriteAllText(new_path, contentsTxt, Encoding.Default);  /* Ready! */

            return new_path;
        }

    }
}