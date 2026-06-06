
using System.Net.Http.Headers;
using IR;
using Parsing;

namespace ASTPasses;

public record class Symbol(string Name, IRDataType DataType, Scope Scope);

public class Scope
{
    public Scope? parent;
    public Dictionary<string, Symbol> symbols = new();

    public Scope(Scope? parent = null)
    {
        this.parent = parent;
    }

    public bool TryGetSymbol(string name, out Symbol? symb)
    {
        if(symbols.TryGetValue(name,out Symbol? symbol))
        {
            symb = symbol;
            return true;
        }

        if (parent!=null && parent.TryGetSymbol(name, out symb))
        {
            return true;
        }


        symb = null;


        return false;
    }

    
    public void AddSymbol(string name, IRDataType dataType = IRDataType.None)
    {
        symbols.Add(name, new Symbol(name, dataType, this));
    }

    public void Show(int depth = 0)
    {
        string outStr = "{" + $" {depth} " + "}";

        string distStr = "";
        for(int i = 0; i<depth; i++)
        {
            distStr += "-";
        }

        Console.WriteLine(outStr);

        foreach(Symbol value in symbols.Values)
        {
            Console.WriteLine(distStr + value.Name +":"+ value.DataType);
        }

    }

}