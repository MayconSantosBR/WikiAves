using Microsoft.ML;
using Microsoft.ML.Transforms;
using Microsoft.ML.Data;
using WikiAves.AudioRecognizer.Test.Models;
using Microsoft.ML.Vision;
using WikiAves.Core.Util;
using NAudio.Wave;
using Google.Protobuf.WellKnownTypes;
using System.IO;
using static Tensorflow.Summary.Types;
using static Microsoft.ML.DataOperationsCatalog;
using Tensorflow;

namespace SoundClassifier
{
    class Program
    {
        private static string AudioDataPath = @"C:\Estudo\WikiAvesSounds\Sounds\Wav";
        private static string BackupAudioDataPath = @"C:\Estudo\WikiAvesSounds\Sounds\Backup";
        private static string SpectogramDataPath = @"C:\Estudo\WikiAvesSounds\Sounds\Spectograms";

        static void Main(string[] args)
        {
            var trainDataPath = string.Concat(AudioDataPath, @"\Data\Train");
            var testDataPath = string.Concat(AudioDataPath, @"\Data\Test");

            var allAudioFiles = Directory.GetFiles(AudioDataPath, "*.wav*", SearchOption.AllDirectories).ToList();
            BreakAudioToStandard(allAudioFiles);

            //Create spectogram of wav files
            foreach (var fileName in allAudioFiles)
                CreateSpectrogram(fileName);


            //Move spectograms to directory
            var allSpectograms = Directory.GetFiles(AudioDataPath, "*.jpg", SearchOption.AllDirectories).ToList();
            MoveSpectogramsToFile(allSpectograms);

            IEnumerable<SpectrogramData> images = LoadImagesFromDirectory(folder: SpectogramDataPath, false);

            //MlContext
            MLContext mlContext = new MLContext(seed: 1);
            IDataView imageData = mlContext.Data.LoadFromEnumerable(images);
            IDataView shuffledData = mlContext.Data.ShuffleRows(imageData);

            //Pipeline
            var preProcessingPipeline = mlContext.Transforms.Conversion.MapValueToKey(
                                            inputColumnName: nameof(ModelInput.Label),
                                            outputColumnName: nameof(ModelInput.LabelAsKey))
                                        .Append(mlContext.Transforms.LoadRawImageBytes(
                                            outputColumnName: "Image",
                                            imageFolder: SpectogramDataPath,
                                            inputColumnName: nameof(ModelInput.ImagePath)));

            IDataView preProcessedData = preProcessingPipeline
                                            .Fit(shuffledData)
                                            .Transform(shuffledData);

            // Split Dataset : Train/Test/Validation
            TrainTestData trainSplit = mlContext.Data.TrainTestSplit(data: preProcessedData, testFraction: 0.3);
            TrainTestData validationTestSplit = mlContext.Data.TrainTestSplit(trainSplit.TestSet);
            IDataView trainSet = trainSplit.TrainSet;                // 70% of total dataset
            IDataView validationSet = validationTestSplit.TrainSet;  // 90% of 30% of total dataset
            IDataView testSet = validationTestSplit.TestSet;         // 10% of 30% of total dataset


            // Classifier Options
            // Architecture - ResnetV2101
            var classifierOptions = new ImageClassificationTrainer.Options()
            {
                FeatureColumnName = "Image",
                LabelColumnName = "LabelAsKey",
                ValidationSet = validationSet,
                Arch = ImageClassificationTrainer.Architecture.InceptionV3,
                MetricsCallback = (metrics) => Console.WriteLine(metrics),
                TestOnTrainSet = false,
                ReuseTrainSetBottleneckCachedValues = true,
                ReuseValidationSetBottleneckCachedValues = true
            };

            // Training Pipeline
            var trainingPipeline = mlContext.MulticlassClassification.Trainers.ImageClassification(classifierOptions);

            // Train Model
            ITransformer trainedModel = trainingPipeline.Fit(trainSet);

            //Evaluate
            EvaluateModel(mlContext, trainSet, trainedModel);

            // Save
            mlContext.Model.Save(trainedModel, trainSet.Schema, "sound-classifier.zip");
        }

        private static void MoveSpectogramsToFile(List<string> allSpectograms)
        {
            foreach (var fileName in allSpectograms)
            {
                var name = fileName.Substring(fileName.LastIndexOf("\\"));
                File.Move(fileName, SpectogramDataPath + name);
            }
        }

