using System;
using Chat.Client.Data.DialogResults;

namespace Chat.Client.ViewModels;

public class DialogViewModelBase<T> : ViewModelBase where T : DialogResultBase
{
    public event EventHandler<DialogResultEventArgs<T>> CloseRequested;

    protected void Close() => Close(default);

    protected void Close(T result)
    {
        var args = new DialogResultEventArgs<T>(result);
        CloseRequested.Invoke(this, args);
    }
}

public class DialogViewModelBase : DialogViewModelBase<DialogResultBase>
{

}