namespace Grif;

public class OutputEventArgs(string text)
{
    public string Text { get; } = text;
}

public class IO
{
    public void Input(string input)
    {
    }

    // Declare the delegate (if using non-generic pattern).
    public delegate void OutputEventHandler(object sender, OutputEventArgs e);

    // Declare the event.
    public event OutputEventHandler? OutputEvent;

    // Wrap the event in a protected virtual method
    // to enable derived classes to raise the event.
    protected virtual void RaiseOutputEvent(string text)
    {
        // Raise the event in a thread-safe manner using the ?. operator.
        OutputEvent?.Invoke(this, new OutputEventArgs(text));
    }
}
