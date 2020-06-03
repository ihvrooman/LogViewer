using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogViewer.Models
{
    /// <summary>
    /// An interface for providing interaction with a <see cref="Models.Notification"/>.
    /// </summary>
    public interface INotification
    {
        /// <summary>
        /// The <see cref="Models.Notification"/>.
        /// </summary>
        Notification Notification { get; }

        /// <summary>
        /// A method for clearing the <see cref="Notification"/>.
        /// </summary>
        void ClearNotification();
    }
}
