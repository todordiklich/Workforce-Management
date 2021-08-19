using System;

namespace WFM.Common.CustomExceptions
{
    public class CustomApplicationException : Exception
    {
        public CustomApplicationException(string message) : base(message)
        {

        }
    }
}
