#region using
using System.Collections.Generic;
using System.Drawing;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;

using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VMath;
using System;
using VVVV.Utils.VColor;
using VVVV.CV.Core;

#endregion

namespace VVVV.CV.Nodes
{
	[FilterInstance("FloydSteinbergDither", Help = "Floyd Steinberg Dithering - Grayscale Version")]
    public class FloydSteinbergDitherInstance : IFilterInstance
	{

        static byte asByte(int value)
        {
            //return (byte)value;
            if (value > 255)
                return 255;
            else if (value < 0)
                return 0;
            else
                return (byte)value;
        }

        private byte FThreshold = 128;

        [Input("Threshold", MinValue = 0.0, MaxValue = 1.0, DefaultValue = 0.5)]
        public Double Threshold
        {
            set
            {
                FThreshold = (byte)(value * 255.0);
            }
        }

		public override void Allocate()
		{
            FOutput.Image.Initialise(FInput.ImageAttributes.Size, TColorFormat.L8);
		}

		public override void Process()
		{
			// Whenever you access the pixels directly of FInput
			// you must lock it for reading using 
			if (!FInput.LockForReading()) return;
            
            //?? FInput.Image.GetImage(TColorFormat.L8, FOutput.Image);

            ImageUtils.CopyImageConverted(FInput.Image, FOutput.Image);
            FInput.ReleaseForReading(); // and this after you've finished with FImage

            ProcessFloydSteinberg();

			FOutput.Send();
		}

        private unsafe void ProcessFloydSteinberg()
        {
            byte* data = (byte*) FOutput.Data.ToPointer();
            uint width = (uint) FOutput.Image.Width;
            uint height = (uint) FOutput.Image.Height;

            for (uint y = 0; y < height; y++)
            {
                for (uint x = 0; x < width; x++)
                {
                    uint index = (uint) (y * width + x);
                    byte oldPixel = data[index];
                    byte newPixel = (byte)(oldPixel > FThreshold ? 255 : 0);
                    data[index] = newPixel;
                    byte propError = asByte(oldPixel - newPixel);

                    
                    if (x + 1 < width)
                    {
                        data[index + 1] += asByte((int)(7.0f * propError) >> 4);
                    }

                    if (y + 1 == height)
                    {
                        continue;
                    }

                    if (x > 0)
                    {
                        data[index - 1 + width] += asByte((int)(3.0f * propError) >> 4);
                    }

                    data[index + width] += asByte((int)(5.0f * propError) >> 4);

                    if (x + 1 < width)
                    {
                        data[index + 1 + width] += asByte((int)(1.0f * propError) >> 4);
                    }

                }
            }
        }

	}
}
