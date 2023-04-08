using System.Linq;

public static class VocabularyProvider
{
    private const string Vocab = "abcdefghijklmnopqrstuvwxyzæøå ";  // Letters of the Danish alphabet + space

    public static string GetVocab()
    {
        return Vocab;
    }

    public static char[] GetVocabArray()
    {
        return Vocab.ToArray();
    }

    public static string GetVocabJustLetters()
    {
        return new string(GetVocabArrayJustLetters());
    }

    public static char[] GetVocabArrayJustLetters()
    {
        return Vocab.Where(char.IsLetter).ToArray();
    }
}
