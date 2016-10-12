using CodeCloud.VisualStudio.Shared;
using Microsoft.TeamFoundation.Controls;
using System;

namespace CodeCloud.TeamFoundation.Base
{

    public class TeamExplorerSection : NotificationAwareObject, ITeamExplorerSection
    {
        private bool _isBusy;
        public bool IsBusy
        {
            get { return _isBusy; }
            set { _isBusy = value; this.RaisePropertyChange(); }
        }

        private bool _isExpanded;
        public bool IsExpanded
        {
            get { return _isExpanded; }
            set { _isExpanded = value; this.RaisePropertyChange(); }
        }

        private bool _isVisible;
        public bool IsVisible
        {
            get { return _isVisible; }
            set { _isVisible = value; this.RaisePropertyChange(); }
        }

        private object _sectionContent;
        public object SectionContent
        {
            get { return _sectionContent; }
            set { _sectionContent = value; this.RaisePropertyChange(); }
        }

        private string _title;
        public string Title
        {
            get { return _title; }
            set { _title = value; this.RaisePropertyChange(); }
        }

        public virtual void Cancel()
        {
        }

        public virtual object GetExtensibilityService(Type serviceType)
        {
            return null;
        }

        public virtual void Initialize(object sender, SectionInitializeEventArgs e)
        {
        }

        public virtual void Loaded(object sender, SectionLoadedEventArgs e)
        {
        }

        public virtual void Refresh()
        {
        }

        public virtual void SaveContext(object sender, SectionSaveContextEventArgs e)
        {
        }

        public virtual void Dispose()
        {
        }
    }
}
