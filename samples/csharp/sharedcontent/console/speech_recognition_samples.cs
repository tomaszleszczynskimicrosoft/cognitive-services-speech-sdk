//
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE.md file in the project root for full license information.
//

// <toplevel>
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Linq;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using Microsoft.CognitiveServices.Speech.PronunciationAssessment;
using DiffPlex;
using DiffPlex.DiffBuilder;
using DiffPlex.DiffBuilder.Model;
using System.Text.Json;
using System.Text;
using Azure.Core;
using Azure.Identity;
// </toplevel>

namespace MicrosoftSpeechSDKSamples
{
    public class SpeechRecognitionSamples
    {
        // Speech recognition from microphone.
        public static async Task RecognitionWithMicrophoneAsync()
        {
            // <recognitionWithMicrophone>
            // Creates an instance of a speech config with specified endpoint and subscription key.
            // Replace with your own endpoint and subscription key.
            // The default language is "en-us".
            var config = SpeechConfig.FromEndpoint(new Uri("https://YourServiceRegion.api.cognitive.microsoft.com"), "YourSubscriptionKey");

            // Creates a speech recognizer using microphone as audio input.
            using (var recognizer = new SpeechRecognizer(config))
            {
                // Starts recognizing.
                Console.WriteLine("Say something...");

                // Starts speech recognition, and returns after a single utterance is recognized. The end of a
                // single utterance is determined by listening for silence at the end or until a maximum of about 30
                // seconds of audio is processed.  The task returns the recognition text as result.
                // Note: Since RecognizeOnceAsync() returns only a single utterance, it is suitable only for single
                // shot recognition like command or query.
                // For long-running multi-utterance recognition, use StartContinuousRecognitionAsync() instead.
                var result = await recognizer.RecognizeOnceAsync().ConfigureAwait(false);

                // Checks result.
                if (result.Reason == ResultReason.RecognizedSpeech)
                {
                    Console.WriteLine($"RECOGNIZED: Text={result.Text}");
                }
                else if (result.Reason == ResultReason.NoMatch)
                {
                    Console.WriteLine($"NOMATCH: Speech could not be recognized.");
                }
                else if (result.Reason == ResultReason.Canceled)
                {
                    var cancellation = CancellationDetails.FromResult(result);
                    Console.WriteLine($"CANCELED: Reason={cancellation.Reason}");

                    if (cancellation.Reason == CancellationReason.Error)
                    {
                        Console.WriteLine($"CANCELED: ErrorCode={cancellation.ErrorCode}");
                        Console.WriteLine($"CANCELED: ErrorDetails={cancellation.ErrorDetails}");
                        Console.WriteLine($"CANCELED: Did you update the subscription info?");
                    }
                }
            }
            // </recognitionWithMicrophone>
        }

        // Speech recognition in the specified spoken language and uses detailed output format.
        public static async Task RecognitionWithLanguageAndDetailedOutputAsync()
        {
            // Creates an instance of a speech config with specified endpoint and subscription key.
            // Replace with your own endpoint and subscription key.
            var config = SpeechConfig.FromEndpoint(new Uri("https://YourServiceRegion.api.cognitive.microsoft.com"), "YourSubscriptionKey");

            // Replace the language with your language in BCP-47 format, e.g., en-US.
            var language = "de-DE";

            // Ask for detailed recognition result
            config.OutputFormat = OutputFormat.Detailed;

            // If you also want word-level timing in the detailed recognition results, set the following.
            // Note that if you set the following, you can omit the previous line
            //      "config.OutputFormat = OutputFormat.Detailed",
            // since word-level timing implies detailed recognition results.
            config.RequestWordLevelTimestamps();

            // Creates a speech recognizer for the specified language, using microphone as audio input.
            // Requests detailed output format.
            using (var recognizer = new SpeechRecognizer(config, language))
            {
                // Starts recognizing.
                Console.WriteLine($"Say something in {language}...");

                // Starts speech recognition, and returns after a single utterance is recognized. The end of a
                // single utterance is determined by listening for silence at the end or until a maximum of about 30
                // seconds of audio is processed.  The task returns the recognition text as result.
                // Note: Since RecognizeOnceAsync() returns only a single utterance, it is suitable only for single
                // shot recognition like command or query.
                // For long-running multi-utterance recognition, use StartContinuousRecognitionAsync() instead.
                var result = await recognizer.RecognizeOnceAsync().ConfigureAwait(false);

                // Checks result.
                if (result.Reason == ResultReason.RecognizedSpeech)
                {
                    Console.WriteLine($"RECOGNIZED: Text={result.Text}");
                    Console.WriteLine("  Detailed results:");

                    // The first item in detailedResults corresponds to the recognized text
                    // (NOT the item with the highest confidence number!)
                    var detailedResults = result.Best();
                    foreach (var item in detailedResults)
                    {
                        Console.WriteLine($"\tConfidence: {item.Confidence}\n\tText: {item.Text}\n\tLexicalForm: {item.LexicalForm}\n\tNormalizedForm: {item.NormalizedForm}\n\tMaskedNormalizedForm: {item.MaskedNormalizedForm}");
                        Console.WriteLine($"\tWord-level timing:");
                        Console.WriteLine($"\t\tWord | Offset | Duration");

                        // Word-level timing
                        foreach (var word in item.Words)
                        {
                            Console.WriteLine($"\t\t{word.Word} {word.Offset} {word.Duration}");
                        }
                    }
                }
                else if (result.Reason == ResultReason.NoMatch)
                {
                    Console.WriteLine($"NOMATCH: Speech could not be recognized.");
                }
                else if (result.Reason == ResultReason.Canceled)
                {
                    var cancellation = CancellationDetails.FromResult(result);
                    Console.WriteLine($"CANCELED: Reason={cancellation.Reason}");

                    if (cancellation.Reason == CancellationReason.Error)
                    {
                        Console.WriteLine($"CANCELED: ErrorCode={cancellation.ErrorCode}");
                        Console.WriteLine($"CANCELED: ErrorDetails={cancellation.ErrorDetails}");
                        Console.WriteLine($"CANCELED: Did you update the subscription info?");
                    }
                }
            }
        }

        // Speech recognition using a customized model.
        public static async Task RecognitionUsingCustomizedModelAsync()
        {
            // <recognitionCustomized>
            // Creates an instance of a speech config with specified endpoint and subscription key.
            // Replace with your own endpoint and subscription key.
            var config = SpeechConfig.FromEndpoint(new Uri("https://YourServiceRegion.api.cognitive.microsoft.com"), "YourSubscriptionKey");
            // Replace with the CRIS endpoint id of your customized model.
            var sourceLanguageConfig = SourceLanguageConfig.FromLanguage("en-US", "YourEndpointId");

            // Creates a speech recognizer using microphone as audio input.
            using (var recognizer = new SpeechRecognizer(config, sourceLanguageConfig))
            {
                Console.WriteLine("Say something...");

                // Starts speech recognition, and returns after a single utterance is recognized. The end of a
                // single utterance is determined by listening for silence at the end or until a maximum of about 30
                // seconds of audio is processed.  The task returns the recognition text as result.
                // Note: Since RecognizeOnceAsync() returns only a single utterance, it is suitable only for single
                // shot recognition like command or query.
                // For long-running multi-utterance recognition, use StartContinuousRecognitionAsync() instead.
                var result = await recognizer.RecognizeOnceAsync().ConfigureAwait(false);

                // Checks results.
                if (result.Reason == ResultReason.RecognizedSpeech)
                {
                    Console.WriteLine($"RECOGNIZED: Text={result.Text}");
                }
                else if (result.Reason == ResultReason.NoMatch)
                {
                    Console.WriteLine($"NOMATCH: Speech could not be recognized.");
                }
                else if (result.Reason == ResultReason.Canceled)
                {
                    var cancellation = CancellationDetails.FromResult(result);
                    Console.WriteLine($"CANCELED: Reason={cancellation.Reason}");

                    if (cancellation.Reason == CancellationReason.Error)
                    {
                        Console.WriteLine($"CANCELED: ErrorCode={cancellation.ErrorCode}");
                        Console.WriteLine($"CANCELED: ErrorDetails={cancellation.ErrorDetails}");
                        Console.WriteLine($"CANCELED: Did you update the subscription info?");
                    }
                }
            }
            // </recognitionCustomized>
        }

        // Continuous speech recognition.
        public static async Task ContinuousRecognitionWithFileAsync()
        {
            // <recognitionContinuousWithFile>
            // Creates an instance of a speech config with specified endpoint and subscription key.
            // Replace with your own endpoint and subscription key.
            var config = SpeechConfig.FromEndpoint(new Uri("https://YourServiceRegion.api.cognitive.microsoft.com"), "YourSubscriptionKey");

            var stopRecognition = new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously);

            // Creates a speech recognizer using file as audio input.
            // Replace with your own audio file name.
            using (var audioInput = AudioConfig.FromWavFileInput(@"whatstheweatherlike.wav"))
            {
                using (var recognizer = new SpeechRecognizer(config, audioInput))
                {
                    // Subscribes to events.
                    recognizer.Recognizing += (s, e) =>
                    {
                        Console.WriteLine($"RECOGNIZING: Text={e.Result.Text}");
                    };

                    recognizer.Recognized += (s, e) =>
                    {
                        if (e.Result.Reason == ResultReason.RecognizedSpeech)
                        {
                            Console.WriteLine($"RECOGNIZED: Text={e.Result.Text}");
                        }
                        else if (e.Result.Reason == ResultReason.NoMatch)
                        {
                            Console.WriteLine($"NOMATCH: Speech could not be recognized.");
                        }
                    };

                    recognizer.Canceled += (s, e) =>
                    {
                        Console.WriteLine($"CANCELED: Reason={e.Reason}");

                        if (e.Reason == CancellationReason.Error)
                        {
                            Console.WriteLine($"CANCELED: ErrorCode={e.ErrorCode}");
                            Console.WriteLine($"CANCELED: ErrorDetails={e.ErrorDetails}");
                            Console.WriteLine($"CANCELED: Did you update the subscription info?");
                        }

                        stopRecognition.TrySetResult(0);
                    };

                    recognizer.SessionStarted += (s, e) =>
                    {
                        Console.WriteLine("\n    Session started event.");
                    };

                    recognizer.SessionStopped += (s, e) =>
                    {
                        Console.WriteLine("\n    Session stopped event.");
                        Console.WriteLine("\nStop recognition.");
                        stopRecognition.TrySetResult(0);
                    };

                    // Starts continuous recognition. Uses StopContinuousRecognitionAsync() to stop recognition.
                    await recognizer.StartContinuousRecognitionAsync().ConfigureAwait(false);

                    // Waits for completion.
                    // Use Task.WaitAny to keep the task rooted.
                    Task.WaitAny(new[] { stopRecognition.Task });

                    // Stops recognition.
                    await recognizer.StopContinuousRecognitionAsync().ConfigureAwait(false);
                }
            }
            // </recognitionContinuousWithFile>
        }

        public static async Task SpeechRecognitionWithCompressedInputPullStreamAudio()
        {
            // <recognitionWithCompressedInputPullStreamAudio>
            // Creates an instance of a speech config with specified endpoint and subscription key.
            // Replace with your own endpoint and subscription key.
            var config = SpeechConfig.FromEndpoint(new Uri("https://YourServiceRegion.api.cognitive.microsoft.com"), "YourSubscriptionKey");

            var stopRecognition = new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously);

