namespace MetroExplorer.Core.Behavior
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading.Tasks;
    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Data;
    using Windows.UI.Xaml.Media;

    /// http://winrtbehaviors.codeplex.com/SourceControl/changeset/view/19567#261274
    public abstract class Behavior : FrameworkElement
    {
        private FrameworkElement associatedObject;

        /// <summary>
        /// The associated object
        /// </summary>
        public FrameworkElement AssociatedObject
        {
            get
            {
                return associatedObject;
            }
            set
            {
                if (associatedObject != null)
                {
                    OnDetaching();
                }
                DataContext = null;

                associatedObject = value;
                if (associatedObject != null)
                {
                    // FIX LocalJoost 17-08-2012 moved ConfigureDataContext to OnLoaded
                    // to prevent the app hanging on a behavior attached to an element#
                    // that's not directly loaded (like a FlipViewItem)
                    OnAttached();
                }
            }
        }

        protected virtual void OnAttached()
        {
            AssociatedObject.Unloaded += AssociatedObjectUnloaded;
            AssociatedObject.Loaded += AssociatedObjectLoaded;
        }

        protected virtual void OnDetaching()
        {
            AssociatedObject.Unloaded -= AssociatedObjectUnloaded;
            AssociatedObject.Loaded -= AssociatedObjectLoaded;
        }

        private void AssociatedObjectLoaded(object sender, RoutedEventArgs e)
        {
            ConfigureDataContext();
        }

        private void AssociatedObjectUnloaded(object sender, RoutedEventArgs e)
        {
            OnDetaching();
        }

        /// <summary>
        /// Configures data context. 
        /// Courtesy of Filip Skakun
        /// http://twitter.com/xyzzer
        /// </summary>
        private async void ConfigureDataContext()
        {
            while (associatedObject != null)
            {
                if (AssociatedObjectIsInVisualTree)
                {
                    Debug.WriteLine(associatedObject.Name + " found in visual tree");
                    SetBinding(
                        DataContextProperty,
                        new Binding
                        {
                            Path = new PropertyPath("DataContext"),
                            Source = associatedObject
                        });

                    return;
                }
                Debug.WriteLine(associatedObject.Name + " Not in visual tree");
                await WaitForLayoutUpdateAsync();
            }
        }

        /// <summary>
        /// Checks if object is in visual tree
        /// Courtesy of Filip Skakun
        /// http://twitter.com/xyzzer
        /// </summary>
        private bool AssociatedObjectIsInVisualTree
        {
            get
            {
                if (associatedObject != null)
                {
                    return Window.Current.Content != null && Ancestors.Contains(Window.Current.Content);
                }
                return false;
            }
        }

        /// <summary>
        /// Finds the object's associatedobject's ancestors
        /// Courtesy of Filip Skakun
        /// http://twitter.com/xyzzer
        /// </summary>
        private IEnumerable<DependencyObject> Ancestors
        {
            get
            {
                if (associatedObject != null)
                {
                    var parent = VisualTreeHelper.GetParent(associatedObject);

                    while (parent != null)
                    {
                        yield return parent;
                        parent = VisualTreeHelper.GetParent(parent);
                    }
                }
            }
        }

        /// <summary>
        /// Creates a task that waits for a layout update to complete
        /// Courtesy of Filip Skakun
        /// http://twitter.com/xyzzer
        /// </summary>
        /// <returns></returns>
        private async Task WaitForLayoutUpdateAsync()
        {
            await EventAsync.FromEvent<object>(
                eh => associatedObject.LayoutUpdated += eh,
                eh => associatedObject.LayoutUpdated -= eh);
        }
    }

    /// <summary>
    /// Base class for behaviors
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class Behavior<T> : Behavior where T : FrameworkElement
    {
        protected Behavior()
        {
        }

        public T AssociatedObject
        {
            get
            {
                return (T)base.AssociatedObject;
            }
            set
            {
                base.AssociatedObject = value;
            }
        }
    }

    /// <summary>
    ///  Based on: http://social.msdn.microsoft.com/Forums/sk/async/thread/30f3339c-5e04-4aa8-9a09-9be72d9d9a1b
    /// Courtesy of Filip Skakun
    /// http://twitter.com/xyzzer
    /// </summary>
    public static class EventAsync
    {
        /// <summary>
        /// Creates a <see cref="System.Threading.Tasks.Task"/>
        /// that waits for an event to occur.
        /// </summary>
        /// <example>
        /// <![CDATA[
        /// await EventAsync.FromEvent(
        ///     eh => storyboard.Completed += eh,
        ///     eh => storyboard.Completed -= eh,
        ///     storyboard.Begin);
        /// ]]>
        /// </example>
        /// <param name="addEventHandler">
        /// The action that subscribes to the event.
        /// </param>
        /// <param name="removeEventHandler">
        /// The action that unsubscribes from the event when it first occurs.
        /// </param>
        /// <param name="beginAction">
        /// The action to call after subscribing to the event.
        /// </param>
        /// <returns>
        /// The <see cref="System.Threading.Tasks.Task"/> that
        /// completes when the event registered in
        /// <paramref name="addEventHandler"/> occurs.
        /// </returns>
        public static Task<object> FromEvent<T>(
            Action<EventHandler<T>> addEventHandler,
            Action<EventHandler<T>> removeEventHandler,
            Action beginAction = null)
        {
            return new EventHandlerTaskSource<T>(
                addEventHandler,
                removeEventHandler,
                beginAction).Task;
        }

        /// <summary>
        /// Creates a <see cref="System.Threading.Tasks.Task"/>
        /// that waits for an event to occur.
        /// </summary>
        /// <example>
        /// <![CDATA[
        /// await EventAsync.FromEvent(
        ///     eh => button.Click += eh,
        ///     eh => button.Click -= eh);
        /// ]]>
        /// </example>
        /// <param name="addEventHandler">
        /// The action that subscribes to the event.
        /// </param>
        /// <param name="removeEventHandler">
        /// The action that unsubscribes from the event when it first occurs.
        /// </param>
        /// <param name="beginAction">
        /// The action to call after subscribing to the event.
        /// </param>
        /// <returns>
        /// The <see cref="System.Threading.Tasks.Task"/> that
        /// completes when the event registered in
        /// <paramref name="addEventHandler"/> occurs.
        /// </returns>
        public static Task<RoutedEventArgs> FromRoutedEvent(
            Action<RoutedEventHandler> addEventHandler,
            Action<RoutedEventHandler> removeEventHandler,
            Action beginAction = null)
        {
            return new RoutedEventHandlerTaskSource(
                addEventHandler,
                removeEventHandler,
                beginAction).Task;
        }

        private sealed class EventHandlerTaskSource<TEventArgs>
        {
            private readonly TaskCompletionSource<object> tcs;
            private readonly Action<EventHandler<TEventArgs>> removeEventHandler;

            public EventHandlerTaskSource(
                Action<EventHandler<TEventArgs>> addEventHandler,
                Action<EventHandler<TEventArgs>> removeEventHandler,
                Action beginAction = null)
            {
                if (addEventHandler == null)
                {
                    throw new ArgumentNullException("addEventHandler");
                }

                if (removeEventHandler == null)
                {
                    throw new ArgumentNullException("removeEventHandler");
                }

                this.tcs = new TaskCompletionSource<object>();
                this.removeEventHandler = removeEventHandler;
                addEventHandler.Invoke(EventCompleted);

                if (beginAction != null)
                {
                    beginAction.Invoke();
                }
            }

            /// <summary>
            /// Returns a task that waits for the event to occur.
            /// </summary>
            public Task<object> Task
            {
                get { return tcs.Task; }
            }

            private void EventCompleted(object sender, TEventArgs args)
            {
                this.removeEventHandler.Invoke(EventCompleted);
                this.tcs.SetResult(args);
            }
        }

        private sealed class RoutedEventHandlerTaskSource
        {
            private readonly TaskCompletionSource<RoutedEventArgs> tcs;
            private readonly Action<RoutedEventHandler> removeEventHandler;

            public RoutedEventHandlerTaskSource(
                Action<RoutedEventHandler> addEventHandler,
                Action<RoutedEventHandler> removeEventHandler,
                Action beginAction = null)
            {
                if (addEventHandler == null)
                {
                    throw new ArgumentNullException("addEventHandler");
                }

                if (removeEventHandler == null)
                {
                    throw new ArgumentNullException("removeEventHandler");
                }

                this.tcs = new TaskCompletionSource<RoutedEventArgs>();
                this.removeEventHandler = removeEventHandler;
                addEventHandler.Invoke(EventCompleted);

                if (beginAction != null)
                {
                    beginAction.Invoke();
                }
            }

            /// <summary>
            /// Returns a task that waits for the routed to occur.
            /// </summary>
            public Task<RoutedEventArgs> Task
            {
                get { return tcs.Task; }
            }

            private void EventCompleted(object sender, RoutedEventArgs args)
            {
                this.removeEventHandler.Invoke(EventCompleted);
                this.tcs.SetResult(args);
            }
        }
    }
}
