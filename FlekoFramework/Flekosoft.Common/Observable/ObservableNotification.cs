using System;

namespace Flekosoft.Common.Observable
{
    public class ObservableNotification
    {
        public ObservableNotification(Object sender, int type, EventArgs eventArgs)
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
