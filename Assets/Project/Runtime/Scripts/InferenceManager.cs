using System;
using System.Linq;
using Unity.Barracuda;
using UnityEngine;

[RequireComponent(typeof(TextManager))]
public class InferenceManager : MonoBehaviour
{
    private const string Vocab = "abcdefghijklmnopqrstuvwxyzæøå ";
    private const int N = 4;
    
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

    public float[] GetInferenceResult()
    {
        var runtimeModel = ModelLoader.Load(model);
        var worker = WorkerFactory.CreateWorker(runtimeModel, WorkerFactory.Device.CPU);
        
        var tm = GetComponent<TextManager>();

        var sentence = tm.GetUserSentence();
        if (sentence == "") return GetUniformPredictionArray();
        
        // Run model
        var input = GetInputTensor(sentence);
        worker.Execute(input);
        
        // Retrieve result
        var output = worker.CopyOutput();
        input.Dispose();
        worker.Dispose();

        // Analyze result
        var result = GetSoftmax(in output);  // TODO: make sure batch dimension is squeezed...
        return result;
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
        // Pad the sentence until it meets model kernel requirements
        while (sentence.Length < N) sentence = string.Concat(" ", sentence);
        
        // Define input Tensor and parse the sentence by vocab indices
        var tensor = new Tensor(N, Vocab.Length);
        var startIdx = sentence.Length - N < 0 ? sentence.Length : N;
        var sentenceData = sentence[^startIdx..]
            .Where(c => Vocab.IndexOf(char.ToLower(c)) != -1)
            .Select(c => Vocab.IndexOf(char.ToLower(c)))  // + 1
            .ToArray();

        // One-hot-encode sentence data into the input Tensor
        for (var i = 0; i < Mathf.Min(sentenceData.Length, startIdx); i++)
        {
#if UNITY_EDITOR
            Debug.Log($"i: {i}, sentenceData[i]: {sentenceData[i]}");
#endif
            tensor[i, sentenceData[i]] = 1f;
        }

        tensor = tensor.Reshape(new TensorShape(1, 1, N, Vocab.Length));
        Debug.Log(tensor.shape);

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
