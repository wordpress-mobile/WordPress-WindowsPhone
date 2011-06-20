using System;
using System.ComponentModel;
using System.Collections.Generic;

namespace WordPress.Model
{
    #region delegate method declarations
        
    /// <summary>
    /// Used to communicate the progress of an xml rpc.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    public delegate void ProgressChangedEventHandler(object sender, ProgressChangedEventArgs args);

    /// <summary>
    /// Used to communicate that an xml rpc has completed.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    public delegate void XMLRPCCompletedEventHandler<T>(object sender, XMLRPCCompletedEventArgs<T> args) where T : INotifyPropertyChanged;

    /// <summary>
    /// Used by the DataService to communicate that an error has occurred performing an operation.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    public delegate void ExceptionEventHandler(object sender, ExceptionEventArgs args);
    
    #endregion

    #region event arg subclasses

    /// <summary>
    /// Used to wrap a fault code returned by the WordPress system when a remote procedure
    /// call fails.
    /// </summary>
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


    //thrown when the client cannot parse the XML-RPC message or when the message doesn't contains a valid XML-RPC message response
    public class XmlRPCParserException : Exception
    {
         public XmlRPCParserException() : base() { }

         public XmlRPCParserException(string message) : base(message)
        {
          
        }

         public XmlRPCParserException(string message, Exception innerException)
            : base(message, innerException)
        {
          
        }     
    }


    /// <summary>
    /// Used to indicate that an xml rpc has completed.  If an exception has occurred during the call
    /// the Error property will reference the exception; if the Error property returns null that indicates
    /// the XML-RPC completed successfully.
    /// </summary>
    /// <typeparam name="T"></typeparam>
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

    /// <summary>
    /// Used to communicate an exception has occurred on a task that may have been executed
    /// on a separate thread.
    /// </summary>
    public class ExceptionEventArgs : EventArgs
    {
        public ExceptionEventArgs(Exception exception)
            : base()
        {
            Exception = exception;
        }

        public Exception Exception { get; private set; }
    }

    #endregion
}