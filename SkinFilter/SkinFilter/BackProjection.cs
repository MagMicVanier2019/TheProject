using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;

namespace SkinFilter
{
    public static class BackProjection
    {
        public static Bitmap BackProject(Bitmap bmp, int[] HueRange, int[] SaturationRange)
        {
            Emgu.CV.Image<Bgr, Byte> Mask = new Image<Bgr, Byte>(bmp);                                     //Image Datatype switch
            Mat Copy = new Mat();                                                                          //Result Mat type
            bool useUMat;                                                                                  //bool for Mat Check
            using (InputOutputArray ia = Copy.GetInputOutputArray())                                       //Determine Mask type
                useUMat = ia.IsUMat;                                                                       //If Mat, use Mat
            using (IImage hsv = useUMat ? (IImage)new UMat() : (IImage)new Mat())                          //Mat Image Copies (Hue)
            using (IImage s = useUMat ? (IImage)new UMat() : (IImage)new Mat())                            //Mat Image Copies (Saturation)
            {
                CvInvoke.CvtColor(Mask, hsv, ColorConversion.Bgr2Hsv);                                     //Convert Image to Hsv
                CvInvoke.ExtractChannel(hsv, Copy, 0);                                                     //Extract Hue channel from Hsv
                CvInvoke.ExtractChannel(hsv, s, 1);                                                        //Extract Saturation channel from Hsv
                                                                                                           //the mask for hue less than 20 or larger than 160
                using (ScalarArray lower = new ScalarArray(HueRange[0]))                                   //hue min
                using (ScalarArray upper = new ScalarArray(HueRange[1]))                                   //hue max
                    CvInvoke.InRange(Copy, lower, upper, Copy);                                            //Check Ranges
                CvInvoke.BitwiseNot(Copy, Copy);                                                           //If ranges dont line up, fade to black
                                                                                                           //s is the mask for saturation of at least 10, this is mainly used to filter out white pixels
                CvInvoke.Threshold(s, s, SaturationRange[0], SaturationRange[1], ThresholdType.Binary);    //saturation check
                CvInvoke.BitwiseAnd(Copy, s, Copy, null);                                                  //If saturation and hue match requirements, place in mask

            }
            return Copy.Bitmap;
        }

        public static Bitmap SuperPositionedImage(Bitmap Mask, Bitmap StandardImage)
        {
            Image<Hsv, Byte> GrayOrigin = new Image<Gray, Byte>(StandardImage).Convert<Hsv,Byte>();
            Image<Hsv, Byte> HSVOrigin = new Image<Hsv, Byte>(StandardImage);
            Image<Gray, Byte> MaskImage = new Image<Gray, Byte>(Mask);
            Mat ResultHolder = new Mat();
            CvInvoke.BitwiseAnd(GrayOrigin, HSVOrigin, ResultHolder, MaskImage);
            Mat TrueResultHolder = new Mat();
            CvInvoke.Add(GrayOrigin, ResultHolder, TrueResultHolder, MaskImage);
            
            return TrueResultHolder.Bitmap;
        }
    }
}
