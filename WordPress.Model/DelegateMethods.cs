using System;
using System.ComponentModel;
using System.Collections.Generic;

namespace WordPress.Model
{
    public class XmlRPCEventArgs<T>: EventArgs where T : INotifyPropertyChanged
    {
        public XmlRPCEventArgs()
        {
            Items = new List<T>();            
        }

        public List<T> Items { get; private set; }
    }

    public delegate void XmlRPCSucceededHandler<T>(object sender, XmlRPCEventArgs<T> args) where T:INotifyPropertyChanged;

    public class XmlRPCException : Exception
    {
        public XmlRPCException() : base() { }

        public XmlRPCException(int faultCode, string message)
            : base(message)
        {
            FaultCode = faultCode;
        }

        public XmlRPCException(int faultCode, string message, Exception innerException)
            : base(message, innerException)
        {
            FaultCode = faultCode;
        }

        public int FaultCode { get; private set; }        
    }

    public delegate void XmlRPCFailedHandler(object sender, Exception exception);



    /*-----------------------------------------------------
     * Delegate declarations...
     * --------------------------------------------------*/

    public delegate void ProgressChangedEventHandler(object sender, ProgressChangedEventArgs args);


    /*-----------------------------------------------------
     * Event arg declarations...
     * ---------------------------------------------------*/
    public class XMLRPCCompletedEventArgs<T> : AsyncCompletedEventArgs where T : INotifyPropertyChanged
    {
        public XMLRPCCompletedEventArgs(T item, Exception ex, bool canceled, object state)
            : base(ex, canceled, state)
        {
            Items = new List<T>();
            if (null != item)
            {
                Items.Add(item);
            }
        }

        public XMLRPCCompletedEventArgs(List<T> items, Exception ex, bool canceled, object state)
            : base(ex, canceled, state)
        {
            Items = new List<T>();
            if (null != items)
            {
                Items.AddRange(items);
            }
        }

        public List<T> Items { get; private set; }
    }

    public delegate void XMLRPCCompletedEventHandler<T>(object sender, XMLRPCCompletedEventArgs<T> args) where T : INotifyPropertyChanged;

    public class ExceptionEventArgs : EventArgs
    {
        public ExceptionEventArgs(Exception exception)
            : base()
        {
            Exception = exception;
        }

        public Exception Exception { get; private set; }
    }

    public delegate void ExceptionEventHandler(object sender, ExceptionEventArgs args);
}