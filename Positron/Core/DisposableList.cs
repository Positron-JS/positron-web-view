using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeuroSpeech.Positron.Core;

public class DisposableList: IDisposable
{
    private List<IDisposable>? disposables;

    public DisposableList()
    {
        
    }

    public void Register(IDisposable disposable)
    {
        (disposables ??= new List<IDisposable>()).Add(disposable);
    }

    public void Dispose()
    {
        if (disposables == null)
            return;
        foreach (var disposable in disposables)
        {
            disposable.Dispose();
        }
    }
}
