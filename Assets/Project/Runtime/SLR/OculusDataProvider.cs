using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public class OculusDataProvider : MonoBehaviour, IDataProvider
{
    private ActiveHandProvider _activeHandProvider;
    
    private MemoryStream _stream;
    private BinaryFormatter _formatter;
    
    private OVRPlugin.HandState _hs;

    private void Awake()
    {
        _activeHandProvider = GetComponent<ActiveHandProvider>();
        
        _stream = new MemoryStream();
        // _stream.Position = 0;
        _formatter = new BinaryFormatter();
        
        _hs = new OVRPlugin.HandState();
    }

    private void Update()
    {
        var activeHand = _activeHandProvider.GetActiveHand();
        if (activeHand == OVRPlugin.Hand.None) return;
        
        OVRPlugin.GetHandState(OVRPlugin.Step.Render, activeHand, ref _hs);
        
        _formatter.Serialize(_stream, _hs);
    }

    public ref readonly MemoryStream GetDataStream()
    {
        return ref _stream;
    }
}
