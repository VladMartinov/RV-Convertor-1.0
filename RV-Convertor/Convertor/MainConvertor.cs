namespace RV_Convertor.Convertor
{
    class MainConvertor : IConvertor
    {
        #region - Class Objects -
        TxtToRtf TxtToRtf = new TxtToRtf();
        RtfToTxt RtfToTxt = new RtfToTxt();
        JpgToBmp JpgToBmp = new JpgToBmp();
        BmpToJpg BmpToJpg = new BmpToJpg();
        #endregion

        /* ENG - Info about input file 
           RU - Информация о входном файле */
        private string inputFilePath;
        public string InputFilePath
        {
            get
            {
                return this.inputFilePath;
            }
            set
            {
                this.inputFilePath = value;
            }
        }

        /* ENG - Info about output file 
           RU - Информация о выходном файле */
        private string outputFilePath;
        public string OutputFilePath
        {
            get
            {
                return this.outputFilePath;
            }
        }


        /* ENG - Calling file conversion methods 
           RU - Вызов методов преобразования файлов */
        public void StartTxtToRtf() { this.outputFilePath = TxtToRtf.StartConvert(inputFilePath); }
        public void StartRtfToTxt() { this.outputFilePath = RtfToTxt.StartConvert(inputFilePath); }
        public void StartJpgToBmp() { this.outputFilePath = JpgToBmp.StartConvert(inputFilePath); }
        public void StartBmpToJpg() { this.outputFilePath = BmpToJpg.StartConvert(inputFilePath); }
    }
}
