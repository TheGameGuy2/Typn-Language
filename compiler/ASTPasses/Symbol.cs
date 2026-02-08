
using System.Net.Http.Headers;
using IR;
using Parsing;

namespace ASTPasses;

public record class Symbol(string Name, IRDataType DataType,Scope Scope);

public class Scope
{
    public Scope? Parent;
    public Dictionary<string, Symbol> Symbols = new();

    public Scope(Scope? parent = null)
    {
        Parent = parent;
    }

    public bool TryGetSymbol(string name, out Symbol? symb)
    {
        if(Symbols.TryGetValue(name,out Symbol? symbol))
        {
            symb = symbol;
            return true;
        }

        if (Parent!=null && Parent.TryGetSymbol(name, out symb))
        {
            return true;
        }


        symb = null;


        return false;
    }

    
    public void AddSymbol(string name, IRDataType dataType = IRDataType.None)
    {
        Symbols.Add(name, new Symbol(name, dataType, this));
    }

}