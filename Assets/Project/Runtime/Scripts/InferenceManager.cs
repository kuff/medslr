using System;
using System.Linq;
using JetBrains.Annotations;
using Unity.Barracuda;
using UnityEngine;

[RequireComponent(typeof(TextManager))]
public class InferenceManager : MonoBehaviour
{
    private static readonly string Vocab = VocabularyProvider.GetVocab();
    private const int N = 4;
    
    private float[] _result;
    
    public NNModel model;

    // private TextManager _tm;
    // private IWorker _worker;

    // private void OnEnable()
    // {
    //     var runtimeModel = ModelLoader.Load(model);
    //     _worker = WorkerFactory.CreateWorker(runtimeModel, WorkerFactory.Device.CPU);
    // }

    // private void Start()
    // {
    //     _tm = GetComponent<TextManager>();
    // }

    public ref float[] CalculateInferenceResult()
    {
        var runtimeModel = ModelLoader.Load(model);
        var worker = WorkerFactory.CreateWorker(runtimeModel, WorkerFactory.Device.CPU);
        
        var tm = GetComponent<TextManager>();

        var sentence = tm.GetUserSentence();
        if (sentence == "")
        {
            _result = GetUniformPredictionArray();
        }
        else
        {
            // Run model
            var input = GetInputTensor(sentence);
            worker.Execute(input);
        
            // Retrieve result
            var output = worker.CopyOutput();
            input.Dispose();
            worker.Dispose();

            // Analyze result
            var result = GetSoftmax(in output);  // TODO: make sure batch dimension is squeezed...
        
            // Cache result
            _result = result;
        }
        
        return ref GetInferenceResult();
    }

    [CanBeNull]
    public ref float[] GetInferenceResult()
    {
        return ref _result;
    }

    private static float[] GetUniformPredictionArray()
    {
        var result = new float[Vocab.Length];
        var value = 1f / result.Length;

        for (var i = 0; i < result.Length; i++)
            result[i] = value;

        return result;
    }

    private static Tensor GetInputTensor(string sentence)
    {
        // Filter out characters not compatible with the model
        var sentenceData = sentence
            .Where(c => Vocab.IndexOf(char.ToLower(c)) != -1)
            .Select(c => Vocab.IndexOf(char.ToLower(c)))
            .ToArray();
        
        // Extract as many elements as possible, up to N
        var startIdx = sentenceData.Length - N < 0 ? sentence.Length : N;
        var sentenceDataList = sentenceData[^startIdx..].ToList();

        // Pad the input in case there isn't enough elements to fill the kernel
        while (sentenceDataList.Count < N) sentenceDataList.Insert(0, Vocab.IndexOf(" ", StringComparison.Ordinal));
        
        // One-hot-encode character data into the input tensor
        var tensor = new Tensor(N, Vocab.Length);
        for (var i = 0; i < N; i++)
        {
#if UNITY_EDITOR
            Debug.Log($"i: {i}, sentenceData[i]: {sentenceDataList[i]}, Vocab[sentenceData[i]]: {Vocab[sentenceDataList[i]]}");
#endif
            tensor[i, sentenceDataList[i]] = 1f;
        }

        // Transpose the tensor to fit with the model
        tensor = tensor.Reshape(new TensorShape(1, 1, N, Vocab.Length));
#if UNITY_EDITOR
        Debug.Log($"tensor.shape: {tensor.shape}");
#endif

        return tensor;
    }
    
    private static float[] GetSoftmax(in Tensor input)
    {
        var x = input.ToReadOnlyArray();

        var max = x.Max();
        var expX = x.Select(v => (float)Math.Exp(v - max)).ToArray(); // Subtract the max to avoid numerical instability
        var expXSum = expX.Sum();
        var softmax = expX.Select(v => v / expXSum).ToArray();
        
        return softmax;
    }

    // private void OnDisable()
    // {
    //     _worker.Dispose();
    // }
}
