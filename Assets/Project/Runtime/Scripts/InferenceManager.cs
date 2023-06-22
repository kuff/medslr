using System;
using System.Linq;
using JetBrains.Annotations;
using Unity.Barracuda;
using UnityEngine;
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMethodReturnValue.Global

[RequireComponent(typeof(TextManager))]
public class InferenceManager : MonoBehaviour
{
    public NNModel model;
    
    private static readonly string Vocab = VocabularyProvider.GetVocab();
    private const int N = 2;
    
    private float[] _result;
    private static string _inputString = "";

    private TextManager _tm;
    private IWorker _worker;

    private void Start()
    {
        Initialize();
    }

    public void Initialize()
    {
        // Get the TextManager component attached to the current GameObject
        _tm = GetComponent<TextManager>();

        // Load the ngram model
        var runtimeModel = ModelLoader.Load(model);

        // Create a worker (inference engine) to execute the model on the CPU and enable verbose logging
        _worker = runtimeModel.CreateWorker(WorkerFactory.Device.CPU, verbose: true);
    }

    public ref float[] CalculateInferenceResult()
    {
#if UNITY_EDITOR
        Debug.Log("Calculating inference result...");
#endif
        // Get input
        var sentence = _tm.GetUserSentence();

        // Run model
        var input = GetInputTensor(sentence);
        _worker.Execute(input);
    
        // Retrieve result
        var output = _worker.CopyOutput();  // TODO: Turn into coroutine
        input.Dispose();

        // Analyze result
        var result = GetSoftmax(in output);
        output.Dispose();
    
        // Cache result
        _result = result;
        // _result = output.ToReadOnlyArray();
        
        return ref GetInferenceResult();
    }

    public void CalculateInferenceResultVoid()
    {
        // For use in Visual Scripting
        CalculateInferenceResult();
    }

    [CanBeNull]
    public ref float[] GetInferenceResult()
    {
        return ref _result;
    }

    private static Tensor GetInputTensor(string sentence)
    {
        // Filter out characters not compatible with the model
        var sentenceData = sentence
            .Where(c => Vocab.IndexOf(char.ToLower(c)) != -1)
            .Select(c => Vocab.IndexOf(char.ToLower(c)))
            .ToArray();
        
        // Extract as many elements as possible, up to N
        var startIdx = sentenceData.Length - N < 0 ? sentenceData.Length : N;
        var sentenceDataList = sentenceData[^startIdx..].ToList();

        // One-hot-encode character data into the input tensor
        var tensor = new Tensor(N, Vocab.Length);
        _inputString = "";
        var offset = N - sentenceDataList.Count;
        for (var i = 0; i < sentenceDataList.Count; i++)
        {
#if UNITY_EDITOR
            Debug.Log($"i: {i}, sentenceData[i]: {sentenceDataList[i]}, Vocab[sentenceData[i]]: {Vocab[sentenceDataList[i]]}");
#endif
            _inputString += Vocab[sentenceDataList[i]];
            tensor[i + offset, sentenceDataList[i]] = 1f;
        }

        // Transpose the tensor to fit with the model
        tensor = tensor.Reshape(new TensorShape(1, N, Vocab.Length, 1));
#if UNITY_EDITOR
        Debug.Log($"tensor.shape: {tensor}");
        Debug.Log($"N: {tensor.height}, V: {tensor.width}");
        for (var w = 0; w < tensor.height; w++)
        {
            var channelString = "[";
            for (var c = 0; c < tensor.width; c++)
            {
                var value = tensor[0, 0, w, c];
                channelString += $" {value},";
            }

            var finalString = $"{channelString[..^1]} ]";
            Debug.Log(finalString);
        }
#endif
        return tensor;
    }

    public static ref string GetInputString()
    {
        // Retrieves the string used as input for the model
        return ref _inputString;
    }
    
    private float[] GetSoftmax(in Tensor input)
    {
        // Convert the input tensor to a read-only array
        var x = input.ToReadOnlyArray();

        // Find the maximum value in the input array
        var max = x.Max();

        // Initialize the sum of exponentiated values and the softmax array
        double expXSum = 0;
        var softmax = new float[x.Length];
        var targetIndex = VocabularyProvider.GetVocabJustLetters().IndexOf(_tm.GetTargetCharacter(), StringComparison.Ordinal);

        // Iterate through the input array, exponentiate the values, subtract the max value, and sum the results
        for (var i = 0; i < x.Length; i++)
        {
            if (i == targetIndex) x[i] += 1;
            
            softmax[i] = (float)Math.Exp(x[i] - max);  // Subtracting max value to avoid numerical instability
            expXSum += softmax[i];
        }

        // Normalize the softmax array by dividing each value by the sum of exponentiated values
        for (var i = 0; i < softmax.Length; i++) softmax[i] /= (float)expXSum;

#if UNITY_EDITOR
        // Log the sum of the softmax array values (should be close to 1) when running in the Unity editor
        Debug.Log($"softmax.Sum(): {softmax.Sum()}");
#endif

        // Return the resulting softmax array
        return softmax;
    }
    
    private void OnDisable()
    {
        _worker.Dispose();
    }
}
