namespace MIS.Application.Common;

public abstract class HrException : Exception
{
    protected HrException(string message, IReadOnlyCollection<string>? errors = null)
        : base(message)
    {
        Errors = errors ?? [];
    }

    public IReadOnlyCollection<string> Errors { get; }
}

public sealed class HrValidationException : HrException
{
    public HrValidationException(string message, IReadOnlyCollection<string>? errors = null)
        : base(message, errors)
    {
    }
}

public sealed class HrNotFoundException : HrException
{
    public HrNotFoundException(string message)
        : base(message)
    {
    }
}

public sealed class HrConflictException : HrException
{
    public HrConflictException(string message)
        : base(message)
    {
    }
}

public sealed class HrForbiddenException : HrException
{
    public HrForbiddenException(string message)
        : base(message)
    {
    }
}
