using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public class GestureInferenceProcess : MonoBehaviour, IInferenceProcess
{
    private OculusDataProvider _dataProvider;
    private MemoryStream _dataStream;
    private BinaryFormatter _formatter;
    
    private float _counter;

    private void Start()
    {
        _dataProvider = GetComponent<OculusDataProvider>();
        _dataStream = _dataProvider.GetDataStream();
        _formatter = new BinaryFormatter();
    }

    private void Update()
    {
        _counter += Time.deltaTime;
        if (_counter < 1) return;
        _counter = 0;

        while (_dataStream.Position < _dataStream.Length)
        {
            var obj = _formatter.Deserialize(_dataStream);
            Debug.Log("WEWEWE:\n\n" + obj);
        }
    }

    public void Run()
    {
        throw new System.NotImplementedException();
    }

    public void Halt()
    {
        throw new System.NotImplementedException();
    }

    public float[] PeekResult()
    {
        throw new System.NotImplementedException();
    }

    public float[] GetFinalResult()
    {
        throw new System.NotImplementedException();
    }
}
