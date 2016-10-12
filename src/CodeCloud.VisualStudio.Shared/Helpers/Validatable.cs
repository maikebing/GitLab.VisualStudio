namespace CodeCloud.VisualStudio.Shared.Helpers
{
    using System;
    using System.Collections;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;

    /// <summary>
    ///     From: http://anthymecaillard.wordpress.com/2012/03/26/wpf-4-5-validation-asynchrone/
    /// </summary>
    public abstract class Validatable : Bindable, INotifyDataErrorInfo
    {
        private readonly ConcurrentDictionary<string, List<string>> _errors = new ConcurrentDictionary<string, List<string>>();

        private readonly object _lock = new object();

        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

        public bool HasErrors
        {
            get { return _errors.Any(kv => kv.Value != null && kv.Value.Count > 0); }
        }

        public void OnErrorsChanged(string propertyName)
        {
            ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
        }

        public void Validate()
        {
            lock (_lock)
            {
                var validationContext = new ValidationContext(this, null, null);
                var validationResults = new List<ValidationResult>();
                Validator.TryValidateObject(this, validationContext, validationResults, true);

                ManualValidate(validationResults);

                foreach (var kv in _errors.ToList())
                {
                    if (validationResults.All(r => r.MemberNames.All(m => m != kv.Key)))
                    {
                        List<string> outLi;
                        _errors.TryRemove(kv.Key, out outLi);
                        OnErrorsChanged(kv.Key);
                    }
                }

                HandleValidationResults(validationResults);
            }
        }

        public Task ValidateAsync()
        {
            return Task.Run(() => Validate());
        }

        public IEnumerable GetErrors(string propertyName)
        {
            List<string> errorsForName;
            _errors.TryGetValue(propertyName, out errorsForName);
            return errorsForName;
        }

        /// <summary>
        /// Checks if a property already matches a desired value. Sets the property and
        /// notifies listeners only when necessary.
        /// </summary>
        /// <typeparam name="T">Type of the property.</typeparam>
        /// <param name="storage">Reference to a property with both getter and setter.</param>
        /// <param name="value">Desired value for the property.</param>
        /// <param name="callback">Action to be executed after value changed.</param>
        /// <param name="propertyName">Name of the property used to notify listeners. This
        /// value is optional and can be provided automatically when invoked from compilers that
        /// support CallerMemberName.</param>
        /// <returns>True if the value was changed, false if the existing value matched the
        /// desired value.</returns>
        protected override bool SetProperty<T>(ref T storage, T value, Action callback = null, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(storage, value))
            {
                return false;
            }

            storage = value;

            OnPropertyChanged(propertyName);
            ValidateProperty(value, propertyName);

            callback?.Invoke();

            return true;
        }

        protected void ValidateProperty(object value, [CallerMemberName] string propertyName = null)
        {
            lock (_lock)
            {
                var validationContext = new ValidationContext(this, null, null);
                validationContext.MemberName = propertyName;

                var validationResults = new List<ValidationResult>();
                Validator.TryValidateProperty(value, validationContext, validationResults);

                ManualValidateProperty(validationResults, propertyName);

                // clear previous errors from tested property
                if (_errors.ContainsKey(propertyName))
                {
                    List<string> outLi;
                    _errors.TryRemove(propertyName, out outLi);
                }

                OnErrorsChanged(propertyName);

                HandleValidationResults(validationResults);
            }
        }

        protected virtual void ManualValidateProperty(IList<ValidationResult> validationResults, string propertyName)
        {
        }

        protected virtual void ManualValidate(IList<ValidationResult> validationResults)
        {
        }

        private void HandleValidationResults(List<ValidationResult> validationResults)
        {
            var q = from r in validationResults
                    from m in r.MemberNames
                    group r by m
                        into g
                        select g;

            foreach (var prop in q)
            {
                var messages = prop.Select(r => r.ErrorMessage).ToList();

                if (_errors.ContainsKey(prop.Key))
                {
                    List<string> outLi;
                    _errors.TryRemove(prop.Key, out outLi);
                }

                _errors.TryAdd(prop.Key, messages);
                OnErrorsChanged(prop.Key);
            }
        }
    }
}
