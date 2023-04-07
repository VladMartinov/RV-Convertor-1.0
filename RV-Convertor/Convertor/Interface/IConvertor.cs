namespace RV_Convertor.Convertor
{
    public interface IConvertor
    {
        /* ENG - You can clearly see which methods 
         * and which formats can be converted;
         * 
         * RU - Вы можете четко видеть, какие методы 
         * и в какие форматы можно конвертировать */

        void StartTxtToRtf();
        void StartRtfToTxt();
        void StartJpgToBmp();
        void StartBmpToJpg();

    }
}
