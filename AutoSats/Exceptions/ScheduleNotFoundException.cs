namespace AutoSats.Exceptions;

public class ScheduleNotFoundException : Exception
{
    public ScheduleNotFoundException(int id) : base($"Schedule with id '{id}' not found")
    {
    }
}
