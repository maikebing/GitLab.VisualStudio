// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

namespace GitLab.VisualStudio.Shared.Helpers.Commands
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading.Tasks;
    using System.Windows.Input;

    /// <summary>
    ///     An <see cref="ICommand" /> whose delegates can be attached for <see cref="Execute" /> and <see cref="CanExecute" />.
    /// </summary>
    /// <typeparam name="T">Parameter type.</typeparam>
    /// <remarks>
    ///     The constructor deliberately prevents the use of value types.
    ///     Because ICommand takes an object, having a value type for T would cause unexpected behavior when CanExecute(null)
    ///     is called during XAML initialization for command bindings.
    ///     Using default(T) was considered and rejected as a solution because the implementor would not be able to distinguish
    ///     between a valid and defaulted values.
    ///     <para />
    ///     Instead, callers should support a value type by using a nullable value type and checking the HasValue property
    ///     before using the Value property.
    ///     <example>
    ///         <code>
    /// public MyClass()
    /// {
    ///     this.submitCommand = new DelegateCommand&lt;int?&gt;(this.Submit, this.CanSubmit);
    /// }
    ///
    /// private bool CanSubmit(int? customerId)
    /// {
    ///     return (customerId.HasValue &amp;&amp; customers.Contains(customerId.Value));
    /// }
    ///     </code>
    ///     </example>
    /// </remarks>
    public class DelegateCommand<T> : DelegateCommand
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DelegateCommand{T}"/> class.
        /// </summary>
        /// <param name="executeMethod">
        ///     Delegate to execute when Execute is called on the command. This can be null to just hook up
        ///     a CanExecute delegate.
        /// </param>
        /// <remarks><see cref="CanExecute" /> will always return true.</remarks>
        public DelegateCommand(Action<T> executeMethod)
                : this(executeMethod, (o) => true)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DelegateCommand{T}"/> class.
        /// </summary>
        /// <param name="executeMethod">
        /// Delegate to execute when Execute is called on the command. This can be null to just hook up
        /// a CanExecute delegate.
        /// </param>
        /// <param name="canExecuteMethod">Delegate to execute when CanExecute is called on the command. This can be null.</param>
        /// <exception cref="ArgumentNullException">
        /// When both <paramref name="executeMethod" /> and
        /// <paramref name="canExecuteMethod" /> ar <see langword="null" />.
        /// </exception>
        public DelegateCommand(Action<T> executeMethod, Func<T, bool> canExecuteMethod)
                : base((o) => executeMethod((T)o), (o) => canExecuteMethod((T)o))
        {
            if (executeMethod == null || canExecuteMethod == null)
            {
                throw new ArgumentNullException(
                    "canExecuteMethod", @"Neither the executeMethod nor the canExecuteMethod delegates can be null.");
            }

            var genericTypeInfo = typeof(T);

            // DelegateCommand allows object or Nullable<>.
            // note: Nullable<> is a struct so we cannot use a class constraint.
            if (genericTypeInfo.IsValueType)
            {
                if ((!genericTypeInfo.IsGenericType) ||
                    (!typeof(Nullable<>).IsAssignableFrom(genericTypeInfo.GetGenericTypeDefinition())))
                {
                    throw new InvalidCastException("T for DelegateCommand&lt;T&gt; is not an object nor Nullable.");
                }
            }
        }
    }

    /// <summary>
    /// An <see cref="ICommand" /> whose delegates do not take any parameters for <see cref="Execute" /> and
    /// <see cref="CanExecute" />.
    /// </summary>
    /// <see cref="DelegateCommand" />
    /// <see cref="DelegateCommand{T}" />
    public class DelegateCommand : ICommand
    {
        private readonly Predicate<dynamic> _canExecuteMethod;

        private readonly Action<dynamic> _executeMethod;

        private List<WeakReference> _canExecuteChangedHandlers;

        /// <summary>
        /// Initializes a new instance of the <see cref="DelegateCommand" /> class with the <see cref="Action" /> to invoke on execution.
        /// </summary>
        /// <param name="executeMethod">The <see cref="Action" /> to invoke when <see cref="ICommand.Execute" /> is called.</param>
        public DelegateCommand(Action executeMethod)
                : this(executeMethod, () => true)
        {
        }

        public DelegateCommand(Action<dynamic> executeMethod)
                : this(executeMethod, (o) => true)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DelegateCommand" /> class with the <see cref="Action" /> to invoke on execution
        /// and a <see langword="Func" /> to query for determining if the command can execute.
        /// </summary>
        /// <param name="executeMethod">The <see cref="Action" /> to invoke when <see cref="ICommand.Execute" /> is called.</param>
        /// <param name="canExecuteMethod">
        ///     The <see cref="Func{TResult}" /> to invoke when <see cref="ICommand.CanExecute" /> is
        ///     called
        /// </param>
        public DelegateCommand(Action executeMethod, Func<bool> canExecuteMethod)
                : this((o) => executeMethod(), (o) => canExecuteMethod())
        {
            if (executeMethod == null || canExecuteMethod == null)
            {
                throw new ArgumentNullException("executeMethod",
                                                @"Neither the executeMethod nor the canExecuteMethod delegates can be null.");
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DelegateCommand" /> class, specifying both the execute action and the can
        /// execute function.
        /// </summary>
        /// <param name="executeMethod">The <see cref="Action" /> to execute when <see cref="ICommand.Execute" /> is invoked.</param>
        /// <param name="canExecuteMethod">
        ///     The <see cref="Func{Object,Bool}" /> to invoked when <see cref="ICommand.CanExecute" />
        ///     is invoked.
        /// </param>
        public DelegateCommand(Action<dynamic> executeMethod, Predicate<dynamic> canExecuteMethod)
        {
            if (executeMethod == null || canExecuteMethod == null)
            {
                throw new ArgumentNullException("executeMethod",
                                                @"Neither the executeMethod nor the canExecuteMethod delegates can be null.");
            }

            _executeMethod = executeMethod;
            _canExecuteMethod = canExecuteMethod;
        }

        private DelegateCommand(Func<Task> executeMethod)
                : this(executeMethod, () => true)
        {
        }

        private DelegateCommand(Func<Task> executeMethod, Func<bool> canExecuteMethod)
                : this((o) => executeMethod(), (o) => canExecuteMethod())
        {
            if (executeMethod == null || canExecuteMethod == null)
            {
                throw new ArgumentNullException("executeMethod",
                                                @"Neither the executeMethod nor the canExecuteMethod delegates can be null.");
            }
        }

        /// <summary>
        ///     Occurs when changes occur that affect whether or not the command should execute. You must keep a hard
        ///     reference to the handler to avoid garbage collection and unexpected results. See remarks for more information.
        /// </summary>
        /// <remarks>
        ///     When subscribing to the <see cref="ICommand.CanExecuteChanged" /> event using
        ///     code (not when binding using XAML) will need to keep a hard reference to the event handler. This is to prevent
        ///     garbage collection of the event handler because the command implements the Weak Event pattern so it does not have
        ///     a hard reference to this handler. An example implementation can be seen in the CompositeCommand and
        ///     CommandBehaviorBase
        ///     classes. In most scenarios, there is no reason to sign up to the CanExecuteChanged event directly, but if you do,
        ///     you
        ///     are responsible for maintaining the reference.
        /// </remarks>
        /// <example>
        ///     The following code holds a reference to the event handler. The myEventHandlerReference value should be stored
        ///     in an instance member to avoid it from being garbage collected.
        ///     <code>
        /// EventHandler myEventHandlerReference = new EventHandler(this.OnCanExecuteChanged);
        /// command.CanExecuteChanged += myEventHandlerReference;
        /// </code>
        /// </example>
        public virtual event EventHandler CanExecuteChanged
        {
            add { WeakEventHandlerManager.AddWeakReferenceHandler(ref _canExecuteChangedHandlers, value, 2); }
            remove { WeakEventHandlerManager.RemoveWeakReferenceHandler(_canExecuteChangedHandlers, value); }
        }

        /// <summary>
        ///     Factory method to create a new instance of <see cref="DelegateCommand" /> from an awaitable handler method.
        /// </summary>
        /// <param name="executeMethod">Delegate to execute when Execute is called on the command.</param>
        /// <returns>Constructed instance of <see cref="DelegateCommand" /></returns>
        public static DelegateCommand FromAsyncHandler(Func<Task> executeMethod)
        {
            return new DelegateCommand(executeMethod);
        }

        /// <summary>
        ///     Factory method to create a new instance of <see cref="DelegateCommand" /> from an awaitable handler method.
        /// </summary>
        /// <param name="executeMethod">
        ///     Delegate to execute when Execute is called on the command. This can be null to just hook up
        ///     a CanExecute delegate.
        /// </param>
        /// <param name="canExecuteMethod">Delegate to execute when CanExecute is called on the command. This can be null.</param>
        /// <returns>Constructed instance of <see cref="DelegateCommand" /></returns>
        public static DelegateCommand FromAsyncHandler(Func<Task> executeMethod, Func<bool> canExecuteMethod)
        {
            return new DelegateCommand(executeMethod, canExecuteMethod);
        }

        void ICommand.Execute(object parameter)
        {
            Execute(parameter);
        }

        bool ICommand.CanExecute(object parameter)
        {
            return CanExecute(parameter);
        }

        /// <summary>
        ///     Raises <see cref="DelegateCommand.CanExecuteChanged" /> on the UI thread so every command invoker
        ///     can requery to check if the command can execute.
        ///     <remarks>
        ///         Note that this will trigger the execution of <see cref="DelegateCommand.CanExecute" /> once for
        ///         each invoker.
        ///     </remarks>
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1030:UseEventsWhereAppropriate", Justification = "To invalidate CanExecute")]
        public void InvalidateCanExecute()
        {
            OnCanExecuteChanged();
        }

        /// <summary>
        ///     Raises <see cref="ICommand.CanExecuteChanged" /> on the UI thread so every
        ///     command invoker can requery <see cref="ICommand.CanExecute" />.
        /// </summary>
        protected virtual void OnCanExecuteChanged()
        {
            WeakEventHandlerManager.CallWeakReferenceHandlers(this, _canExecuteChangedHandlers);
        }

        /// <summary>
        ///     Determines if the command can execute with the provided parameter by invoking the <see cref="Func{Object,Bool}" />
        ///     supplied during construction.
        /// </summary>
        /// <param name="parameter">The parameter to use when determining if this command can execute.</param>
        /// <returns>Returns <see langword="true" /> if the command can execute.  <see langword="False" /> otherwise.</returns>
        private bool CanExecute(dynamic parameter)
        {
            return _canExecuteMethod == null || _canExecuteMethod(parameter);
        }

        /// <summary>
        ///     Executes the command with the provided parameter by invoking the <see cref="Action{Object}" /> supplied during
        ///     construction.
        /// </summary>
        /// <param name="parameter">parameter</param>
        private void Execute(object parameter)
        {
            _executeMethod(parameter);
        }
    }
}