using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace CodeCloud.VisualStudio.Shared
{
    public static class PropertyNotifierExtensions
    {
        public static void RaisePropertyChange<TSender>(this TSender This, [CallerMemberName] string propertyName = null)
           where TSender : INotifyPropertySource
        {
            This.RaisePropertyChanged(propertyName);
        }
    }

    public interface INotifyPropertySource
    {
        void RaisePropertyChanged(string propertyName);
    }

    public abstract class NotificationAwareObject : INotifyPropertyChanged, INotifyPropertySource
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public void RaisePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
