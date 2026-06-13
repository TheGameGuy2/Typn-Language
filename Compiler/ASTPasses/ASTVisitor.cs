using Parsing;

namespace ASTPasses;



public abstract class ASTVisitor
{
    public virtual void Visit(BinOp node){}

    public virtual void Visit(Operator node){}

    public virtual void Visit(ConstValue node){}

    public virtual void Visit(NegateNode node){}

    public virtual void Visit(NotNode node){}

    public virtual void Visit(Name node){}
    
    public virtual void Visit(VariableNode node){}

    public virtual void Visit(AssignNode node){}

    public virtual void Visit(CallNode node){}

    public virtual void Visit(BlockNode node){}

    public virtual void Exit(BlockNode node){}

    public virtual void Visit(IfNode node){}

    public virtual void Visit(WhileNode node){}

    public virtual void Visit(BreakNode node){}

    public virtual void Visit(ContinueNode node){}

    public virtual void Visit(ReturnNode node){}

}


