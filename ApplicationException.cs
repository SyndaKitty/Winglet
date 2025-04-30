public class ApplicationException : Exception
{
    public string UserMessage;
    public Exception? Inner;
    
    public ApplicationException(string userMessage, Exception? inner)
    {
        UserMessage = userMessage;
        Inner = inner;
    }
}