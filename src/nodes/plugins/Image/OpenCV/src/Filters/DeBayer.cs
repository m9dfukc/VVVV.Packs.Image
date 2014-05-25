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
	[FilterInstance("DeBayer", Help = "Converts incoming Bayer L8 image into RGB", Author = "m9dfukc")]
	public class DeBayerInstance : IFilterInstance
    {
        TColorFormat FOutFormat;
        Boolean FPassThrough = false;
        public override void Allocate()
        {
            FOutFormat = GetFormat(FInput.ImageAttributes.ColorFormat);

            //if we can't convert or it's already grayscale, just pass through
            if (FOutFormat == TColorFormat.UnInitialised)
            {
                FOutFormat = FInput.ImageAttributes.ColorFormat;
                FPassThrough = true;
            }
                
            FOutput.Image.Initialise(FInput.Image.ImageAttributes.Size, FOutFormat);
		}

		public override void Process()
		{
            if (FPassThrough)
                FInput.GetImage(FOutput.Image);
            else
            {
                // Whenever you access the pixels directly of FInput
                // you must lock it for reading using 
                if (!FInput.LockForReading()) return;
                CvInvoke.cvCvtColor(FInput.CvMat, FOutput.CvMat, COLOR_CONVERSION.CV_BayerBG2BGR);
                FInput.ReleaseForReading(); // and this after you've finished with FImage
            }   

            FOutput.Send();
		}

        public static TColorFormat GetFormat(TColorFormat format)
        {
            switch (format)
            {
                case TColorFormat.L8:
                    return TColorFormat.RGB8;

                default:
                    return TColorFormat.UnInitialised;
            }
        }
	}
}
