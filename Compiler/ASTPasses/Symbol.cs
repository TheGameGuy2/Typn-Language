using IR;

namespace ASTPasses;

public record class Symbol(string name, string id, IRDataType dataType, Scope scope);

public class Scope
{
    public Scope? parent;
    public string scopeID;
    public int blockCount = 0; //amount of block statements inside scope. (subscopes)
    public Dictionary<string, Symbol> symbols = new();

    public Scope(Scope? parent = null, string scopeID = "_0")
    {
        this.parent = parent;
        this.scopeID = scopeID;
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

    /// <summary>
    /// Adds a subscope
    /// </summary>
    /// <returns>New subscope</returns>
    public Scope AddSubscope()
    {
        Scope s = new(this, scopeID + "_" + blockCount);
        blockCount++;

        return s;
    }

    public void AddSymbol(string name, IRDataType dataType = IRDataType.None)
    {
        symbols.Add(name, new Symbol(name,name + scopeID, dataType, this));
    }

    public void Show(int depth = 0)
    {
        string outStr = "{" + $" {scopeID} " + "}";

        string distStr = "";
        for(int i = 0; i<depth; i++)
        {
            distStr += "-";
        }

        Console.WriteLine(outStr);

        foreach(Symbol value in symbols.Values)
        {
            Console.WriteLine(distStr + value.id +":"+ value.dataType);
        }

    }

}