        private static void BreakAudioToStandard(List<string> allAudioFiles, int time = 10)
        {
            foreach (var audio in allAudioFiles)
            {
                List<string> deleteFiles = new();
                List<string> moveFiles = new();

                using (WaveFileReader waveReader = new WaveFileReader(audio))
                {
                    if (audio.Contains("-part"))
                        continue;

                    if (waveReader.TotalTime < TimeSpan.FromSeconds(time))
                        deleteFiles.Add(audio);

                    if (waveReader.TotalTime > TimeSpan.FromSeconds(time))
                    {
                        var currentTime = 0;
                        int part = 1;

                        while (currentTime < waveReader.TotalTime.TotalSeconds && (currentTime + time) < waveReader.TotalTime.TotalSeconds)
                        {
                            using (var audioReader = new AudioFileReader(audio))
                            {
                                audioReader.CurrentTime = TimeSpan.FromSeconds(0 + currentTime);
                                WaveFileWriter.CreateWaveFile16($"{audio.Substring(0, audio.Length - 4)}-part{part}.wav", audioReader.Take(TimeSpan.FromSeconds(time)));
                                currentTime += time;
                                part++;
                            }
                        }

                        moveFiles.Add(audio);
                    }
                }

                foreach (var file in deleteFiles)
                    File.Delete(file);

                foreach (var file in moveFiles)
                {
                    var name = file.Substring(file.LastIndexOf("\\"));
                    File.Move(file, BackupAudioDataPath + name);
                }
            }
        }

        private static void EvaluateModel(MLContext mlContext, IDataView testDataset, ITransformer trainedModel)
        {
            Console.WriteLine("Making predictions in bulk for evaluating model's quality...");

            IDataView predictionsDataView = trainedModel.Transform(testDataset);

            var metrics = mlContext.MulticlassClassification.Evaluate(predictionsDataView, labelColumnName: "LabelAsKey", predictedLabelColumnName: "PredictedLabel");

            Console.WriteLine("*** Showing all the predictions ***");
            VBuffer<ReadOnlyMemory<char>> keys = default;
            predictionsDataView.Schema["LabelAsKey"].GetKeyValues(ref keys);
            var originalLabels = keys.DenseValues().ToArray();

            List<SpectrogramPredictionEx> predictions = mlContext.Data.CreateEnumerable<SpectrogramPredictionEx>(predictionsDataView, false, true).ToList();
            predictions.ForEach(pred => ConsoleWriteImagePrediction(pred.ImagePath, pred.Label, (originalLabels[pred.PredictedLabel - 1]).ToString(), pred.Score.Max()));
        }

        private static void CreateSpectrogram(string fileName)
        {
            var spectrogramName = fileName.Substring(0, fileName.Length - 4) + "-spectro.jpg";
            var name = SpectogramDataPath + spectrogramName.Substring(fileName.LastIndexOf("\\"));


            if (File.Exists(spectrogramName) || File.Exists(name))
                return;

            var spec = new Spectrogram.Spectrogram(sampleRate: 8000, fftSize: 2048, step: 700);
            float[] values = Spectrogram.Tools.ReadWav(fileName);
            spec.AddExtend(values);

            var bitmap = spec.GetBitmap(intensity: 2, freqHigh: 2500);

            if (bitmap == null)
                return;

            spec.SaveBitmap(bitmap, spectrogramName);
        }

        public static IEnumerable<SpectrogramData> LoadImagesFromDirectory(string folder, bool useFolderNameasLabel = true)
        {
            var files = Directory.GetFiles(folder, "*spectro.jpg",
                searchOption: SearchOption.AllDirectories);

            foreach (var file in files)
            {
                if ((Path.GetExtension(file) != ".jpg") && (Path.GetExtension(file) != ".png"))
                    continue;

                var fileName = Path.GetFileName(file);
                var label = fileName.Substring(0, fileName.LastIndexOf("_"));

                yield return new SpectrogramData()
                {
                    ImagePath = file,
                    Label = label,
                };
            }
        }

        public static void ConsoleWriteImagePrediction(string ImagePath, string Label, string PredictedLabel, float Probability)
        {
            var defaultForeground = Console.ForegroundColor;
            var labelColor = ConsoleColor.Magenta;
            var probColor = ConsoleColor.Blue;

            Console.Write("Image File: ");
            Console.ForegroundColor = labelColor;
            Console.Write($"{Path.GetFileName(ImagePath)}");
            Console.ForegroundColor = defaultForeground;
            Console.Write(" original labeled as ");
            Console.ForegroundColor = labelColor;
            Console.Write(Label);
            Console.ForegroundColor = defaultForeground;
            Console.Write(" predicted as ");
            Console.ForegroundColor = labelColor;
            Console.Write(PredictedLabel);
            Console.ForegroundColor = defaultForeground;
            Console.Write(" with score ");
            Console.ForegroundColor = probColor;
            Console.Write(Probability);
            Console.ForegroundColor = defaultForeground;
            Console.WriteLine("");
        }
    }
}