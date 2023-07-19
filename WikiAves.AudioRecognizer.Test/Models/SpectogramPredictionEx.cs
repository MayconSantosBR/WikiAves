using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WikiAves.AudioRecognizer.Test.Models
{
    public class SpectrogramPredictionEx
    {
        public string ImagePath;
        public string Label;
        public UInt32 PredictedLabel;
        public float[] Score;
    }
}
