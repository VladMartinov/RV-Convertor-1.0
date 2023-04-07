using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;

namespace RV_Convertor.Convertor
{
    class BmpToJpg : IActionWTFile
    {
        /* IActionWTFile function */
        public string StartConvert(string inpPathFile)
        {
            /* Create new path */
            string new_path = Path.GetDirectoryName(inpPathFile);
            string new_name = Path.GetFileName(inpPathFile);
            string fileName = new_path + "\\" + new_name.Remove(new_name.Length - 4) + "_conv" + ".jpg";

            /* No sorted because we have reversed picture when we read the byte array */
            byte[] notSortedArr;

            int UWidth;
            int UHeight;

            /* Start read the file */
            using (var stream = new FileStream(inpPathFile, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (var reader = new BinaryReader(stream))
            {
                /* 18 byte - Width */
                reader.BaseStream.Position = 18;
                UWidth = reader.ReadUInt16();

                /* 22 byte - Width */
                reader.BaseStream.Position += 2;
                UHeight = reader.ReadUInt16();

                /* 54 byte - the beginning of the palette */
                reader.BaseStream.Position = 54;

                /* Read to end */
                notSortedArr = reader.ReadBytes((int)stream.Length);
            }
         
            /* If we have padding */
            bool isPadding = false;
            if (notSortedArr.Length > UWidth * UHeight * 3) isPadding = true;
            
            /* This array from sorted RGB value */
            byte[] finalArrByte = new byte[notSortedArr.Length];

            /* From correct posittion of rgb into the array */
            int CountZero = UHeight-1;
            /* To start writing to the sorted array from the very beginning ('i' starts from the end) */
            int CountIter = 0;

            /* Sorting all rgb */
            for (int i = UHeight - 1; i >= 0; i--)
            {
                for (int j = 0; j < UWidth * 3; j++)
                {
                    finalArrByte[CountIter] = isPadding ? notSortedArr[(j + i * UWidth * 3) + CountZero] : notSortedArr[j + i * UWidth * 3];

                    CountIter++;
                }
                if (isPadding)
                {
                    finalArrByte[CountIter] = 0;
                    CountZero--;
                    CountIter++;
                }
            }

            /* Create the bmp picture with 24bppRGB */
            Bitmap pic = new Bitmap(UWidth, UHeight, System.Drawing.Imaging.PixelFormat.Format24bppRgb);

            /* Locking the image in memory to access it */
            BitmapData bmpData = pic.LockBits(
                       new Rectangle(0, 0, pic.Width, pic.Height),
                       ImageLockMode.WriteOnly, pic.PixelFormat);

            /* Copy the data from the byte array into BitmapData.Scan0 */
            Marshal.Copy(finalArrByte, 0, bmpData.Scan0, finalArrByte.Length);


            /* Unlock the pixels */
            pic.UnlockBits(bmpData);

            /* Saving the resulting image */
            pic.Save(fileName);

            return fileName;
        }

    }
}
