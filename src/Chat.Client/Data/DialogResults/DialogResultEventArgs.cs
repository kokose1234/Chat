using System;

namespace Chat.Client.Data.DialogResults;

public class DialogResultEventArgs<T> : EventArgs
{
    public T Result { get; }

    public DialogResultEventArgs(T result)
    {
        Result = result;
    }
}