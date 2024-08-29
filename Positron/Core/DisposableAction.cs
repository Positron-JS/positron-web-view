using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Positron
{
    public class DisposableAction : IDisposable
    {
        private readonly Action action;

        public DisposableAction(Action action)
        {
            this.action = action;
        }

        public void Dispose()
        {
            action();
        }
    }
}
