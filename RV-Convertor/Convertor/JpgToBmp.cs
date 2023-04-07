using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;

namespace RV_Convertor.Convertor
{
    class JpgToBmp : IActionWTFile
    {
        #region - Variable - 
        /* Header BitMap */
        const short HEADER_SIGN = 0x4D42;
        static uint fileSize = 14 + 40 + 100 * 100 * 3;
        static uint reserved = 0;
        static uint offset = 14 + 40;

        /* Title BitMap */
        static uint headerSize = 40;
        static uint[] dimensions = { 100, 100 };
        static ushort colorPlanes = 1;
        static ushort bpp = 24;
        static uint compression = 0;
        static uint imgSize = 100 * 100 * 3;
        static uint[] resolution = { 2795, 2795 };
        static uint pltColors = 0;
        static uint impColors = 0;

        static string fileName = "NewBmpFile.bmp";

        /* For bitwise manipulation */
        static MemoryStream ms = new MemoryStream();
        static BinaryWriter bmp = new BinaryWriter(ms);

        /* If JPG file have padding */
        static bool isWriteZero;
        #endregion

        /* Read all file info and save into the varibales */
        public static void setValue(string inpPathFile)
        {
            Bitmap bitmap = new Bitmap(inpPathFile);

            uint UWidth = (uint)bitmap.Width;
            uint UHeight = (uint)bitmap.Height;

            /* 24bpp RGB - common from JPG or JPEG */
            ushort UBpp = (ushort)Image.GetPixelFormatSize(bitmap.PixelFormat);
            bpp = UBpp;

            fileSize = 14 + 40 + UWidth * UHeight * 4;
            /* 3 - RxGxB */
            imgSize = UWidth * UHeight * 3;
            dimensions[0] = UWidth;
            dimensions[1] = UHeight;

            string new_path = Path.GetDirectoryName(inpPathFile);
            string new_name = Path.GetFileName(inpPathFile);
            fileName = new_path + "\\" + new_name.Remove(new_name.Length - 4) + "_conv" + ".bmp";
        }


        /* IActionWTFile function */
        public string StartConvert(string inpPathFile)
        {
            setValue(inpPathFile);

            /* Creating an array with dimension W x H (from R x G x B) */
            byte[,] arrI = new byte[dimensions[0] * dimensions[1], 3];

            /* This method write into the arrI all RGB bytes and say, if we have padding into the jpg picture */
            isWriteZero = getRGB(inpPathFile, 0, 0, (int)dimensions[0], (int)dimensions[1], arrI, 0, (int)dimensions[0]);

            /* Write all info into the BMP file (using memory stream and binary writer) */
            /* Header BMP */
            bmp.Write(HEADER_SIGN);
            bmp.Write(fileSize);
            bmp.Write(reserved);
            bmp.Write(offset);

            /* Title BMP */
            bmp.Write(headerSize);
            bmp.Write(dimensions[0]);
            bmp.Write(dimensions[1]);
            bmp.Write(colorPlanes);
            bmp.Write(bpp);
            bmp.Write(compression);
            bmp.Write(imgSize);
            bmp.Write(resolution[0]);
            bmp.Write(resolution[1]);
            bmp.Write(pltColors);
            bmp.Write(impColors);

            /* 
             * If we try bmp.Write((byte) 0), we will have an exception!
             * And then we create this variable.
             */
            byte zero = (byte)0;

            /* 
             * Here we write the entire array of rgb data to our file 
             * (from bottom to top, from left to right, otherwise the 
             * image will be inverted).
             * */
            for (int i = (int)dimensions[1] - 1; i >= 0; i--)
            {
                for (int j = 0; j < (int)dimensions[0]; j++)
                {
                    for (int b = 0; b < 3; b++)
                    {
                        bmp.Write(arrI[j + i * (int)dimensions[0], b]);
                    }
                }

                if (isWriteZero) /* We have padding? If yes - write. */
                {
                    bmp.Write(zero);
                }
            }

            /* Save bmp file */
            new Bitmap(ms).Save(fileName);

            /* Reset MemoryStream */
            ms.SetLength(0);

            return fileName;
        }

        /* Method for reading rgb array from JPG. */
        private static bool getRGB(string inpPathFile, int startX, int startY, int w, int h, byte[,] rgbArray, int offset, int scansize)
        {
            Bitmap image = new Bitmap(inpPathFile);

            bool isLastZero = false;

            /* RGB */
            const int PixelWidth = 3;
            /* If we try 32bpp we need the alpha (it is PNG) */
            const PixelFormat PixelFormat = PixelFormat.Format24bppRgb;

            /* We check the correctness of the specified data */
            if (image == null) throw new ArgumentNullException("image");
            if (rgbArray == null) throw new ArgumentNullException("rgbArray");
            if (startX < 0 || startX + w > image.Width) throw new ArgumentOutOfRangeException("startX");
            if (startY < 0 || startY + h > image.Height) throw new ArgumentOutOfRangeException("startY");
            if (w < 0 || w > scansize || w > image.Width) throw new ArgumentOutOfRangeException("w");
            if (h < 0 || (rgbArray.Length < offset + h * scansize) || h > image.Height) throw new ArgumentOutOfRangeException("h");

            /* We lock the image in memory thereby gaining access to pixels */
            BitmapData data = image.LockBits(new Rectangle(startX, startY, w, h), System.Drawing.Imaging.ImageLockMode.ReadOnly, PixelFormat);
            try
            {
                /* Create an array to store the rgb value of the row */
                byte[] pixelData = new Byte[data.Stride];

                /* If Width of file more then true width (without padding) */
                if (pixelData.Length > data.Width * 3)
                    isLastZero = true;

                /* Start scaning the picture */
                for (int scanline = 0; scanline < data.Height; scanline++)
                {
                    /* Save into the pixelData rgb array from row */
                    Marshal.Copy(data.Scan0 + (scanline * data.Stride), pixelData, 0, data.Stride);
                    for (int pixeloffset = 0; pixeloffset < data.Width; pixeloffset++)
                    {
                        /* Save all r g b value from pixelData into rgbArray */
                        rgbArray[offset + (scanline * scansize) + pixeloffset, 0] = pixelData[pixeloffset * PixelWidth];        // R
                        rgbArray[offset + (scanline * scansize) + pixeloffset, 1] = pixelData[pixeloffset * PixelWidth + 1];    // G
                        rgbArray[offset + (scanline * scansize) + pixeloffset, 2] = pixelData[pixeloffset * PixelWidth + 2];    // B
                    }
                }
            }
            finally
            {
                /* If we have finished the work, we will unlock bits */
                image.UnlockBits(data);
            }

            return isLastZero;
        }
    }
}
