namespace MetroExplorer.core
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    public static class Singleton<T, TK>
        where T : class, IEnumerable<TK>
        where TK : class
    {
        static volatile T _instance;

        static object _lock = new object();

        static Singleton()
        { }

        public static T Instance
        {
            get
            {
                return _instance ?? ResetAndGetInstance();
            }
        }

        private static T ResetAndGetInstance()
        {
            lock (_lock)
            {
                if (_instance == null)
                {
                    GetInstance();
                }
            }

            return _instance;
        }

        private static void GetInstance()
        {
            ConstructorInfo constructor;

            try
            {
                constructor = typeof(T).GetTypeInfo().DeclaredConstructors.FirstOrDefault(info => info.IsPrivate);
            }
            catch (Exception excep) { throw new SingletonException(excep); }

            if (constructor == null || constructor.IsAssembly)
            {
                throw new SingletonException(string.Format("A private or protected constructor is missing for '{0}'.", typeof(T).Name));
            }

            _instance = (T)constructor.Invoke(null);
        }
    }

    public class SingletonException
       : Exception
    {

        /// <summary>

        /// Initializes a new instance.

        /// </summary>
        public SingletonException()
        {

        }

        /// <summary>

        /// Initializes a new instance with a specified error message.

        /// </summary>

        /// <param name="message">The message that describes the error.</param>
        public SingletonException(string message)
            : base(message)
        {




        }

        /// <summary>

        /// Initializes a new instance with a reference to the inner 

        /// exception that is the cause of this exception.

        /// </summary>

        /// <param name="innerException">

        /// The exception that is the cause of the current exception, 

        /// or a null reference if no inner exception is specified.

        /// </param>
        public SingletonException(Exception innerException)
            : base(null, innerException)
        {




        }

        /// <summary>

        /// Initializes a new instance with a specified error message and a 

        /// reference to the inner exception that is the cause of this exception.

        /// </summary>

        /// <param name="message">The message that describes the error.</param>

        /// <param name="innerException">

        /// The exception that is the cause of the current exception, 

        /// or a null reference if no inner exception is specified.

        /// </param>
        public SingletonException(string message, Exception innerException)
            : base(message, innerException)
        {




        }
    }
}
