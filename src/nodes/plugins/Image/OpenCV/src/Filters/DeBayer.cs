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
        public override void Allocate()
        {
            FOutput.Image.Initialise(FInput.Image.ImageAttributes.Size, TColorFormat.RGB8);
		}

		public override void Process()
		{
            // Whenever you access the pixels directly of FInput
            // you must lock it for reading using 
            if (!FInput.LockForReading()) return;

            CvInvoke.cvCvtColor(FInput.CvMat, FOutput.CvMat, COLOR_CONVERSION.CV_BayerBG2BGR);

            FInput.ReleaseForReading(); // and this after you've finished with FImage
            FOutput.Send();
		}
	}
}