            // Create an audio stream from a wav file.
            // Replace with your own audio file name.

            using (var audioInput = AudioConfig.FromStreamInput(new PullAudioInputStream(new BinaryAudioStreamReader(
                                    new BinaryReader(File.OpenRead(@"whatstheweatherlike.mp3"))),
                                    AudioStreamFormat.GetCompressedFormat(AudioStreamContainerFormat.MP3))))
            {
                // Creates a speech recognizer using audio stream input.
                using (var recognizer = new SpeechRecognizer(config, audioInput))
                {
                    // Subscribes to events.
                    recognizer.Recognizing += (s, e) =>
                    {
                        Console.WriteLine($"RECOGNIZING: Text={e.Result.Text}");
                    };

                    recognizer.Recognized += (s, e) =>
                    {
                        if (e.Result.Reason == ResultReason.RecognizedSpeech)
                        {
                            Console.WriteLine($"RECOGNIZED: Text={e.Result.Text}");
                        }
                        else if (e.Result.Reason == ResultReason.NoMatch)
                        {
                            Console.WriteLine($"NOMATCH: Speech could not be recognized.");
                        }
                    };

                    recognizer.Canceled += (s, e) =>
                    {
                        Console.WriteLine($"CANCELED: Reason={e.Reason}");

                        if (e.Reason == CancellationReason.Error)
                        {
                            Console.WriteLine($"CANCELED: ErrorCode={e.ErrorCode}");
                            Console.WriteLine($"CANCELED: ErrorDetails={e.ErrorDetails}");
                            Console.WriteLine($"CANCELED: Did you update the subscription info?");
                        }

                        stopRecognition.TrySetResult(0);
                    };

                    recognizer.SessionStarted += (s, e) =>
                    {
                        Console.WriteLine("\nSession started event.");
                    };

                    recognizer.SessionStopped += (s, e) =>
                    {
                        Console.WriteLine("\nSession stopped event.");
                        Console.WriteLine("\nStop recognition.");
                        stopRecognition.TrySetResult(0);
                    };

                    // Starts continuous recognition. Uses StopContinuousRecognitionAsync() to stop recognition.
                    await recognizer.StartContinuousRecognitionAsync().ConfigureAwait(false);

                    // Waits for completion.
                    // Use Task.WaitAny to keep the task rooted.
                    Task.WaitAny(new[] { stopRecognition.Task });

                    // Stops recognition.
                    await recognizer.StopContinuousRecognitionAsync().ConfigureAwait(false);
                }
            }
            // </recognitionWithCompressedInputPullStreamAudio>
        }

        public static async Task SpeechRecognitionWithCompressedInputPushStreamAudio()
        {
            // <recognitionWithCompressedInputPushStreamAudio>
            // Creates an instance of a speech config with specified endpoint and subscription key.
            // Replace with your own endpoint and subscription key.
            var config = SpeechConfig.FromEndpoint(new Uri("https://YourServiceRegion.api.cognitive.microsoft.com"), "YourSubscriptionKey");

            var stopRecognition = new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously);

            using (var pushStream = AudioInputStream.CreatePushStream(AudioStreamFormat.GetCompressedFormat(AudioStreamContainerFormat.MP3)))
            {
                using (var audioInput = AudioConfig.FromStreamInput(pushStream))
                {
                    // Creates a speech recognizer using audio stream input.
                    using (var recognizer = new SpeechRecognizer(config, audioInput))
                    {
                        // Subscribes to events.
                        recognizer.Recognizing += (s, e) =>
                        {
                            Console.WriteLine($"RECOGNIZING: Text={e.Result.Text}");
                        };

                        recognizer.Recognized += (s, e) =>
                        {
                            if (e.Result.Reason == ResultReason.RecognizedSpeech)
                            {
                                Console.WriteLine($"RECOGNIZED: Text={e.Result.Text}");
                            }
                            else if (e.Result.Reason == ResultReason.NoMatch)
                            {
                                Console.WriteLine($"NOMATCH: Speech could not be recognized.");
                            }
                        };

                        recognizer.Canceled += (s, e) =>
                        {
                            Console.WriteLine($"CANCELED: Reason={e.Reason}");

                            if (e.Reason == CancellationReason.Error)
                            {
                                Console.WriteLine($"CANCELED: ErrorCode={e.ErrorCode}");
                                Console.WriteLine($"CANCELED: ErrorDetails={e.ErrorDetails}");
                                Console.WriteLine($"CANCELED: Did you update the subscription info?");
                            }

                            stopRecognition.TrySetResult(0);
                        };

                        recognizer.SessionStarted += (s, e) =>
                        {
                            Console.WriteLine("\nSession started event.");
                        };

                        recognizer.SessionStopped += (s, e) =>
                        {
                            Console.WriteLine("\nSession stopped event.");
                            Console.WriteLine("\nStop recognition.");
                            stopRecognition.TrySetResult(0);
                        };

                        // Starts continuous recognition. Uses StopContinuousRecognitionAsync() to stop recognition.
                        await recognizer.StartContinuousRecognitionAsync().ConfigureAwait(false);

                        using (BinaryAudioStreamReader reader = Helper.CreateBinaryFileReader(@"whatstheweatherlike.mp3"))
                        {
                            byte[] buffer = new byte[1000];
                            while (true)
                            {
                                var readSamples = reader.Read(buffer, (uint)buffer.Length);
                                if (readSamples == 0)
                                {
                                    break;
                                }
                                pushStream.Write(buffer, readSamples);
                            }
                        }
                        pushStream.Close();

                        // Waits for completion.
                        // Use Task.WaitAny to keep the task rooted.
                        Task.WaitAny(new[] { stopRecognition.Task });

                        // Stops recognition.
                        await recognizer.StopContinuousRecognitionAsync().ConfigureAwait(false);
                    }
                }
            }
            // </recognitionWithCompressedInputPushStreamAudio>
        }

        // Speech recognition with audio stream
        public static async Task RecognitionWithPullAudioStreamAsync()
        {
            // Creates an instance of a speech config with specified endpoint and subscription key.
            // Replace with your own endpoint and subscription key.
            var config = SpeechConfig.FromEndpoint(new Uri("https://YourServiceRegion.api.cognitive.microsoft.com"), "YourSubscriptionKey");

            var stopRecognition = new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously);

            // Create an audio stream from a wav file.
            // Replace with your own audio file name.
            using (var audioInput = Helper.OpenWavFile(@"whatstheweatherlike.wav"))
            {
                // Creates a speech recognizer using audio stream input.
                using (var recognizer = new SpeechRecognizer(config, audioInput))
                {
                    // Subscribes to events.
                    recognizer.Recognizing += (s, e) =>
                    {
                        Console.WriteLine($"RECOGNIZING: Text={e.Result.Text}");
                    };

                    recognizer.Recognized += (s, e) =>
                    {
                        if (e.Result.Reason == ResultReason.RecognizedSpeech)
                        {
                            Console.WriteLine($"RECOGNIZED: Text={e.Result.Text}");
                        }
                        else if (e.Result.Reason == ResultReason.NoMatch)
                        {
                            Console.WriteLine($"NOMATCH: Speech could not be recognized.");
                        }
                    };

                    recognizer.Canceled += (s, e) =>
                    {
                        Console.WriteLine($"CANCELED: Reason={e.Reason}");

                        if (e.Reason == CancellationReason.Error)
                        {
                            Console.WriteLine($"CANCELED: ErrorCode={e.ErrorCode}");
                            Console.WriteLine($"CANCELED: ErrorDetails={e.ErrorDetails}");
                            Console.WriteLine($"CANCELED: Did you update the subscription info?");
                        }

                        stopRecognition.TrySetResult(0);
                    };

                    recognizer.SessionStarted += (s, e) =>
                    {
                        Console.WriteLine("\nSession started event.");
                    };

                    recognizer.SessionStopped += (s, e) =>
                    {
                        Console.WriteLine("\nSession stopped event.");
                        Console.WriteLine("\nStop recognition.");
                        stopRecognition.TrySetResult(0);
                    };

                    // Starts continuous recognition. Uses StopContinuousRecognitionAsync() to stop recognition.
                    await recognizer.StartContinuousRecognitionAsync().ConfigureAwait(false);

                    // Waits for completion.
                    // Use Task.WaitAny to keep the task rooted.
                    Task.WaitAny(new[] { stopRecognition.Task });

                    // Stops recognition.
                    await recognizer.StopContinuousRecognitionAsync().ConfigureAwait(false);
                }
            }
        }

        public static async Task RecognitionWithPushAudioStreamAsync()
        {
            // Creates an instance of a speech config with specified endpoint and subscription key.
            // Replace with your own endpoint and subscription key.
            var config = SpeechConfig.FromEndpoint(new Uri("https://YourServiceRegion.api.cognitive.microsoft.com"), "YourSubscriptionKey");

            var stopRecognition = new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously);

            // Create a push stream
            using (var pushStream = AudioInputStream.CreatePushStream())
            {
                using (var audioInput = AudioConfig.FromStreamInput(pushStream))
                {
                    // Creates a speech recognizer using audio stream input.
                    using (var recognizer = new SpeechRecognizer(config, audioInput))
                    {
                        // Subscribes to events.
                        recognizer.Recognizing += (s, e) =>
                        {
                            Console.WriteLine($"RECOGNIZING: Text={e.Result.Text}");
                        };

                        recognizer.Recognized += (s, e) =>
                        {
                            if (e.Result.Reason == ResultReason.RecognizedSpeech)
                            {
                                Console.WriteLine($"RECOGNIZED: Text={e.Result.Text}");
                            }
                            else if (e.Result.Reason == ResultReason.NoMatch)
                            {
                                Console.WriteLine($"NOMATCH: Speech could not be recognized.");
                            }
                        };

                        recognizer.Canceled += (s, e) =>
                        {
                            Console.WriteLine($"CANCELED: Reason={e.Reason}");

                            if (e.Reason == CancellationReason.Error)
                            {
                                Console.WriteLine($"CANCELED: ErrorCode={e.ErrorCode}");
                                Console.WriteLine($"CANCELED: ErrorDetails={e.ErrorDetails}");
                                Console.WriteLine($"CANCELED: Did you update the subscription info?");
                            }

                            stopRecognition.TrySetResult(0);
                        };

                        recognizer.SessionStarted += (s, e) =>
                        {
                            Console.WriteLine("\nSession started event.");
                        };

                        recognizer.SessionStopped += (s, e) =>
                        {
                            Console.WriteLine("\nSession stopped event.");
                            Console.WriteLine("\nStop recognition.");
                            stopRecognition.TrySetResult(0);
                        };

                        // Starts continuous recognition. Uses StopContinuousRecognitionAsync() to stop recognition.
                        await recognizer.StartContinuousRecognitionAsync().ConfigureAwait(false);

                        // open and read the wave file and push the buffers into the recognizer
                        using (BinaryAudioStreamReader reader = Helper.CreateWavReader(@"whatstheweatherlike.wav"))
                        {
                            byte[] buffer = new byte[1000];
                            while (true)
                            {
                                var readSamples = reader.Read(buffer, (uint)buffer.Length);
                                if (readSamples == 0)
                                {
                                    break;
                                }
                                pushStream.Write(buffer, readSamples);
                            }
                        }
                        pushStream.Close();

                        // Waits for completion.
                        // Use Task.WaitAny to keep the task rooted.
                        Task.WaitAny(new[] { stopRecognition.Task });

                        // Stops recognition.
                        await recognizer.StopContinuousRecognitionAsync().ConfigureAwait(false);
                    }
                }
            }
        }

        // Continuous speech recognition with keyword spotting.
        public static async Task ContinuousRecognitionWithKeywordSpottingAsync()
        {
            // Creates an instance of a speech config with specified endpoint and subscription key.
            // Replace with your own endpoint and subscription key.
            var config = SpeechConfig.FromEndpoint(new Uri("https://YourServiceRegion.api.cognitive.microsoft.com"), "YourSubscriptionKey");

            // Creates an instance of a keyword recognition model. Update this to
            // point to the location of your keyword recognition model.
            var model = KeywordRecognitionModel.FromFile("YourKeywordRecognitionModelFile.table");

            // The phrase your keyword recognition model triggers on.
            var keyword = "YourKeyword";

            var stopRecognition = new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously);

            // Creates a speech recognizer using microphone as audio input.
            using (var recognizer = new SpeechRecognizer(config))
            {
                // Subscribes to events.
                recognizer.Recognizing += (s, e) =>
                {
                    if (e.Result.Reason == ResultReason.RecognizingKeyword)
                    {
                        Console.WriteLine($"RECOGNIZING KEYWORD: Text={e.Result.Text}");
                    }
                    else if (e.Result.Reason == ResultReason.RecognizingSpeech)
                    {
                        Console.WriteLine($"RECOGNIZING: Text={e.Result.Text}");
                    }
                };

                recognizer.Recognized += (s, e) =>
                {
                    if (e.Result.Reason == ResultReason.RecognizedKeyword)
                    {
                        Console.WriteLine($"RECOGNIZED KEYWORD: Text={e.Result.Text}");
                    }
                    else if (e.Result.Reason == ResultReason.RecognizedSpeech)
                    {
                        Console.WriteLine($"RECOGNIZED: Text={e.Result.Text}");
                    }
                    else if (e.Result.Reason == ResultReason.NoMatch)
                    {
                        Console.WriteLine("NOMATCH: Speech could not be recognized.");
                    }
                };

                recognizer.Canceled += (s, e) =>
                {
                    Console.WriteLine($"CANCELED: Reason={e.Reason}");

                    if (e.Reason == CancellationReason.Error)
                    {
                        Console.WriteLine($"CANCELED: ErrorCode={e.ErrorCode}");
                        Console.WriteLine($"CANCELED: ErrorDetails={e.ErrorDetails}");
                        Console.WriteLine($"CANCELED: Did you update the subscription info?");
                    }
                    stopRecognition.TrySetResult(0);
                };

                recognizer.SessionStarted += (s, e) =>
                {
                    Console.WriteLine("\n    Session started event.");
                };

                recognizer.SessionStopped += (s, e) =>
                {
                    Console.WriteLine("\n    Session stopped event.");
                    Console.WriteLine("\nStop recognition.");
                    stopRecognition.TrySetResult(0);
                };

                // Starts recognizing.
                Console.WriteLine($"Say something starting with the keyword '{keyword}' followed by whatever you want...");

                // Starts continuous recognition using the keyword model. Use
                // StopKeywordRecognitionAsync() to stop recognition.
                await recognizer.StartKeywordRecognitionAsync(model).ConfigureAwait(false);

                // Waits for a single successful keyword-triggered speech recognition (or error).
                // Use Task.WaitAny to keep the task rooted.
                Task.WaitAny(new[] { stopRecognition.Task });

                // Stops recognition.
                await recognizer.StopKeywordRecognitionAsync().ConfigureAwait(false);
            }
        }

        // Continuous speech recognition assisted with a phrase list.
        public static async Task ContinuousRecognitionWithFileAndPhraseListsAsync()
        {
            // Creates an instance of a speech config with specified endpoint and subscription key.
            // Replace with your own endpoint and subscription key.
            var config = SpeechConfig.FromEndpoint(new Uri("https://YourServiceRegion.api.cognitive.microsoft.com"), "YourSubscriptionKey");

            var stopRecognition = new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously);

            // Creates a speech recognizer using file as audio input.
            // Replace with your own audio file name.
            using (var audioInput = AudioConfig.FromWavFileInput(@"wreck-a-nice-beach.wav"))
            {
                using (var recognizer = new SpeechRecognizer(config, audioInput))
                {
                    // Subscribes to events.
                    recognizer.Recognizing += (s, e) =>
                    {
                        Console.WriteLine($"RECOGNIZING: Text={e.Result.Text}");
                    };

                    recognizer.Recognized += (s, e) =>
                    {
                        if (e.Result.Reason == ResultReason.RecognizedSpeech)
                        {
                            Console.WriteLine($"RECOGNIZED: Text={e.Result.Text}");
                        }
                        else if (e.Result.Reason == ResultReason.NoMatch)
                        {
                            Console.WriteLine($"NOMATCH: Speech could not be recognized.");
                        }
                    };

                    recognizer.Canceled += (s, e) =>
                    {
                        Console.WriteLine($"CANCELED: Reason={e.Reason}");

                        if (e.Reason == CancellationReason.Error)
                        {
                            Console.WriteLine($"CANCELED: ErrorCode={e.ErrorCode}");
                            Console.WriteLine($"CANCELED: ErrorDetails={e.ErrorDetails}");
                            Console.WriteLine($"CANCELED: Did you update the subscription info?");
                        }

                        stopRecognition.TrySetResult(0);
                    };

                    recognizer.SessionStarted += (s, e) =>
                    {
                        Console.WriteLine("\n    Session started event.");
                    };

                    recognizer.SessionStopped += (s, e) =>
                    {
                        Console.WriteLine("\n    Session stopped event.");
                        Console.WriteLine("\nStop recognition.");
                        stopRecognition.TrySetResult(0);
                    };

                    // Before starting recognition, add a phrase list to help recognition.
                    PhraseListGrammar phraseListGrammar = PhraseListGrammar.FromRecognizer(recognizer);
                    phraseListGrammar.AddPhrase("Wreck a nice beach");

                    // Starts continuous recognition. Uses StopContinuousRecognitionAsync() to stop recognition.
                    await recognizer.StartContinuousRecognitionAsync().ConfigureAwait(false);

                    // Waits for completion.
                    // Use Task.WaitAny to keep the task rooted.
                    Task.WaitAny(new[] { stopRecognition.Task });

                    // Stops recognition.
                    await recognizer.StopContinuousRecognitionAsync().ConfigureAwait(false);
                }
            }
        }

        // Speech recognition with auto detection for source language
        public static async Task RecognitionWithAutoDetectSourceLanguageAsync()
        {
            // Creates an instance of a speech config with specified endpoint and subscription key.
            // Replace with your own endpoint and subscription key.
            var config = SpeechConfig.FromEndpoint(new Uri("https://YourServiceRegion.api.cognitive.microsoft.com"), "YourSubscriptionKey");

            // Creates an instance of AutoDetectSourceLanguageConfig with the 2 source language candidates
            // Currently this feature only supports 2 different language candidates
            // Replace the languages to be the language candidates for your speech. Please see https://docs.microsoft.com/azure/cognitive-services/speech-service/language-support for all supported languages
            var autoDetectSourceLanguageConfig = AutoDetectSourceLanguageConfig.FromLanguages(new string[] { "de-DE", "fr-FR" });

            var stopRecognition = new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously);

            // Creates a speech recognizer using the auto detect source language config, and the file as audio input.
            // Replace with your own audio file name.
            using (var audioInput = AudioConfig.FromWavFileInput(@"whatstheweatherlike.wav"))
            {
                using (var recognizer = new SpeechRecognizer(config, autoDetectSourceLanguageConfig, audioInput))
                {
                    // Subscribes to events.
                    recognizer.Recognizing += (s, e) =>
                    {
                        if (e.Result.Reason == ResultReason.RecognizingSpeech)
                        {
                            Console.WriteLine($"RECOGNIZING: Text={e.Result.Text}");
                            // Retrieve the detected language
                            var autoDetectSourceLanguageResult = AutoDetectSourceLanguageResult.FromResult(e.Result);
                            Console.WriteLine($"DETECTED: Language={autoDetectSourceLanguageResult.Language}");
                        }
                    };

                    recognizer.Recognized += (s, e) =>
                    {
                        if (e.Result.Reason == ResultReason.RecognizedSpeech)
                        {
                            Console.WriteLine($"RECOGNIZED: Text={e.Result.Text}");
                            // Retrieve the detected language
                            var autoDetectSourceLanguageResult = AutoDetectSourceLanguageResult.FromResult(e.Result);
                            Console.WriteLine($"DETECTED: Language={autoDetectSourceLanguageResult.Language}");
                        }
                        else if (e.Result.Reason == ResultReason.NoMatch)
                        {
                            Console.WriteLine($"NOMATCH: Speech could not be recognized.");
                        }
                    };

                    recognizer.Canceled += (s, e) =>
                    {
                        Console.WriteLine($"CANCELED: Reason={e.Reason}");

                        if (e.Reason == CancellationReason.Error)
                        {
                            Console.WriteLine($"CANCELED: ErrorCode={e.ErrorCode}");
                            Console.WriteLine($"CANCELED: ErrorDetails={e.ErrorDetails}");
                            Console.WriteLine($"CANCELED: Did you update the subscription info?");
                        }

                        stopRecognition.TrySetResult(0);
                    };

                    recognizer.SessionStarted += (s, e) =>
                    {
                        Console.WriteLine("\n    Session started event.");
                    };

                    recognizer.SessionStopped += (s, e) =>
                    {
                        Console.WriteLine("\n    Session stopped event.");
                        Console.WriteLine("\nStop recognition.");
                        stopRecognition.TrySetResult(0);
                    };

                    // Starts continuous recognition. Uses StopContinuousRecognitionAsync() to stop recognition.
                    await recognizer.StartContinuousRecognitionAsync().ConfigureAwait(false);

                    // Waits for completion.
                    // Use Task.WaitAny to keep the task rooted.
                    Task.WaitAny(new[] { stopRecognition.Task });

                    // Stops recognition.
                    await recognizer.StopContinuousRecognitionAsync().ConfigureAwait(false);
                }
            }
        }

        // Speech recognition with auto detection for source language and custom model
        public static async Task RecognitionWithAutoDetectSourceLanguageAndCustomModelAsync()
        {
            // Creates an instance of a speech config with specified endpoint and subscription key.
            // Replace with your own endpoint and subscription key.
            var config = SpeechConfig.FromEndpoint(new Uri("https://YourServiceRegion.api.cognitive.microsoft.com"), "YourSubscriptionKey");

            var sourceLanguageConfigs = new SourceLanguageConfig[]
            {
                // The endpoint id is optional, if not specified,  the service will use the default model for en-US
                // Replace the language with your source language candidate. Please see https://docs.microsoft.com/azure/cognitive-services/speech-service/language-support for all supported languages
                SourceLanguageConfig.FromLanguage("en-US"),

                // Replace the id with the CRIS endpoint id of your customized model. If the speech is in fr-FR, the service will use the corresponding customized model for speech recognition
                SourceLanguageConfig.FromLanguage("fr-FR", "YourEndpointId"),
            };

            // Creates an instance of AutoDetectSourceLanguageConfig with the 2 source language configurations
            // Currently this feature only supports 2 different language candidates
            var autoDetectSourceLanguageConfig = AutoDetectSourceLanguageConfig.FromSourceLanguageConfigs(sourceLanguageConfigs);

            var stopRecognition = new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously);

            // Creates a speech recognizer using the auto detect source language config, and the file as audio input.
            // Replace with your own audio file name.
            using (var audioInput = AudioConfig.FromWavFileInput(@"whatstheweatherlike.wav"))
            {
                using (var recognizer = new SpeechRecognizer(config, autoDetectSourceLanguageConfig, audioInput))
                {
                    recognizer.Recognizing += (s, e) =>
                    {
                        if (e.Result.Reason == ResultReason.RecognizingSpeech)
                        {
                            Console.WriteLine($"RECOGNIZING: Text={e.Result.Text}");
                            // Retrieve the detected language
                            var autoDetectSourceLanguageResult = AutoDetectSourceLanguageResult.FromResult(e.Result);
                            Console.WriteLine($"DETECTED: Language={autoDetectSourceLanguageResult.Language}");
                        }
                    };

                    recognizer.Recognized += (s, e) =>
                    {
                        if (e.Result.Reason == ResultReason.RecognizedSpeech)
                        {
                            Console.WriteLine($"RECOGNIZED: Text={e.Result.Text}");
                            // Retrieve the detected language
                            var autoDetectSourceLanguageResult = AutoDetectSourceLanguageResult.FromResult(e.Result);
                            Console.WriteLine($"DETECTED: Language={autoDetectSourceLanguageResult.Language}");
                        }
                        else if (e.Result.Reason == ResultReason.NoMatch)
                        {
                            Console.WriteLine($"NOMATCH: Speech could not be recognized.");
                        }
                    };

                    recognizer.Canceled += (s, e) =>
                    {
                        Console.WriteLine($"CANCELED: Reason={e.Reason}");

                        if (e.Reason == CancellationReason.Error)
                        {
                            Console.WriteLine($"CANCELED: ErrorCode={e.ErrorCode}");
                            Console.WriteLine($"CANCELED: ErrorDetails={e.ErrorDetails}");
                            Console.WriteLine($"CANCELED: Did you update the subscription info?");
                        }

                        stopRecognition.TrySetResult(0);
                    };

                    recognizer.SessionStarted += (s, e) =>
                    {
                        Console.WriteLine("\n    Session started event.");
                    };

                    recognizer.SessionStopped += (s, e) =>
                    {
                        Console.WriteLine("\n    Session stopped event.");
                        Console.WriteLine("\nStop recognition.");
                        stopRecognition.TrySetResult(0);
                    };

                    // Starts continuous recognition. Uses StopContinuousRecognitionAsync() to stop recognition.
                    await recognizer.StartContinuousRecognitionAsync().ConfigureAwait(false);

                    // Waits for completion.
                    // Use Task.WaitAny to keep the task rooted.
                    Task.WaitAny(new[] { stopRecognition.Task });

                    // Stops recognition.
                    await recognizer.StopContinuousRecognitionAsync().ConfigureAwait(false);
                }
            }
        }

        public static async Task KeywordRecognizer()
        {
            Console.WriteLine("say something ...");
            using (var audioInput = AudioConfig.FromDefaultMicrophoneInput())
            {
                using (var recognizer = new KeywordRecognizer(audioInput))
                {
                    var model = KeywordRecognitionModel.FromFile("YourKeywordRecognitionModelFile.table");
                    var result = await recognizer.RecognizeOnceAsync(model).ConfigureAwait(false);
                    Console.WriteLine($"got result reason as {result.Reason}");
                    if(result.Reason == ResultReason.RecognizedKeyword)
                    {
                        var stream = AudioDataStream.FromResult(result);

                        await Task.Delay(2000);

                        stream.DetachInput();
                        await stream.SaveToWaveFileAsync("AudioFromRecognizedKeyword.wav");
                    }
                    else
                    {
                        Console.WriteLine($"got result reason as {result.Reason}. You can't get audio when no keyword is recognized.");
                    }
                }
            }
        }

        // Pronunciation assessment with microphone as audio input.
        // See more information at https://aka.ms/csspeech/pa
        public static async Task PronunciationAssessmentWithMicrophoneAsync()
        {
            // Creates an instance of a speech config with specified endpoint and subscription key.
            // Replace with your own endpoint and subscription key.
            var config = SpeechConfig.FromEndpoint(new Uri("https://YourServiceRegion.api.cognitive.microsoft.com"), "YourSubscriptionKey");

            // Replace the language with your language in BCP-47 format, e.g., en-US.
            var language = "en-US";

            // The pronunciation assessment service has a longer default end silence timeout (5 seconds) than normal STT
            // as the pronunciation assessment is widely used in education scenario where kids have longer break in reading.
            // You can adjust the end silence timeout based on your real scenario.
            config.SetProperty(PropertyId.SpeechServiceConnection_EndSilenceTimeoutMs, "3000");

            var referenceText = "";
            // create pronunciation assessment config, set grading system, granularity and if enable miscue based on your requirement.
            var pronunciationConfig = new PronunciationAssessmentConfig(referenceText,
                GradingSystem.HundredMark, Granularity.Phoneme, true);

            pronunciationConfig.EnableProsodyAssessment();

            // Creates a speech recognizer for the specified language, using microphone as audio input.
            using (var recognizer = new SpeechRecognizer(config, language))
            {
                recognizer.SessionStarted += (s, e) => {
                    Console.WriteLine($"SESSION ID: {e.SessionId}");
                };

                while (true)
                {
                    // Receives reference text from console input.
                    Console.WriteLine("Enter reference text you want to assess, or enter empty text to exit.");
                    Console.Write("> ");
                    referenceText = Console.ReadLine();
                    if (string.IsNullOrEmpty(referenceText))
                    {
                        break;
                    }

                    pronunciationConfig.ReferenceText = referenceText;

                    // Starts recognizing.
                    Console.WriteLine($"Read out \"{referenceText}\" for pronunciation assessment ...");

                    pronunciationConfig.ApplyTo(recognizer);

                    // Starts speech recognition, and returns after a single utterance is recognized.
                    // For long-running multi-utterance recognition, use StartContinuousRecognitionAsync() instead.
                    var result = await recognizer.RecognizeOnceAsync().ConfigureAwait(false);

                    // Checks result.
                    if (result.Reason == ResultReason.RecognizedSpeech)
                    {
                        Console.WriteLine($"RECOGNIZED: Text={result.Text}");
                        Console.WriteLine("  PRONUNCIATION ASSESSMENT RESULTS:");

                        var pronunciationResult = PronunciationAssessmentResult.FromResult(result);
                        Console.WriteLine(
                            $"    Accuracy score: {pronunciationResult.AccuracyScore}, Prosody Score: {pronunciationResult.ProsodyScore}, Pronunciation score: {pronunciationResult.PronunciationScore}, Completeness score : {pronunciationResult.CompletenessScore}, FluencyScore: {pronunciationResult.FluencyScore}");

                        Console.WriteLine("  Word-level details:");

                        foreach (var word in pronunciationResult.Words)
                        {
                            Console.WriteLine($"    Word: {word.Word}, Accuracy score: {word.AccuracyScore}, Error type: {word.ErrorType}.");
                        }
                    }
                    else if (result.Reason == ResultReason.NoMatch)
                    {
                        Console.WriteLine($"NOMATCH: Speech could not be recognized.");
                    }
                    else if (result.Reason == ResultReason.Canceled)
                    {
                        var cancellation = CancellationDetails.FromResult(result);
                        Console.WriteLine($"CANCELED: Reason={cancellation.Reason}");

                        if (cancellation.Reason == CancellationReason.Error)
                        {
                            Console.WriteLine($"CANCELED: ErrorCode={cancellation.ErrorCode}");
                            Console.WriteLine($"CANCELED: ErrorDetails={cancellation.ErrorDetails}");
                            Console.WriteLine($"CANCELED: Did you update the subscription info?");
                        }
                    }
                }
            }
        }

        // Pronunciation assessment with audio stream input.
        // See more information at https://aka.ms/csspeech/pa
        public static void PronunciationAssessmentWithStream()
        {
            // Creates an instance of a speech config with specified endpoint and subscription key.
            // Replace with your own endpoint and subscription key.
            var config = SpeechConfig.FromEndpoint(new Uri("https://YourServiceRegion.api.cognitive.microsoft.com"), "YourSubscriptionKey");

            // Read audio data from file. In real scenario this can be from memory or network
            var audioDataWithHeader = File.ReadAllBytes("whatstheweatherlike.wav");
            var audioData = new byte[audioDataWithHeader.Length - 46];
            Array.Copy(audioDataWithHeader, 46, audioData, 0, audioData.Length);

            var resultReceived = new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously);
            var resultContainer = new List<string>();

            var startTime = DateTime.Now;

            var task = PronunciationAssessmentWithStreamInternalAsync(config, "what's the weather like", audioData, resultReceived, resultContainer);
            Task.WaitAny(new[] { resultReceived.Task });
            var resultJson = resultContainer[0];

            var endTime = DateTime.Now;

            Console.WriteLine(resultJson);

            var timeCost = endTime.Subtract(startTime).TotalMilliseconds;
            Console.WriteLine($"Time cost: {timeCost}ms");
        }

        private static async Task PronunciationAssessmentWithStreamInternalAsync(SpeechConfig speechConfig, string referenceText, byte[] audioData, TaskCompletionSource<int> resultReceived, List<string> resultContainer)
        {
            using (var audioInputStream = AudioInputStream.CreatePushStream(AudioStreamFormat.GetWaveFormatPCM(16000, 16, 1))) // This need be set based on the format of the given audio data
            using (var audioConfig = AudioConfig.FromStreamInput(audioInputStream))
            // Specify the language used for Pronunciation Assessment.
            using (var speechRecognizer = new SpeechRecognizer(speechConfig, "en-US", audioConfig))
            {
                // create pronunciation assessment config, set grading system, granularity and if enable miscue based on your requirement.
                var pronAssessmentConfig = new PronunciationAssessmentConfig(referenceText, GradingSystem.HundredMark, Granularity.Phoneme, false);

                pronAssessmentConfig.EnableProsodyAssessment();

                speechRecognizer.SessionStarted += (s, e) => {
                    Console.WriteLine($"SESSION ID: {e.SessionId}");
                };

                pronAssessmentConfig.ApplyTo(speechRecognizer);

                audioInputStream.Write(audioData);
                audioInputStream.Write(new byte[0]); // send a zero-size chunk to signal the end of stream

                var result = await speechRecognizer.RecognizeOnceAsync().ConfigureAwait(false);
                if (result.Reason == ResultReason.Canceled)
                {
                    var cancellationDetail = CancellationDetails.FromResult(result);
                    Console.Write(cancellationDetail);
                }
                else
                {
                    var responseJson = result.Properties.GetProperty(PropertyId.SpeechServiceResponse_JsonResult);
                    resultContainer.Add(responseJson);
                }

                resultReceived.SetResult(1);
            }
        }

        // Pronunciation assessment configured with json
        // See more information at https://aka.ms/csspeech/pa
        public static async Task PronunciationAssessmentConfiguredWithJson()
        {
            // Creates an instance of a speech config with specified endpoint and subscription key.
            // Replace with your own endpoint and subscription key.
            var config = SpeechConfig.FromEndpoint(new Uri("https://YourServiceRegion.api.cognitive.microsoft.com"), "YourSubscriptionKey");

            // Replace the language with your language in BCP-47 format, e.g., en-US.
            var language = "en-US";

            // Creates an instance of audio config from an audio file
            var audioConfig = AudioConfig.FromWavFileInput(@"whatstheweatherlike.wav");

            var referenceText = "what's the weather like";
            // create pronunciation assessment config, set grading system, granularity and if enable miscue based on your requirement.
            string json_config = "{\"GradingSystem\":\"HundredMark\",\"Granularity\":\"Phoneme\",\"EnableMiscue\":true, \"ScenarioId\":\"\"}";
            var pronunciationConfig = PronunciationAssessmentConfig.FromJson(json_config);
            pronunciationConfig.ReferenceText = referenceText;

            pronunciationConfig.EnableProsodyAssessment();

            // Creates a speech recognizer for the specified language
            using (var recognizer = new SpeechRecognizer(config, language, audioConfig))
            {

                recognizer.SessionStarted += (s, e) => {
                    Console.WriteLine($"SESSION ID: {e.SessionId}");
                };

                // Starts recognizing.
                pronunciationConfig.ApplyTo(recognizer);

                // Starts speech recognition, and returns after a single utterance is recognized.
                // For long-running multi-utterance recognition, use StartContinuousRecognitionAsync() instead.
                var result = await recognizer.RecognizeOnceAsync().ConfigureAwait(false);

                // Checks result.
                if (result.Reason == ResultReason.RecognizedSpeech)
                {
                    Console.WriteLine($"RECOGNIZED: Text={result.Text}");
                    Console.WriteLine("  PRONUNCIATION ASSESSMENT RESULTS:");

                    var pronunciationResult = PronunciationAssessmentResult.FromResult(result);
                    Console.WriteLine(
                        $"    Accuracy score: {pronunciationResult.AccuracyScore}, Prosody Score: {pronunciationResult.ProsodyScore}, Pronunciation score: {pronunciationResult.PronunciationScore}, Completeness score : {pronunciationResult.CompletenessScore}, FluencyScore: {pronunciationResult.FluencyScore}");

                    Console.WriteLine("  Word-level details:");

                    foreach (var word in pronunciationResult.Words)
                    {
                        Console.WriteLine($"    Word: {word.Word}, Accuracy score: {word.AccuracyScore}, Error type: {word.ErrorType}.");
                    }
                }
                else if (result.Reason == ResultReason.NoMatch)
                {
                    Console.WriteLine($"NOMATCH: Speech could not be recognized.");
                }
                else if (result.Reason == ResultReason.Canceled)
                {
                    var cancellation = CancellationDetails.FromResult(result);
                    Console.WriteLine($"CANCELED: Reason={cancellation.Reason}");

                    if (cancellation.Reason == CancellationReason.Error)
                    {
                        Console.WriteLine($"CANCELED: ErrorCode={cancellation.ErrorCode}");
                        Console.WriteLine($"CANCELED: ErrorDetails={cancellation.ErrorDetails}");
                        Console.WriteLine($"CANCELED: Did you update the subscription info?");
                    }
                }
            }
        }
        private static List<string> ConvertReferenceWords(string referenceText, List<string> referenceWords)
        {
            HashSet<string> dictionary = new HashSet<string>(referenceWords);
            int maxLength = dictionary.Max(word => word.Length);

            referenceText = RemovePunctuation(referenceText);
            return SegmentWord(referenceText, dictionary, maxLength);
        }

        private static string RemovePunctuation(string text)
        {
            // Remove punctuation from the reference text
            return new string(text.Where(c => char.IsLetterOrDigit(c) || char.IsWhiteSpace(c)).ToArray());
        }

        private static List<string> SegmentWord(string referenceText, HashSet<string> dictionary, int maxLength)
        {
            List<string> leftToRight = LeftToRightSegmentation(referenceText, dictionary, maxLength);
            List<string> rightToLeft = RightToLeftSegmentation(referenceText, dictionary, maxLength);

            if (string.Join("", leftToRight) == referenceText)
                return leftToRight;
            if (string.Join("", rightToLeft) == referenceText)
                return rightToLeft;

            Console.WriteLine("WW failed to segment the text with the dictionary");

            if (leftToRight.Count < rightToLeft.Count)
                return leftToRight;
            if (leftToRight.Count > rightToLeft.Count)
                return rightToLeft;

            // If the word number is the same, then return the one with the smallest single word
            int leftToRightSingle = leftToRight.Count(word => word.Length == 1);
            int rightToLeftSingle = rightToLeft.Count(word => word.Length == 1);
            return leftToRightSingle < rightToLeftSingle ? leftToRight : rightToLeft;
        }

        // From left to right to do the longest matching to get the word segmentation
        private static List<string> LeftToRightSegmentation(string text, HashSet<string> dictionary, int maxLength)
        {
            List<string> result = new List<string>();
            while (text.Length > 0)
            {
                // If the length of the text is less than the max_length, then the sub_text is the text itself
                string subText = text.Length < maxLength ? text : text.Substring(0, maxLength);
                while (subText.Length > 0)
                {
                    // If the sub_text is in the dictionary or the length of the sub_text is 1, then add it to the result
                    if (dictionary.Contains(subText) || subText.Length == 1)
                    {
                        result.Add(subText);
                        text = text.Substring(subText.Length);
                        break;
                    }
                    // # If the sub_text is not in the dictionary, then remove the last character of the sub_text
                    subText = subText.Substring(0, subText.Length - 1);
                }
            }
            return result;
        }

        // From right to left to do the longest matching to get the word segmentation
        private static List<string> RightToLeftSegmentation(string text, HashSet<string> dictionary, int maxLength)
        {
            List<string> result = new List<string>();
            while (text.Length > 0)
            {
                // If the length of the text is less than the max_length, then the sub_text is the text itself
                string subText = text.Length < maxLength ? text : text.Substring(text.Length - maxLength);
                while (subText.Length > 0)
                {
                    // If the sub_text is in the dictionary or the length of the sub_text is 1, then add it to the result
                    if (dictionary.Contains(subText) || subText.Length == 1)
                    {
                        result.Add(subText);
                        text = text.Substring(0, text.Length - subText.Length);
                        break;
                    }

                    // If the sub_text is not in the dictionary, then remove the first character of the sub_text
                    subText = subText.Substring(1);
                }
            }

            // Reverse the result to get the correct order
            result.Reverse();
            return result;
        }
        private static List<string> GetReferenceWords(string waveFilename, string referenceText, string language, SpeechConfig speechConfig)
        {
            var audioConfig = AudioConfig.FromWavFileInput(waveFilename);
            speechConfig.SpeechRecognitionLanguage = language;

            var speechRecognizer = new SpeechRecognizer(speechConfig, audioConfig);

            // Create pronunciation assessment config, set grading system, granularity, and enable miscue based on requirement
            bool enableMiscue = true;
            var pronunciationConfig = new PronunciationAssessmentConfig(referenceText,
                GradingSystem.HundredMark, Granularity.Phoneme, enableMiscue);

            // Apply pronunciation assessment config to speech recognizer
            pronunciationConfig.ApplyTo(speechRecognizer);

            // Perform speech recognition
            var result = speechRecognizer.RecognizeOnceAsync().Result;

            if (result.Reason == ResultReason.RecognizedSpeech)
            {
                var referenceWords = new List<string>();

                var responseJson = result.Properties.GetProperty(PropertyId.SpeechServiceResponse_JsonResult);
                // Parse the JSON result to extract NBest and Words
                JsonDocument doc = JsonDocument.Parse(responseJson);
                JsonElement root = doc.RootElement;


                JsonElement words = root.GetProperty("NBest")[0].GetProperty("Words");
                foreach (JsonElement item in words.EnumerateArray())
                {
                    string word_item = item.GetProperty("Word").GetString();
                    string errorType_item = item.GetProperty("PronunciationAssessment").GetProperty("ErrorType").GetString();

                    if (errorType_item != "Insertion")
                    {
                        referenceWords.Add(word_item);
                    }
                }

                return ConvertReferenceWords(referenceText, referenceWords);
            }
            else if (result.Reason == ResultReason.NoMatch)
            {
                Console.WriteLine("No speech could be recognized");
                return null;
            }
            else if (result.Reason == ResultReason.Canceled)
            {
                var cancellation = CancellationDetails.FromResult(result);
                Console.WriteLine($"Speech Recognition canceled: {cancellation.Reason}");
                if (cancellation.Reason == CancellationReason.Error)
                {
                    Console.WriteLine($"Error details: {cancellation.ErrorDetails}");
                }
                return null;
            }

            return null;
        }

        // Pronunciation assessment continous from file
        // See more information at https://aka.ms/csspeech/pa
        public static async Task PronunciationAssessmentContinuousWithFile()
        {
            // Creates an instance of a speech config with specified endpoint and subscription key.
            // Replace with your own endpoint and subscription key.
            var config = SpeechConfig.FromEndpoint(new Uri("https://YourServiceRegion.api.cognitive.microsoft.com"), "YourSubscriptionKey");
            var waveFileName = @"zhcn_continuous_mode_sample.wav";
            var scriptFileName = @"zhcn_continuous_mode_sample.txt";

            var referenceText = File.ReadAllText(scriptFileName);

            // You can adjust the segmentation silence timeout based on your real scenario.
            config.SetProperty(PropertyId.Speech_SegmentationSilenceTimeoutMs, "1500");

            // Switch to other languages for example Spanish, change language "en-US" to "es-ES". Language name is not case sensitive.
            var language = "zh-CN";
            if (language == "zh-CN")
            {
                Console.OutputEncoding = Encoding.UTF8;
            }

            // Creates a speech recognizer using file as audio input. 
            using (var audioInput = AudioConfig.FromWavFileInput(waveFileName))
            {

                using (var recognizer = new SpeechRecognizer(config, language, audioInput))
                {

                    bool enableMiscue = true;

                    var pronConfig = new PronunciationAssessmentConfig(referenceText, GradingSystem.HundredMark, Granularity.Phoneme, enableMiscue);

                    pronConfig.EnableProsodyAssessment();

                    recognizer.SessionStarted += (s, e) => {
                        Console.WriteLine($"SESSION ID: {e.SessionId}");
                    };

                    pronConfig.ApplyTo(recognizer);

                    var recognizedWords = new List<string>();
                    var pronWords = new List<Word>();
                    var finalWords = new List<Word>();
                    var prosody_scores = new List<double>();
                    var startOffset = 0L;
                    var endOffset = 0L;
                    var durations = new List<int>();
                    var done = false;

                    recognizer.SessionStopped += (s, e) => {
                        Console.WriteLine("ClOSING on {0}", e);
                        done = true;
                    };

                    recognizer.Canceled += (s, e) => {
                        Console.WriteLine("ClOSING on {0}", e);
                        done = true;
                    };

                    recognizer.Recognized += (s, e) => {
                        Console.WriteLine($"RECOGNIZED: Text={e.Result.Text}");
                        var pronResult = PronunciationAssessmentResult.FromResult(e.Result);
                        Console.WriteLine($"    Accuracy score: {pronResult.AccuracyScore}, prosody score:{pronResult.ProsodyScore}, pronunciation score: {pronResult.PronunciationScore}, completeness score: {pronResult.CompletenessScore}, fluency score: {pronResult.FluencyScore}");

                        prosody_scores.Add(pronResult.ProsodyScore);

                        foreach(var word in pronResult.Words)
                        {
                            var newWord = new Word(word.Word, word.ErrorType, word.AccuracyScore);
                            pronWords.Add(newWord);
                        }

                        foreach (var result in e.Result.Best())
                        {
                            durations.AddRange(result.Words.Select(item => item.Duration + 100000).ToList());
                            recognizedWords.AddRange(result.Words.Select(item => item.Word.ToLower()).ToList());

                            if (startOffset == 0) startOffset = result.Words.First().Offset;

                            endOffset = result.Words.Last().Offset + result.Words.Last().Duration + 100000;
                        }
                    };

                    // Starts continuous recognition.
                    await recognizer.StartContinuousRecognitionAsync().ConfigureAwait(false);

                    while (! done)
                    {
                        // Allow the program to run and process results continuously.
                        await Task.Delay(1000); // Adjust the delay as needed.
                    }

                    // Waits for completion.
                    await recognizer.StopContinuousRecognitionAsync().ConfigureAwait(false);

                    // set the duration of Word in pronWords
                    pronWords.Zip(durations, (word, duration) => word.Duration = duration).ToList();

                    // For continuous pronunciation assessment mode, the service won't return the words with `Insertion` or `Omission`
                    // even if miscue is enabled.
                    // We need to compare with the reference text after received all recognized words to get these error words.
                    string[] referenceWords;

                    if (language == "zh-CN")
                    {
                        // Split words for Chinese using the reference text and any short wave file
                        referenceWords = GetReferenceWords(@"zhcn_short_dummy_sample.wav", referenceText, language, config).ToArray();
                    }
                    else
                    {
                        referenceWords = referenceText.ToLower().Split(' ');
                        for (int j = 0; j < referenceWords.Length; j++)
                        {
                            referenceWords[j] = Regex.Replace(referenceWords[j], "^[\\p{P}\\s]+|[\\p{P}\\s]+$", "");
                        }
                    }

                    if (enableMiscue)
                    {
                        var differ = new Differ();
                        var inlineBuilder = new InlineDiffBuilder(differ);
                        var diffModel = inlineBuilder.BuildDiffModel(string.Join("\n", referenceWords), string.Join("\n", recognizedWords));

                        int currentIdx = 0;

                        foreach (var delta in diffModel.Lines)
                        {
                            if (delta.Type == ChangeType.Unchanged)
                            {
                                finalWords.Add(pronWords[currentIdx]);

                                currentIdx += 1;
                            }

                            if (delta.Type == ChangeType.Deleted || delta.Type == ChangeType.Modified)
                            {
                                var word = new Word(delta.Text, "Omission");
                                finalWords.Add(word);
                            }

                            if (delta.Type == ChangeType.Inserted || delta.Type == ChangeType.Modified)
                            {
                                Word w = new Word(pronWords[currentIdx].WordText, pronWords[currentIdx].ErrorType, pronWords[currentIdx].AccuracyScore, pronWords[currentIdx].Duration);
                                if (w.ErrorType == "None")
                                {
                                    w.ErrorType = "Insertion";
                                    finalWords.Add(w);
                                }

                                currentIdx += 1;
                            }
                        }
                    }
                    else
                    {
                        finalWords = pronWords;
                    }

                    // We can calculate whole accuracy by averaging
                    var filteredWords = finalWords.Where(item => item.ErrorType != "Insertion");
                    var accuracyScore = filteredWords.Sum(item => item.AccuracyScore) / filteredWords.Count();

                    // Recalculate the prosody score by averaging
                    var prosodyScore = prosody_scores.Average();

                    // Recalculate fluency score
                    var durations_sum = finalWords.Where(item => item.ErrorType == "None")
                        .Sum(item => item.Duration);

                    var fluencyScore = durations_sum * 1.0 / (endOffset - startOffset) * 100;

                    // Calculate whole completeness score
                    var completenessScore = (double)finalWords.Count(item => item.ErrorType == "None") / filteredWords.Count() * 100;
                    completenessScore = completenessScore <= 100 ? completenessScore : 100;

                    List<double> scores_list = new List<double> {accuracyScore, prosodyScore, completenessScore, fluencyScore };

                    double pronunciationScore = scores_list.Sum(n => n * 0.2) + scores_list.Min() * 0.2;

                    Console.WriteLine("Paragraph accuracy score: {0}, prosody score: {1} completeness score: {2}, fluency score: {3}, pronunciation score: {4}", accuracyScore, prosodyScore, completenessScore, fluencyScore, pronunciationScore);

                    for (int idx = 0; idx < finalWords.Count(); idx++)
                    {
                        Word word = finalWords[idx];
                        Console.WriteLine("{0}: word: {1}\taccuracy score: {2}\terror type: {3}",
                            idx + 1, word.WordText, word.AccuracyScore, word.ErrorType);
                    }
                }
            }
        }

        // Pronunciation assessment with Microsoft Audio Stack (MAS) enabled
        // See more information at https://aka.ms/csspeech/pa
        public static async Task PronunciationAssessmentWithMas()
        {
            // Creates an instance of a speech config with specified endpoint and subscription key.
            // Replace with your own endpoint and subscription key.
            var config = SpeechConfig.FromEndpoint(new Uri("https://YourServiceRegion.api.cognitive.microsoft.com"), "YourSubscriptionKey");

            // Replace the language with your language in BCP-47 format, e.g., en-US.
            var language = "en-US";

            // Creates an instance of audio processing options with the default settings
            var audioProcessingOptions = AudioProcessingOptions.Create(
                AudioProcessingConstants.AUDIO_INPUT_PROCESSING_DISABLE_ECHO_CANCELLATION |
                AudioProcessingConstants.AUDIO_INPUT_PROCESSING_ENABLE_DEFAULT,
                PresetMicrophoneArrayGeometry.Mono);

            // Creates an instance of audio config from an audio file
            var audioConfig = AudioConfig.FromWavFileInput(@"whatstheweatherlike.wav", audioProcessingOptions);

            var referenceText = "what's the weather like";

            // Create pronunciation assessment config, set grading system, granularity and if enable miscue based on your requirement.
            var pronunciationConfig = new PronunciationAssessmentConfig(referenceText, GradingSystem.HundredMark, Granularity.Phoneme, enableMiscue: true);

            // Enable prosody assessment
            pronunciationConfig.EnableProsodyAssessment();

            // Creates a speech recognizer for the specified language
            using (var recognizer = new SpeechRecognizer(config, language, audioConfig))
            {
                // Starts recognizing.
                pronunciationConfig.ApplyTo(recognizer);

                // Starts speech recognition, and returns after a single utterance is recognized.
                // For long-running multi-utterance recognition, use StartContinuousRecognitionAsync() instead.
                var result = await recognizer.RecognizeOnceAsync().ConfigureAwait(false);

                // Checks result.
                if (result.Reason == ResultReason.RecognizedSpeech)
                {
                    Console.WriteLine($"RECOGNIZED: Text={result.Text}");
                    Console.WriteLine("  PRONUNCIATION ASSESSMENT RESULTS:");

                    var pronunciationResult = PronunciationAssessmentResult.FromResult(result);
                    Console.WriteLine(
                        $"    Accuracy score: {pronunciationResult.AccuracyScore}, Prosody Score: {pronunciationResult.ProsodyScore}, Pronunciation score: {pronunciationResult.PronunciationScore}, Completeness score : {pronunciationResult.CompletenessScore}, FluencyScore: {pronunciationResult.FluencyScore}");

                    Console.WriteLine("  Word-level details:");

                    foreach (var word in pronunciationResult.Words)
                    {
                        Console.WriteLine($"    Word: {word.Word}, Accuracy score: {word.AccuracyScore}, Error type: {word.ErrorType}.");
                    }
                }
                else if (result.Reason == ResultReason.NoMatch)
                {
                    Console.WriteLine($"NOMATCH: Speech could not be recognized.");
                }
                else if (result.Reason == ResultReason.Canceled)
                {
                    var cancellation = CancellationDetails.FromResult(result);
                    Console.WriteLine($"CANCELED: Reason={cancellation.Reason}");

                    if (cancellation.Reason == CancellationReason.Error)
                    {
                        Console.WriteLine($"CANCELED: ErrorCode={cancellation.ErrorCode}");
                        Console.WriteLine($"CANCELED: ErrorDetails={cancellation.ErrorDetails}");
                        Console.WriteLine($"CANCELED: Did you update the subscription info?");
                    }
                }
            }
        }

        private static async Task<RecognitionResult> RecognizeOnceAsyncInternal(string key, string region)
        {
            RecognitionResult recognitionResult = null;
            var config = SpeechConfig.FromEndpoint(new Uri($"https://{region}.api.cognitive.microsoft.com"), key);

            // Creates a speech recognizer using file as audio input.
            using (var audioInput = AudioConfig.FromWavFileInput(@"whatstheweatherlike.wav"))
            {
                using (var recognizer = new SpeechRecognizer(config, audioInput))
                {
                    recognitionResult = await recognizer.RecognizeOnceAsync().ConfigureAwait(false);
                }
            }

            return recognitionResult;
        }

        // Speech recognition with backup subscription region.
        public static async Task RecognitionOnceWithFileAsyncSwitchSecondaryRegion()
        {
            // Create a speech resource with primary subscription key and service region.
            // Also create a speech resource with secondary subscription key and service region
            RecognitionResult recognitionResult = await RecognizeOnceAsyncInternal("YourPrimarySubscriptionKey", "YourPrimaryServiceRegion");
            if (recognitionResult.Reason == ResultReason.Canceled)
            {
                CancellationDetails details = CancellationDetails.FromResult(recognitionResult);
                if (details.ErrorCode == CancellationErrorCode.ConnectionFailure
                    || details.ErrorCode == CancellationErrorCode.ServiceUnavailable
                    || details.ErrorCode == CancellationErrorCode.ServiceTimeout)
                {
                    recognitionResult = await RecognizeOnceAsyncInternal("YourSecondarySubscriptionKey", "YourSecondaryServiceRegion");
                }
            }
            Console.WriteLine("Recognized {0}", recognitionResult.Text);
        }

        // Speech recognition from default microphone with Microsoft Audio Stack enabled.
        public static async Task ContinuousRecognitionFromDefaultMicrophoneWithMASEnabled()
        {
            // Creates an instance of a speech config with specified endpoint and subscription key.
            // Replace with your own endpoint and subscription key.
            var config = SpeechConfig.FromEndpoint(new Uri("https://YourServiceRegion.api.cognitive.microsoft.com"), "YourSubscriptionKey");

            var stopRecognition = new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously);

            // Creates an instance of audio config using default microphone as audio input and with audio processing options specified.
            // All default enhancements from Microsoft Audio Stack are enabled.
            // Only works when input is from a microphone array.
            // On Windows, microphone array geometry is obtained from the driver. On other operating systems, a single channel (mono)
            // microphone is assumed.
            using (var audioProcessingOptions = AudioProcessingOptions.Create(AudioProcessingConstants.AUDIO_INPUT_PROCESSING_ENABLE_DEFAULT))
            using (var audioInput = AudioConfig.FromDefaultMicrophoneInput(audioProcessingOptions))
            {
                // Creates a speech recognizer.
                using (var recognizer = new SpeechRecognizer(config, audioInput))
                {
                    // Subscribes to events.
                    recognizer.Recognizing += (s, e) =>
                    {
                        Console.WriteLine($"RECOGNIZING: Text={e.Result.Text}");
                    };

                    recognizer.Recognized += (s, e) =>
                    {
                        if (e.Result.Reason == ResultReason.RecognizedSpeech)
                        {
                            Console.WriteLine($"RECOGNIZED: Text={e.Result.Text}");
                        }
                        else if (e.Result.Reason == ResultReason.NoMatch)
                        {
                            Console.WriteLine($"NOMATCH: Speech could not be recognized.");
                        }
                    };

                    recognizer.Canceled += (s, e) =>
                    {
                        Console.WriteLine($"CANCELED: Reason={e.Reason}");

                        if (e.Reason == CancellationReason.Error)
                        {
                            Console.WriteLine($"CANCELED: ErrorCode={e.ErrorCode}");
                            Console.WriteLine($"CANCELED: ErrorDetails={e.ErrorDetails}");
                            Console.WriteLine($"CANCELED: Did you update the subscription info?");
                        }

                        stopRecognition.TrySetResult(0);
                    };

                    recognizer.SessionStarted += (s, e) =>
                    {
                        Console.WriteLine("\n    Session started event.");
                    };

                    recognizer.SessionStopped += (s, e) =>
                    {
                        Console.WriteLine("\n    Session stopped event.");
                        Console.WriteLine("\nStop recognition.");
                        stopRecognition.TrySetResult(0);
                    };

                    // Starts continuous recognition. Uses StopContinuousRecognitionAsync() to stop recognition.
                    await recognizer.StartContinuousRecognitionAsync().ConfigureAwait(false);

                    Console.WriteLine("Say something (press Enter to stop)...");
                    Console.ReadKey();

                    // Stops recognition.
                    await recognizer.StopContinuousRecognitionAsync().ConfigureAwait(false);
                }
            }
        }

        // Speech recognition from a microphone with Microsoft Audio Stack enabled and pre-defined microphone array geometry specified.
        public static async Task RecognitionFromMicrophoneWithMASEnabledAndPresetGeometrySpecified()
        {
            // Creates an instance of a speech config with specified endpoint and subscription key.
            // Replace with your own endpoint and subscription key.
            var config = SpeechConfig.FromEndpoint(new Uri("https://YourServiceRegion.api.cognitive.microsoft.com"), "YourSubscriptionKey");

            // Creates an instance of audio config using a microphone as audio input and with audio processing options specified.
            // All default enhancements from Microsoft Audio Stack are enabled and preset microphone array geometry is specified
            // in audio processing options.
            using (var audioProcessingOptions = AudioProcessingOptions.Create(AudioProcessingConstants.AUDIO_INPUT_PROCESSING_ENABLE_DEFAULT,
                                                                              PresetMicrophoneArrayGeometry.Linear2))
            using (var audioInput = AudioConfig.FromMicrophoneInput("<device id>", audioProcessingOptions))
            {
                // Creates a speech recognizer.
                using (var recognizer = new SpeechRecognizer(config, audioInput))
                {
                    // Starts recognizing.
                    Console.WriteLine("Say something...");

                    // Starts speech recognition, and returns after a single utterance is recognized. The end of a
                    // single utterance is determined by listening for silence at the end or until a maximum of about 30
                    // seconds of audio is processed.  The task returns the recognition text as result.
                    // Note: Since RecognizeOnceAsync() returns only a single utterance, it is suitable only for single
                    // shot recognition like command or query.
                    // For long-running multi-utterance recognition, use StartContinuousRecognitionAsync() instead.
                    var result = await recognizer.RecognizeOnceAsync().ConfigureAwait(false);

                    // Checks result.
                    if (result.Reason == ResultReason.RecognizedSpeech)
                    {
                        Console.WriteLine($"RECOGNIZED: Text={result.Text}");
                    }
                    else if (result.Reason == ResultReason.NoMatch)
                    {
                        Console.WriteLine($"NOMATCH: Speech could not be recognized.");
                    }
                    else if (result.Reason == ResultReason.Canceled)
                    {
                        var cancellation = CancellationDetails.FromResult(result);
                        Console.WriteLine($"CANCELED: Reason={cancellation.Reason}");

                        if (cancellation.Reason == CancellationReason.Error)
                        {
                            Console.WriteLine($"CANCELED: ErrorCode={cancellation.ErrorCode}");
                            Console.WriteLine($"CANCELED: ErrorDetails={cancellation.ErrorDetails}");
                            Console.WriteLine($"CANCELED: Did you update the subscription info?");
                        }
                    }
                }
            }
        }

        // Speech recognition from multi-channel file with Microsoft Audio Stack enabled and custom microphone array geometry specified.
        public static async Task ContinuousRecognitionFromMultiChannelFileWithMASEnabledAndCustomGeometrySpecified()
        {
            // Creates an instance of a speech config with specified endpoint and subscription key.
            // Replace with your own endpoint and subscription key.
            var config = SpeechConfig.FromEndpoint(new Uri("https://YourServiceRegion.api.cognitive.microsoft.com"), "YourSubscriptionKey");

            var stopRecognition = new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously);

            // Approximate coordinates for a microphone array with one microphone in the center and six microphones evenly spaced
            // in a circle with radius approximately equal to 42.5 mm.
            MicrophoneCoordinates[] microphoneCoordinates = new MicrophoneCoordinates[7]
            {
                new MicrophoneCoordinates(0, 0, 0),
                new MicrophoneCoordinates(40, 0, 0),
                new MicrophoneCoordinates(20, -35, 0),
                new MicrophoneCoordinates(-20, -35, 0),
                new MicrophoneCoordinates(-40, 0, 0),
                new MicrophoneCoordinates(-20, 35, 0),
                new MicrophoneCoordinates(20, 35, 0)
            };

            // Creates an instance of microphone array geometry with microphone coordinates.
            var microphoneArrayGeometry = new MicrophoneArrayGeometry(MicrophoneArrayType.Planar, microphoneCoordinates);

            // Creates an instance of audio config using multi-channel WAV file as audio input and with audio processing options specified.
            // All default enhancements from Microsoft Audio Stack are enabled and custom microphone array geometry is provided.
            using (var audioProcessingOptions = AudioProcessingOptions.Create(AudioProcessingConstants.AUDIO_INPUT_PROCESSING_ENABLE_DEFAULT,
                                                                              microphoneArrayGeometry,
                                                                              SpeakerReferenceChannel.LastChannel))
            using (var audioInput = AudioConfig.FromWavFileInput("katiesteve.wav", audioProcessingOptions))
            {
                // Creates a speech recognizer.
                using (var recognizer = new SpeechRecognizer(config, audioInput))
                {
                    // Subscribes to events.
                    recognizer.Recognizing += (s, e) =>
                    {
                        Console.WriteLine($"RECOGNIZING: Text={e.Result.Text}");
                    };

                    recognizer.Recognized += (s, e) =>
                    {
                        if (e.Result.Reason == ResultReason.RecognizedSpeech)
                        {
                            Console.WriteLine($"RECOGNIZED: Text={e.Result.Text}");
                        }
                        else if (e.Result.Reason == ResultReason.NoMatch)
                        {
                            Console.WriteLine($"NOMATCH: Speech could not be recognized.");
                        }
                    };

                    recognizer.Canceled += (s, e) =>
                    {
                        Console.WriteLine($"CANCELED: Reason={e.Reason}");

                        if (e.Reason == CancellationReason.Error)
                        {
                            Console.WriteLine($"CANCELED: ErrorCode={e.ErrorCode}");
                            Console.WriteLine($"CANCELED: ErrorDetails={e.ErrorDetails}");
                            Console.WriteLine($"CANCELED: Did you update the subscription info?");
                        }

                        stopRecognition.TrySetResult(0);
                    };

                    recognizer.SessionStarted += (s, e) =>
                    {
                        Console.WriteLine("\n    Session started event.");
                    };

                    recognizer.SessionStopped += (s, e) =>
                    {
                        Console.WriteLine("\n    Session stopped event.");
                        Console.WriteLine("\nStop recognition.");
                        stopRecognition.TrySetResult(0);
                    };

                    // Starts continuous recognition. Uses StopContinuousRecognitionAsync() to stop recognition.
                    await recognizer.StartContinuousRecognitionAsync().ConfigureAwait(false);

                    // Waits for completion.
                    // Use Task.WaitAny to keep the task rooted.
                    Task.WaitAny(new[] { stopRecognition.Task });

                    // Stops recognition.
                    await recognizer.StopContinuousRecognitionAsync().ConfigureAwait(false);
                }
            }
        }

        // Speech recognition from pull stream with custom set of enhancements from Microsoft Audio Stack enabled.
        public static async Task RecognitionFromPullStreamWithSelectMASEnhancementsEnabled()
        {
            // Creates an instance of a speech config with specified endpoint and subscription key.
            // Replace with your own endpoint and subscription key.
            var config = SpeechConfig.FromEndpoint(new Uri("https://YourServiceRegion.api.cognitive.microsoft.com"), "YourSubscriptionKey");

            // Creates an instance of audio config with pull stream as audio input and with audio processing options specified.
            // All default enhancements from Microsoft Audio Stack are enabled except acoustic echo cancellation and preset
            // microphone array geometry is specified in audio processing options.
            using (var audioProcessingOptions = AudioProcessingOptions.Create(AudioProcessingConstants.AUDIO_INPUT_PROCESSING_ENABLE_DEFAULT |
                                                                              AudioProcessingConstants.AUDIO_INPUT_PROCESSING_DISABLE_ECHO_CANCELLATION,
                                                                              PresetMicrophoneArrayGeometry.Mono))
            using (var audioInput = Helper.OpenWavFile("whatstheweatherlike.wav", audioProcessingOptions))
            {
                // Creates a speech recognizer.
                using (var recognizer = new SpeechRecognizer(config, audioInput))
                {
                    // Starts speech recognition, and returns after a single utterance is recognized. The end of a
                    // single utterance is determined by listening for silence at the end or until a maximum of about 30
                    // seconds of audio is processed.  The task returns the recognition text as result.
                    // Note: Since RecognizeOnceAsync() returns only a single utterance, it is suitable only for single
                    // shot recognition like command or query.
                    // For long-running multi-utterance recognition, use StartContinuousRecognitionAsync() instead.
                    var result = await recognizer.RecognizeOnceAsync().ConfigureAwait(false);

                    // Checks result.
                    if (result.Reason == ResultReason.RecognizedSpeech)
                    {
                        Console.WriteLine($"RECOGNIZED: Text={result.Text}");
                    }
                    else if (result.Reason == ResultReason.NoMatch)
                    {
                        Console.WriteLine($"NOMATCH: Speech could not be recognized.");
                    }
                    else if (result.Reason == ResultReason.Canceled)
                    {
                        var cancellation = CancellationDetails.FromResult(result);
                        Console.WriteLine($"CANCELED: Reason={cancellation.Reason}");

                        if (cancellation.Reason == CancellationReason.Error)
                        {
                            Console.WriteLine($"CANCELED: ErrorCode={cancellation.ErrorCode}");
                            Console.WriteLine($"CANCELED: ErrorDetails={cancellation.ErrorDetails}");
                            Console.WriteLine($"CANCELED: Did you update the subscription info?");
                        }
                    }
                }
            }
        }

        // Speech recognition from push stream with Microsoft Audio Stack enabled and beamforming angles specified.
        public static async Task ContinuousRecognitionFromPushStreamWithMASEnabledAndBeamformingAnglesSpecified()
        {
            // Creates an instance of a speech config with specified endpoint and subscription key.
            // Replace with your own endpoint and subscription key.
            var config = SpeechConfig.FromEndpoint(new Uri("https://YourServiceRegion.api.cognitive.microsoft.com"), "YourSubscriptionKey");

            var stopRecognition = new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously);

            // Approximate coordinates for a microphone array with one microphone in the center and six microphones evenly spaced
            // in a circle with radius approximately equal to 42.5 mm.
            MicrophoneCoordinates[] microphoneCoordinates = new MicrophoneCoordinates[7]
            {
                new MicrophoneCoordinates(0, 0, 0),
                new MicrophoneCoordinates(40, 0, 0),
                new MicrophoneCoordinates(20, -35, 0),
                new MicrophoneCoordinates(-20, -35, 0),
                new MicrophoneCoordinates(-40, 0, 0),
                new MicrophoneCoordinates(-20, 35, 0),
                new MicrophoneCoordinates(20, 35, 0)
            };

            // Creates an instance of microphone array geometry with beamforming angles and microphone coordinates.
            var microphoneArrayGeometry = new MicrophoneArrayGeometry(MicrophoneArrayType.Planar, 70, 110, microphoneCoordinates);

            // Create the push stream to push audio to.
            using (var pushStream = AudioInputStream.CreatePushStream(AudioStreamFormat.GetWaveFormatPCM(16000, (byte)16, (byte)8)))
            {
                // Creates an instance of audio config with push stream as audio input and with audio processing options specified.
                // All default enhancements from Microsoft Audio Stack are enabled and custom microphone array geometry with beamforming
                // angles is specified.
                using (var audioProcessingOptions = AudioProcessingOptions.Create(AudioProcessingConstants.AUDIO_INPUT_PROCESSING_ENABLE_DEFAULT,
                                                                                  microphoneArrayGeometry,
                                                                                  SpeakerReferenceChannel.LastChannel))
                using(var audioInput = AudioConfig.FromStreamInput(pushStream, audioProcessingOptions))
                {
                    // Creates a speech recognizer using audio stream input.
                    using (var recognizer = new SpeechRecognizer(config, audioInput))
                    {
                        // Subscribes to events.
                        recognizer.Recognizing += (s, e) =>
                        {
                            Console.WriteLine($"RECOGNIZING: Text={e.Result.Text}");
                        };

                        recognizer.Recognized += (s, e) =>
                        {
                            if (e.Result.Reason == ResultReason.RecognizedSpeech)
                            {
                                Console.WriteLine($"RECOGNIZED: Text={e.Result.Text}");
                            }
                            else if (e.Result.Reason == ResultReason.NoMatch)
                            {
                                Console.WriteLine($"NOMATCH: Speech could not be recognized.");
                            }
                        };

                        recognizer.Canceled += (s, e) =>
                        {
                            Console.WriteLine($"CANCELED: Reason={e.Reason}");

                            if (e.Reason == CancellationReason.Error)
                            {
                                Console.WriteLine($"CANCELED: ErrorCode={e.ErrorCode}");
                                Console.WriteLine($"CANCELED: ErrorDetails={e.ErrorDetails}");
                                Console.WriteLine($"CANCELED: Did you update the subscription info?");
                            }

                            stopRecognition.TrySetResult(0);
                        };

                        recognizer.SessionStarted += (s, e) =>
                        {
                            Console.WriteLine("\nSession started event.");
                        };

                        recognizer.SessionStopped += (s, e) =>
                        {
                            Console.WriteLine("\nSession stopped event.");
                            Console.WriteLine("\nStop recognition.");
                            stopRecognition.TrySetResult(0);
                        };

                        // Starts continuous recognition. Uses StopContinuousRecognitionAsync() to stop recognition.
                        await recognizer.StartContinuousRecognitionAsync().ConfigureAwait(false);

                        // Open and read the wave file and push the buffers into the recognizer
                        using (BinaryAudioStreamReader reader = Helper.CreateBinaryFileReader("katiesteve.wav"))
                        {
                            byte[] buffer = new byte[1000];
                            while (true)
                            {
                                var readSamples = reader.Read(buffer, (uint)buffer.Length);
                                if (readSamples == 0)
                                {
                                    break;
                                }
                                pushStream.Write(buffer, readSamples);
                            }
                        }
                        pushStream.Close();

                        // Waits for completion.
                        // Use Task.WaitAny to keep the task rooted.
                        Task.WaitAny(new[] { stopRecognition.Task });

                        // Stops recognition.
                        await recognizer.StopContinuousRecognitionAsync().ConfigureAwait(false);
                    }
                }
            }
        }

        // Speech continous recognition authenticated via aad token crendential.
        public static async Task RecognitionContiniusAADTokenCredentialAsync()
        {
            // Create a token credential using DefaultAzureCredential.
            // This credential supports multiple authentication methods, including Managed Identity, environment variables, and Azure CLI login.
            // For more types of token credentials, refer to: 
            // https://learn.microsoft.com/dotnet/api/azure.identity.defaultazurecredential?view=azure-dotnet
            TokenCredential credential = new DefaultAzureCredential(new DefaultAzureCredentialOptions());

            // Define the v2 endpoint for Azure Speech Service.
            // This is required when using a private endpoint with a custom domain.
            // For details on setting up a custom domain with private links, see: 
            // https://learn.microsoft.com/azure/ai-services/speech-service/speech-services-private-link?tabs=portal#create-a-custom-domain-name
            var v2Endpoint = string.Format("wss://{custom domain}/stt/speech/universal/v2");

            // Create a SpeechConfig instance using the v2 endpoint and the token credential for authentication.
            var config = SpeechConfig.FromEndpoint(new Uri(v2Endpoint), credential);

            var stopRecognition = new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously);

            // Create a push stream
            using (var pushStream = AudioInputStream.CreatePushStream())
            {
                using (var audioInput = AudioConfig.FromStreamInput(pushStream))
                {
                    // Creates a speech recognizer using audio stream input.
                    using (var recognizer = new SpeechRecognizer(config, audioInput))
                    {
                        // Subscribes to events.
                        recognizer.Recognizing += (s, e) =>
                        {
                            Console.WriteLine($"RECOGNIZING: Text={e.Result.Text}");
                        };

                        recognizer.Recognized += (s, e) =>
                        {
                            if (e.Result.Reason == ResultReason.RecognizedSpeech)
                            {
                                Console.WriteLine($"RECOGNIZED: Text={e.Result.Text}");
                            }
                            else if (e.Result.Reason == ResultReason.NoMatch)
                            {
                                Console.WriteLine($"NOMATCH: Speech could not be recognized.");
                            }
                        };

                        recognizer.Canceled += (s, e) =>
                        {
                            Console.WriteLine($"CANCELED: Reason={e.Reason}");

                            if (e.Reason == CancellationReason.Error)
                            {
                                Console.WriteLine($"CANCELED: ErrorCode={e.ErrorCode}");
                                Console.WriteLine($"CANCELED: ErrorDetails={e.ErrorDetails}");
                                Console.WriteLine($"CANCELED: Did you update the subscription info?");
                            }

                            stopRecognition.TrySetResult(0);
                        };

                        recognizer.SessionStarted += (s, e) =>
                        {
                            Console.WriteLine("\nSession started event.");
                        };

                        recognizer.SessionStopped += (s, e) =>
                        {
                            Console.WriteLine("\nSession stopped event.");
                            Console.WriteLine("\nStop recognition.");
                            stopRecognition.TrySetResult(0);
                        };

                        // Starts continuous recognition. Uses StopContinuousRecognitionAsync() to stop recognition.
                        await recognizer.StartContinuousRecognitionAsync().ConfigureAwait(false);

                        // open and read the wave file and push the buffers into the recognizer
                        using (BinaryAudioStreamReader reader = Helper.CreateWavReader(@"whatstheweatherlike.wav"))
                        {
                            byte[] buffer = new byte[1000];
                            while (true)
                            {
                                var readSamples = reader.Read(buffer, (uint)buffer.Length);
                                if (readSamples == 0)
                                {
                                    break;
                                }
                                pushStream.Write(buffer, readSamples);
                            }
                        }
                        pushStream.Close();

                        // Waits for completion.
                        // Use Task.WaitAny to keep the task rooted.
                        Task.WaitAny(new[] { stopRecognition.Task });

                        // Stops recognition.
                        await recognizer.StopContinuousRecognitionAsync().ConfigureAwait(false);
                    }
                }
            }
        }

        // Speech once recognition authenticated via aad token credential.
        public static async Task RecognitionOnceAADTokenCredentialAsync()
        {
            // Create a token credential using DefaultAzureCredential.
            // This credential supports multiple authentication methods, including Managed Identity, environment variables, and Azure CLI login.
            // For more types of token credentials, refer to: 
            // https://learn.microsoft.com/dotnet/api/azure.identity.defaultazurecredential?view=azure-dotnet
            TokenCredential credential = new DefaultAzureCredential(new DefaultAzureCredentialOptions());

            // Define the v2 endpoint for Azure Speech Service.
            // This is required when using a private endpoint with a custom domain.
            // For details on setting up a custom domain with private links, see: 
            // https://learn.microsoft.com/azure/ai-services/speech-service/speech-services-private-link?tabs=portal#create-a-custom-domain-name
            var v2Endpoint = string.Format("wss://{custom domain}/stt/speech/universal/v2");

            // Create a SpeechConfig instance using the v2 endpoint and the token credential for authentication.
            var config = SpeechConfig.FromEndpoint(new Uri(v2Endpoint), credential);

            // Creates a speech recognizer using microphone as audio input.
            using (var recognizer = new SpeechRecognizer(config))
            {
                // Starts recognizing.
                Console.WriteLine("Say something...");

                // Starts speech recognition, and returns after a single utterance is recognized. The end of a
                // single utterance is determined by listening for silence at the end or until a maximum of about 30
                // seconds of audio is processed.  The task returns the recognition text as result.
                // Note: Since RecognizeOnceAsync() returns only a single utterance, it is suitable only for single
                // shot recognition like command or query.
                // For long-running multi-utterance recognition, use StartContinuousRecognitionAsync() instead.
                var result = await recognizer.RecognizeOnceAsync().ConfigureAwait(false);

                // Checks result.
                if (result.Reason == ResultReason.RecognizedSpeech)
                {
                    Console.WriteLine($"RECOGNIZED: Text={result.Text}");
                }
                else if (result.Reason == ResultReason.NoMatch)
                {
                    Console.WriteLine($"NOMATCH: Speech could not be recognized.");
                }
                else if (result.Reason == ResultReason.Canceled)
                {
                    var cancellation = CancellationDetails.FromResult(result);
                    Console.WriteLine($"CANCELED: Reason={cancellation.Reason}");

                    if (cancellation.Reason == CancellationReason.Error)
                    {
                        Console.WriteLine($"CANCELED: ErrorCode={cancellation.ErrorCode}");
                        Console.WriteLine($"CANCELED: ErrorDetails={cancellation.ErrorDetails}");
                        Console.WriteLine($"CANCELED: Did you update the subscription info?");
                    }
                }
            }
        }
    }

    public class Word
    {
        public string WordText { get; set; }
        public string ErrorType { get; set; }
        public double AccuracyScore { get; set; }
        public double Duration { get; set; }

        public Word(string wordText, string errorType)
        {
            WordText = wordText;
            ErrorType = errorType;
            AccuracyScore = 0;
            Duration = 0;
        }

        public Word(string wordText, string errorType, double accuracyScore) : this(wordText, errorType)
        {
            AccuracyScore = accuracyScore;
        }

        public Word(string wordText, string errorType, double accuracyScore, double duration) : this(wordText, errorType, accuracyScore)
        {
            Duration = duration;
        }
    }

}
