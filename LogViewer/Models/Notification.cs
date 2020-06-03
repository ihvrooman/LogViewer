using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogViewer.Models
{
    /// <summary>
    /// A class containing notification information and methods.
    /// </summary>
    public class Notification
    {
        /// <summary>
        /// The notification's parent object.
        /// </summary>
        private INotification _parent { get; set; }
        /// <summary>
        /// The notification message.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Creates a new <see cref="Notification"/>.
        /// </summary>
        /// <param name="parent">The notification's parent object.</param>
        /// <param name="message">The notification message.</param>
        public Notification(INotification parent, string message)
        {
            _parent = parent;
            Message = message;
        }

        /// <summary>
        /// Clear's the notification.
        /// </summary>
        public void Clear()
        {
            _parent.ClearNotification();
        }
    }
}
