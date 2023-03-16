public interface IInferenceProcess
{
    public void Run();
    public void Halt();
    public float[] PeekResult();
    public float[] GetFinalResult();
}
