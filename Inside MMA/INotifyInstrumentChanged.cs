namespace Inside_MMA
{
    public delegate void InstrumentChanged(string newBoard, string newSeccode);
    public interface INotifyInstrumentChanged
    {
        event InstrumentChanged InstrumentChanged;
    }
}