using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogViewer.Models
{
    public interface IParentLog
    {
        bool KeepCurrent { get; }
        string Source1 { get; }
        string Source2 { get; }
    }
}
