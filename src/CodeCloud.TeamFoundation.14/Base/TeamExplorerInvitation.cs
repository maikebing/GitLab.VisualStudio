using CodeCloud.VisualStudio.Shared;
using Microsoft.TeamFoundation.Controls;
using System;

namespace CodeCloud.TeamFoundation.Base
{
    public abstract class TeamExplorerInvitation : NotificationAwareObject, ITeamExplorerServiceInvitation
    {
        private bool _canConnect;
        public bool CanConnect
        {
            get { return _canConnect; }
            set { _canConnect = value; this.RaisePropertyChange(); }
        }

        private bool _canSignUp;
        public bool CanSignUp
        {
            get { return _canSignUp; }
            set { _canSignUp = value; this.RaisePropertyChange(); }
        }

        private string _connectLabel;
        public string ConnectLabel
        {
            get { return _connectLabel; }
            set { _connectLabel = value; this.RaisePropertyChange(); }
        }

        private string _description;
        public string Description
        {
            get { return _description; }
            set { _description = value; this.RaisePropertyChange(); }
        }

        private object _icon;
        public object Icon
        {
            get { return _icon; }
            set { _icon = value; this.RaisePropertyChange(); }
        }

        private bool _isVisible;
        public bool IsVisible
        {
            get { return _isVisible; }
            set { _isVisible = value; this.RaisePropertyChange(); }
        }

        private string _name;
        public string Name
        {
            get { return _name; }
            set { _name = value; this.RaisePropertyChange(); }
        }

        private string _provider;
        public string Provider
        {
            get { return _provider; }
            set { _provider = value; this.RaisePropertyChange(); }
        }

        private string _signUpLabel;
        public string SignUpLabel
        {
            get { return _signUpLabel; }
            set { _signUpLabel = value; this.RaisePropertyChange(); }
        }

        public abstract void Connect();


        public virtual void Initialize(IServiceProvider serviceProvider)
        {
        }

        public abstract void SignUp();

        #region IDisposable Support
        private bool _disposed = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                _disposed = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~TeamExplorerInvitation() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
