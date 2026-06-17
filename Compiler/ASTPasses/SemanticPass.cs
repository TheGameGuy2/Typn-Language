using Errors;
using Parsing;
namespace ASTPasses;


public enum ContextType : byte
{
    TopLevel,
    While,
    WhileCond,
    If,
    IfCond,
    Fnc
}

public struct Context
{
    public string enterLabel;
    public string escapeLabel;
    public ContextType type;
}

public class SemanticAnalizer : ASTVisitor
{
    
    private List<Context> scopes = [];

    private bool HasContext(ContextType context)
    {
        for(int i = scopes.Count-1; i>=0; i--)
        {
            if(scopes[i].type == context)
            {
                return true;
            }
        }
        return false;
    }

    public SemanticAnalizer()
    {
        scopes.Add(new(){type=ContextType.TopLevel});
    }

    public override void Visit(WhileNode node)
    {
        scopes.Add(new(){type=ContextType.While});
    }

    public override void Visit(IfNode node)
    {
        scopes.Add(new(){type=ContextType.If});
    }

    public override void Exit(BlockNode node)
    {
        if(scopes.Count>1)
        {     
            scopes.RemoveAt(scopes.Count-1);
        }
    }

    public override void Visit(BreakNode node)
    {
        if(!HasContext(ContextType.While))
        {
           ErrorHandler.AddError(ErrorType.SyntaxError,node.GetLine(),"No enclosing loop for break.");
        }
    }

    public override void Visit(ContinueNode node)
    {
        if(!HasContext(ContextType.While))
        {
           ErrorHandler.AddError(ErrorType.SyntaxError,node.GetLine(),"No enclosing loop for continue.");
        }
    }

    public override void Visit(ReturnNode node)
    {
        ErrorHandler.AddError(ErrorType.SyntaxError, node.GetLine(),"return is not yet supported.");
    }


}

