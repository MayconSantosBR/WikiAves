using Microsoft.ML;
using Microsoft.ML.Transforms;
using Microsoft.ML.Data;
using WikiAves.AudioRecognizer.Test.Models;
using Microsoft.ML.Vision;
using WikiAves.Core.Util;
using NAudio.Wave;
using Google.Protobuf.WellKnownTypes;
using System.IO;

namespace SoundClassifier
{
    class Program
    {
        private static string DataPath = @"C:\Estudo\WikiAvesSounds\Sounds\Wav";
        private static string BackupDataPath = @"C:\Estudo\WikiAvesSounds\Sounds\Backup";

        static void Main(string[] args)
        {
            var trainDataPath = string.Concat(DataPath, @"\Data\Train");
            var testDataPath = string.Concat(DataPath, @"\Data\Test");

            List<string> allAudioFiles = Directory.GetFiles(DataPath, "*.wav*", SearchOption.AllDirectories).ToList();
            BreakAudioToStandard(allAudioFiles);

            allAudioFiles = Directory.GetFiles(DataPath, "*.wav*", SearchOption.AllDirectories).ToList();
            allAudioFiles.Shuffle();

            List<List<string>> splittedFiles = allAudioFiles.ChunkBy(allAudioFiles.Count / 2);

            foreach (var file in splittedFiles)
            {
                if (splittedFiles.ElementAt(0) == file)
                {
                    foreach (var audio in file)
                    {
                        var name = audio.Substring(audio.LastIndexOf("\\"));
                        File.Move(audio, testDataPath + name);
                    }
                }

                if (splittedFiles.ElementAt(1) == file)
                {
                    foreach (var audio in file)
                    {
                        var name = audio.Substring(audio.LastIndexOf("\\"));
                        File.Move(audio, trainDataPath + name);
                    }
                }
            }

            allAudioFiles = Directory.GetFiles(DataPath, "*.wav*", SearchOption.AllDirectories).ToList();

            //Data pre-processing
            foreach (var fileName in allAudioFiles)
                CreateSpectrogram(fileName);

            MLContext mlContext = new MLContext(seed: 1);

            //Read and shuffle
            IEnumerable<SpectrogramData> images = LoadImagesFromDirectory(folder: trainDataPath, useFolderNameasLabel: false).ToList();
            IEnumerable<SpectrogramData> testImages = LoadImagesFromDirectory(folder: testDataPath, useFolderNameasLabel: false).ToList();

            IDataView trainDataView = mlContext.Data.LoadFromEnumerable(images);
            trainDataView = mlContext.Data.ShuffleRows(trainDataView);

            IDataView testDataView = mlContext.Data.LoadFromEnumerable(testImages);

            IDataView transformedValidationDataView = mlContext.Transforms.Conversion.MapValueToKey(outputColumnName: "LabelAsKey",
                                                                            inputColumnName: "Label")
                                                        .Append(mlContext.Transforms.LoadRawImageBytes(
                                                            outputColumnName: "Image",
                                                            imageFolder: testDataPath,
                                                            inputColumnName: "ImagePath"))
                                                        .Fit(testDataView)
                                                        .Transform(testDataView);

            IDataView transformedTrainDataView = mlContext.Transforms.Conversion.MapValueToKey(outputColumnName: "LabelAsKey",
                                                                            inputColumnName: "Label")
                                                        .Append(mlContext.Transforms.LoadRawImageBytes(
                                                            outputColumnName: "Image",
                                                            imageFolder: trainDataPath,
                                                            inputColumnName: "ImagePath"))
                                                        .Fit(trainDataView)
                                                        .Transform(trainDataView);

            //Define training pipeline
            var pipeline = mlContext.Transforms.Conversion.MapValueToKey(outputColumnName: "LabelAsKey",
                                                                            inputColumnName: "Label")
                                                                                .Append(mlContext.MulticlassClassification.Trainers.ImageClassification(new ImageClassificationTrainer.Options()
                                                                                {
                                                                                    LabelColumnName = "LabelAsKey",
                                                                                    FeatureColumnName = "Image",
                                                                                    Arch = ImageClassificationTrainer.Architecture.InceptionV3,
                                                                                    Epoch = 200,
                                                                                    MetricsCallback = (metrics) => Console.WriteLine(metrics),
                                                                                    ValidationSet = transformedValidationDataView,
                                                                                }));

            //Train model
            ITransformer trainedModel = pipeline.Fit(transformedTrainDataView);

            //Evaluate
            EvaluateModel(mlContext, transformedValidationDataView, trainedModel);

            // Save
            mlContext.Model.Save(trainedModel, trainDataView.Schema, "sound-classifier.zip");
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
                    var name = audio.Substring(audio.LastIndexOf("\\"));
                    File.Move(audio, BackupDataPath + name);
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

            if (File.Exists(spectrogramName))
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