using System;

namespace AutoSats.Exceptions;

public class ScheduleRunFailedException : Exception
{
    public ScheduleRunFailedException(string message) : base(message)
    {
    }
}
