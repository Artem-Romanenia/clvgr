namespace clvgr.Core.Exceptions;

internal class ClipboardException : Exception
{
    public ClipboardException(string message) : base(message) { }
    public ClipboardException(string message, Exception ex) : base(message, ex) { }
}
