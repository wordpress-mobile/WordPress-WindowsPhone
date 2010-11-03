using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;

namespace WordPress.Model
{
    /// <summary>
    /// Executes a batch update for modifying a list of Comment objects.
    /// </summary>
    public class EditCommentsStatusRPC
    {
        //DEV NOTE: we want to model this class after the XmlRemoteProcedureCall class, but it
        //really isn't a subclass--we want to execute a series of rpcs via the EditCommentRPC
        //class, listen for their success/failure, and report back to the user

        #region member variables

        private static object _syncRoot = new object();

        private List<Comment> _modifiedComments;
        private int _numberOfCompletedRPCs;
        private AsyncOperation _operation;
        private SendOrPostCallback _progressReportDelegate;
        private SendOrPostCallback _completedDelegate;

        private List<XMLRPCCompletedEventArgs<Comment>> _successes;
        private List<XMLRPCCompletedEventArgs<Comment>> _failures;

        #endregion

        #region events

        public event ProgressChangedEventHandler ProgressChanged;

        public event XMLRPCCompletedEventHandler<Comment> Completed;

        #endregion

        #region constructors

        public EditCommentsStatusRPC() 
        {
            _progressReportDelegate = new SendOrPostCallback(NotifyProgressChanged);
            _completedDelegate = new SendOrPostCallback(NotifyCompleted);
        }

        #endregion

        #region properties

        public eCommentStatus CommentStatus { get; set; }

        public IList<Comment> Comments { get; set; }

        #endregion

        #region methods

        public void ExecuteAsync()
        {
            ValidateValues();

            if (0 == Comments.Count)
            {
                List<Comment> items = new List<Comment>();
                XMLRPCCompletedEventArgs<Comment> args = new XMLRPCCompletedEventArgs<Comment>(items, null, false, null);
                NotifyCompleted(args);
                return;
            }

            _modifiedComments = Comments.Where(comment => CommentStatus != comment.CommentStatus).ToList();

            if (0 == _modifiedComments.Count)
            {
                List<Comment> items = new List<Comment>();
                XMLRPCCompletedEventArgs<Comment> args = new XMLRPCCompletedEventArgs<Comment>(items, null, false, null);
                NotifyCompleted(args);
                return;
            }

            //initialize our counter ivar
            _numberOfCompletedRPCs = 0;
            
            //initialize our success/failure collections
            if (null != _successes)
            {
                _successes.Clear();
                _successes = null;
            }
            _successes = new List<XMLRPCCompletedEventArgs<Comment>>();

            if (null != _failures)
            {
                _failures.Clear();
                _failures = null;
            }
            _failures = new List<XMLRPCCompletedEventArgs<Comment>>();

            _operation = AsyncOperationManager.CreateOperation(Guid.NewGuid());
            
            _modifiedComments.ForEach(comment =>
            {
                comment.CommentStatus = this.CommentStatus;

                EditCommentRPC rpc = new EditCommentRPC(DataService.Current.CurrentBlog, comment);
                rpc.Completed += OnEditCommentRPCCompleted;
                rpc.ExecuteAsync();
            });
        }

        private void ValidateValues()
        {
            if (null == Comments)
            {
                throw new ArgumentException("Comments may not be null", "Comments");
            }
        }

        private void NotifyProgressChanged(object state)
        {
            ProgressChangedEventArgs args = state as ProgressChangedEventArgs;
            if (null != ProgressChanged)
            {
                ProgressChanged(this, args);
            }
        }

        private void NotifyCompleted(object state)
        {
            XMLRPCCompletedEventArgs<Comment> args = state as XMLRPCCompletedEventArgs<Comment>;
            if (null != Completed)
            {
                Completed(this, args);
            }

            //modify any matches in the datastore so we can save a web call
            args.Items.ForEach(comment =>
            {
                Comment match = DataService.Current.CurrentBlog.Comments.Single(c => c.CommentId == comment.CommentId);
                if (null != match)
                {
                    match.CommentStatus = comment.CommentStatus;
                }
            });
        }

        private void OnEditCommentRPCCompleted(object sender, XMLRPCCompletedEventArgs<Comment> args)
        {
            lock (_syncRoot)
            {
                _numberOfCompletedRPCs++;
                
                if (null == args.Error)
                {
                    _successes.Add(args);
                }
                else
                {
                    _failures.Add(args);
                }

                if (_numberOfCompletedRPCs == _modifiedComments.Count)
                {
                    Exception error = null;
                    if (0 < _failures.Count)
                    {
                        error = _failures[0].Error;
                    }
                    List<Comment> modifiedComments = new List<Comment>();
                    _successes.ForEach(successArgs =>
                    {
                        modifiedComments.Add(successArgs.Items[0]);
                    });

                    XMLRPCCompletedEventArgs<Comment> completedArgs = new XMLRPCCompletedEventArgs<Comment>(_modifiedComments, error, false, _operation.UserSuppliedState);
                    _operation.PostOperationCompleted(_completedDelegate, completedArgs);
                }
                else
                {
                    int progress = _modifiedComments.Count / _numberOfCompletedRPCs;

                    ProgressChangedEventArgs progressArgs = new ProgressChangedEventArgs(progress, null);
                    _operation.Post(_progressReportDelegate, progressArgs);
                }
            }
        }

        #endregion
    }
}
