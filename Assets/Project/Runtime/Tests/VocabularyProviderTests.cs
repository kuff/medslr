using NUnit.Framework;
// ReSharper disable StringLiteralTypo

public class VocabularyProviderTests
{
    [Test]
    public void GetVocab_ReturnsCorrectString()
    {
        const string expected = "abcdefghijklmnopqrstuvwxyzæøå ";
        var actual = VocabularyProvider.GetVocab();
        Assert.AreEqual(expected, actual);
    }

    [Test]
    public void GetVocabArray_ReturnsCorrectCharArray()
    {
        var expected = "abcdefghijklmnopqrstuvwxyzæøå ".ToCharArray();
        var actual = VocabularyProvider.GetVocabArray();
        Assert.AreEqual(expected, actual);
    }

    [Test]
    public void GetVocabJustLetters_ReturnsCorrectString()
    {
        const string expected = "abcdefghijklmnopqrstuvwxyzæøå";
        var actual = VocabularyProvider.GetVocabJustLetters();
        Assert.AreEqual(expected, actual);
    }

    [Test]
    public void GetVocabArrayJustLetters_ReturnsCorrectCharArray()
    {
        var expected = "abcdefghijklmnopqrstuvwxyzæøå".ToCharArray();
        var actual = VocabularyProvider.GetVocabArrayJustLetters();
        Assert.AreEqual(expected, actual);
    }
}