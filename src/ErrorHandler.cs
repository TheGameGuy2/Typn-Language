
using System.ComponentModel.Design;

namespace Errors;


public enum ErrorType
{
    SyntaxError,
    TypeError
}


internal struct TypnError
{
    public readonly ErrorType type;
    public readonly int line;

    public readonly string message;

    public TypnError(ErrorType type, int line, string message)
    {
        this.type = type;
        this.line = line;
        this.message = message;
    }

    public void Throw()
    {
        Console.WriteLine($"[{type}] in line {line}: {message}");
    }
}

public static class ErrorHandler
{
    private static List<TypnError> errors = new();

    public static void ThrowAll()
    {
        foreach(TypnError err in errors)
        {
            err.Throw();
        }
        Console.ReadKey();
        Environment.Exit(-1);

    }

    public static bool HasErrors()
    {
        return errors.Count > 0;
    }

    public static void AddError(ErrorType type, int line, string message, bool critical = false)
    {
        errors.Add(new TypnError(type, line, message));
        
        if(critical)
        {
            Console.WriteLine("[Critical]");
            ThrowAll();
        }
    }

}