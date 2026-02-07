
using Parsing;
using IR;
using Errors;

namespace ASTPasses;


public class SymbolResolver : ASTVisitor
{

    //This will be treated as a stack of symbols
    private Stack<Scope> scopeStack = new();
    private int currentScope = 0;


    public SymbolResolver()
    {
        scopeStack.Push(new Scope()); //Push top scope
    }

    private void EnterScope()
    {
        scopeStack.Push(new Scope(scopeStack.Peek()));
    }

    private void LeaveScope()
    {
        scopeStack.Pop();
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
        string name = node.name.value;

        if(scopeStack.Peek().TryGetSymbol(name, out Symbol? symb))
        {

            ErrorHandler.AddError(ErrorType.NameError, -1, $"Symbol {dataType}:{name} gets redefined.");
            return;
        }

        scopeStack.Peek().AddSymbol(name, dataType);
    }

    public override void Visit(Name node)
    {
        if(!scopeStack.Peek().TryGetSymbol(node.value.value,out Symbol symb))
        {
            //used not defined name
            ErrorHandler.AddError(ErrorType.NameError, -1, $"Name {node.value.value} not defined.");
        }
        else
        {
            node.resolvedSymbol = symb;
        }
    }
    

    
}
