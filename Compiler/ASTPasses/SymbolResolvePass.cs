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
        foreach(ASTNode n in node.statements)
        {
            n.AcceptVisitor(this);
        }
    }

    public override void Exit(BlockNode node)
    {
        LeaveScope();
    }

    public override void Visit(BinOp node)
    {
        node.right.AcceptVisitor(this);
        node.left.AcceptVisitor(this);
    }

    public override void Visit(IfNode node)
    {
        node.expr.AcceptVisitor(this);
        node.body.AcceptVisitor(this);
    }

    public override void Visit(NotNode node)
    {
        node.Expr.AcceptVisitor(this);
    }

    public override void Visit(CallNode node)
    {
        foreach(ASTNode n in node.callExpr)
        {
            n.AcceptVisitor(this);
        }
    }

    public override void Visit(NegateNode node)
    {
        node.Expr.AcceptVisitor(this);
    }

    public override void Visit(WhileNode node)
    {
        node.expr.AcceptVisitor(this);
        node.body.AcceptVisitor(this);
    }

    public override void Visit(VariableNode node)
    {
        node.varValueExpr.AcceptVisitor(this);

        IRDataType dataType = IRBuilder.GetDTFromToken(node.dataTypeToken);
        string name = node.name.value.value;

        if(current.TryGetSymbol(name, out Symbol? symb))
        {
            ErrorHandler.AddError(ErrorType.NameError, node.GetLine(), $"Symbol '{dataType}:{name}' gets redefined.");
            return;
        }

        node.name.resolvedSymbol = current.AddSymbol(name, dataType);
        

    }

    public override void Visit(AssignNode node)
    {
        node.name.AcceptVisitor(this);
        node.assignExpr.AcceptVisitor(this);
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
