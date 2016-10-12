using Microsoft.TeamFoundation.Controls;
using Microsoft.VisualStudio.PlatformUI;
using System;
using System.Drawing;
using CodeCloud.VisualStudio.Shared;

namespace CodeCloud.TeamFoundation.Base
{
    public abstract class TeamExplorerNavigationItem : NotificationAwareObject, ITeamExplorerNavigationItem2
    {
        private int _argbColor;
        public int ArgbColor
        {
            get { return _argbColor; }
            set { _argbColor = value; this.RaisePropertyChange(); }
        }

        private object _icon;
        public object Icon
        {
            get { return _icon; }
            set { _icon = value; this.RaisePropertyChange(); }
        }

        private bool _isEnabled;
        public bool IsEnabled
        {
            get { return _isEnabled; }
            set { _isEnabled = value; this.RaisePropertyChange(); }
        }

        private string _text;
        public string Text
        {
            get { return _text; }
            protected set { _text = value; this.RaisePropertyChange(); }
        }

        private Image _image;
        public Image Image
        {
            get { return _image; }
            protected set { _image = value; this.RaisePropertyChange(); }
        }

        private bool _isVisible;
        public bool IsVisible
        {
            get { return _isVisible; }
            protected set { _isVisible = value; this.RaisePropertyChange(); }
        }

        public abstract void Execute();

        public virtual void Invalidate()
        {
        }

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
        // ~TeamExplorerNavigationItem() {
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
