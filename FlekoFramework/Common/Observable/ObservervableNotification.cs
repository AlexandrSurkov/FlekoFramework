using System;

namespace FlekoSoft.Common.Observable
{
    public class ObservervableNotification
    {
        public ObservervableNotification(Object sender, int type, EventArgs eventArgs)
        {
            Type = type;
            Sender = sender;
            EventArgs = eventArgs;
        }

        public object Sender { get; }
        public int Type { get; }
        public EventArgs EventArgs { get; }
    }
}
