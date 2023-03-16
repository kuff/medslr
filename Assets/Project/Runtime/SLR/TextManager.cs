using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

public class TextManager : MonoBehaviour, IDataProvider
{
    public char[] referenceText;
    public char[] userText;
    public char   currentCharacter;
    
    private MemoryStream _stream;

    private void Start()
    {
        _stream = new MemoryStream();
    }

    public ref readonly MemoryStream GetDataStream()
    {
        return ref _stream;
    }

    private void ConvertAndWriteToStream(char input)
    {
        var span = new ReadOnlySpan<byte>(new byte[] { (byte)input });
    }
    
    private void ConvertAndWriteToStream()
    {
        var span = new ReadOnlySpan<byte>(userText.Select(c => (byte)c).ToArray());
    }

    public char GetNextCharacter()
    {
        return 'a';
    }

    public void Advance()
    {
        // ...
    }
}
