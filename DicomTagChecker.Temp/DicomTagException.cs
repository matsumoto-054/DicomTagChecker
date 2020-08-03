using System;

namespace DicomTagChecker.Temp
{
    public class InvalidTagException : Exception
    {
        public InvalidTagException(string message) : base(message)
        {
        }
    }

    public class InvalidPatientIdException : InvalidTagException
    {
        public InvalidPatientIdException(string message) : base(message)
        {
        }
    }
}