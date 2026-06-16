using Parsing;
using IR;
using Errors;

namespace ASTPasses;


public class SymbolResolver : ASTVisitor
{

    private Scope current;


    public SymbolResolver()
    {
        current = new Scope();
    }

    public void DisplayScopes()
    {
        current.Show();
    }

    private void EnterScope()
    {
        current = current.AddSubscope();
    }

    private void LeaveScope()
    {
        current = current.parent; 
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

        if(current.TryGetSymbol(name, out Symbol? symb))
        {
            ErrorHandler.AddError(ErrorType.NameError, node.GetLine(), $"Symbol '{dataType}:{name}' gets redefined.");
            return;
        }

        current.AddSymbol(name, dataType);

    }

    public override void Visit(Name node)
    {
        if(!current.TryGetSymbol(node.value.value,out Symbol? symb))
        {
            
            //used not defined name
            ErrorHandler.AddError(ErrorType.NameError, node.GetLine(), $"Name '{node.value.value}' not defined in current scope.");
        }
        else
        {
            node.resolvedSymbol = symb;
            node.dataType = symb.dataType;
        }
    }

    
}
