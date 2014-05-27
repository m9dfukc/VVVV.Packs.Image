#region usings
using System;
using System.ComponentModel.Composition;
using System.Drawing;
using System.Threading;

using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VMath;
using VVVV.Core.Logging;

using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.CvEnum;
using ThreadState = System.Threading.ThreadState;
using System.Collections.Generic;
using Emgu.CV.Features2D;
using VVVV.CV.Core;

#endregion usings

namespace VVVV.CV.Nodes.Features
{
    public class DetectFeatures2Instance : IDestinationInstance
    {
        public FeatureSet FeaturesSet {get; private set;}
        public FastDetector Detector = new FastDetector(20, true);
        CVImage FGrayScale = new CVImage();

        public DetectFeatures2Instance()
        {
            this.FeaturesSet = new FeatureSet();
        }

        public int Threshold
        {
            set
            {
                this.Detector = new FastDetector(value, true);
            }
        }

        public override void Allocate()
        {
            FGrayScale.Initialise(FInput.Image.Size, TColorFormat.L8);
        }

        public override void Process()
        {
            lock (this.FeaturesSet.Lock)
            {
                FInput.GetImage(FGrayScale);
                var gray = FGrayScale.GetImage() as Image<Gray, Byte>;
                this.FeaturesSet.KeyPoints = this.Detector.DetectKeyPointsRaw(gray, null);
                this.FeaturesSet.Allocated = true;
                this.FeaturesSet.OnUpdate();
            }
        }
    }

    #region PluginInfo
    [PluginInfo(Name = "DetectFeatures2", Category = "CV.Image", Help = "Find feature points in 2D image", Tags = "FAST")]
    #endregion PluginInfo
    public class DetectFeatures2Node : IDestinationNode<DetectFeatures2Instance>
    {
        [Input("Threshold", MinValue=0, DefaultValue=20)]
        IDiffSpread<int> FInThreshold;

        [Output("Output")]
        ISpread<FeatureSet> FOutput;

        protected override void Update(int InstanceCount, bool SpreadChanged)
        {
            if (FInThreshold.IsChanged || SpreadChanged)
            {
                for (int i = 0; i < InstanceCount; i++)
                {
                    FProcessor[i].Threshold = FInThreshold[i];
                }
            }

            if (SpreadChanged)
            {
                FOutput.SliceCount = InstanceCount;

                for (int i = 0; i < InstanceCount; i++)
                {
                    FOutput[i] = FProcessor[i].FeaturesSet;
                }
            }
        }
    }
}
