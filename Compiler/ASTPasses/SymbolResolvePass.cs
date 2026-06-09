using Parsing;
using IR;
using Errors;

namespace ASTPasses;


public class SymbolResolver : ASTVisitor
{

    //This will be treated as a stack of symbols
    private List<Scope> scopeStack = new();
    private int stackPointer = 0;

    private int currentScope = 0;


    public SymbolResolver()
    {
        scopeStack.Add(new Scope()); //Push top scope
    }

    public void DisplayScopes()
    {
        int count = 0;
        foreach(Scope s in scopeStack)
        {
            s.Show(count);
            count++;
        }
    }

    private void EnterScope()
    {
        scopeStack.Add(new Scope());
        stackPointer++;
    }

    private void LeaveScope()
    {
        stackPointer--;
    }

    public override void Visit(BlockNode node)
    {
        EnterScope();
    }

    public override void Exit(BlockNode node)
    {
        LeaveScope();
    }

    

    public override void Visit(VariableNode node)
    {

        IRDataType dataType = IRBuilder.GetDTFromToken(node.dataTypeToken);
        string name = node.name.value.value;

        if(scopeStack[stackPointer].TryGetSymbol(name, out Symbol? symb))
        {
            ErrorHandler.AddError(ErrorType.NameError, node.GetLine(), $"Symbol '{dataType}:{name}' gets redefined.");
            return;
        }

        scopeStack[stackPointer].AddSymbol(name, dataType);

    }

    public override void Visit(Name node)
    {
        if(!scopeStack[stackPointer].TryGetSymbol(node.value.value,out Symbol? symb))
        {
            //Check upper scopes for definition.
            int lookBackPtr = stackPointer-1;
            while(lookBackPtr>=0)
            {
                if(scopeStack[lookBackPtr].TryGetSymbol(node.value.value,out Symbol? uSymb))
                { 
                    node.resolvedSymbol = uSymb;
                    node.dataType = uSymb.DataType;
                    return;
                }
                lookBackPtr--;
            }
            //used not defined name
            ErrorHandler.AddError(ErrorType.NameError, node.GetLine(), $"Name '{node.value.value}' not defined in current scope.");
        }
        else
        {
            node.resolvedSymbol = symb;
            node.dataType = symb.DataType;
        }
    }

    public List<Scope> GetScopes()
    {
        return scopeStack;
    }
}
