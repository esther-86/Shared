using System;
using System.Collections.Generic;

namespace Common.Utilities
{
    public class Error
    {
        public static void Ignore(Action action, string desc)
        {
            Handle(action, desc, null);
        }

        public static void Add(Action action, string desc, List<Exception> exceptions, bool throwException = true)
        {
            Handle(action, desc, (ex) =>
            {
                exceptions.Add(ex);
                if (throwException)
                    throw ex;
            });
        }

        public static void Handle(Action action, string desc, Action<Exception> additionalActionsOnError)
        {
            try { action.Invoke(); }
            catch (Exception ex)
            {
                Console.WriteLine("Saw error when trying to {0}: {1}", desc, ex.Message);
                if (additionalActionsOnError != null)
                    additionalActionsOnError.Invoke(ex);
            }
        }
    }
}
