using IR;
using Parsing;
using Lexing;
using Errors;

namespace ASTPasses;

//WIP - Generation is done in AST for now.
// TODO: Do this, remember to track context and put var defs before loops
public class IRGeneratePass : ASTVisitor
{
    
    private readonly IRBuilder builder;
    private Stack<Context> contexts = new(); 

    //both used for the current loop, because loop_escape,loop_enter change in if.
    private string curLoopEnter;
    private string curLoopEscape;


    public IRGeneratePass(IRBuilder builder)
    {
        contexts.Push(new(){type = ContextType.TopLevel, escapeLabel = "_P_END"});
     
        this.builder = builder;

        builder.StartFunction(); //Top level scope is treated as a function.
    }

    public override void Visit(BinOp node)
    {
        node.right.AcceptVisitor(this);
        node.left.AcceptVisitor(this);
        builder.MakeOperator(IRBuilder.GetInstrFromOp(node.value));
    }

    public override void Visit(CallNode node)
    {
        foreach(ASTNode n in node.callExpr)
        {
            n.AcceptVisitor(this);
        }
        
        builder.MakeCall(node.caller.value.value);
    }

    public override void Visit(Name node)
    {
        builder.MakeLoad(node.resolvedSymbol.id); 
    }

    public override void Visit(NegateNode node)
    {
        node.Expr.AcceptVisitor(this);
        builder.MakeConstant("-1",builder.GetLastDT());
        builder.MakeOperator(InstrType.Mul);
    }

    public override void Visit(NotNode node)
    {
        node.Expr.AcceptVisitor(this);
        builder.MakeNot();
    }

    public override void Visit(ConstValue node)
    {
        builder.MakeConstant(node.value.value, ASTNode.GetValueDataType(node.value));
    }

    public override void Visit(VariableNode node)
    {
        if(node.name.resolvedSymbol == null){ErrorHandler.ThrowCLIError("[Internal] Unresolved symbol in ir gen.");}

        node.varValueExpr.AcceptVisitor(this);
        builder.MakeDefine(node.name.resolvedSymbol.id, node.name.resolvedSymbol.dataType);

        if(!(node.varValueExpr is NullValue))
        {    
            builder.MakeSet(node.name.resolvedSymbol.id);
        }
    }

    public override void Visit(AssignNode node)
    {
        node.assignExpr.AcceptVisitor(this);
        builder.MakeSet(node.name.resolvedSymbol.id);
    }

    public override void Visit(WhileNode node)
    {

        contexts.Push(new(){enterLabel = builder.NewLabelName(), 
                            escapeLabel = builder.NewLabelName(),
                            type = ContextType.WhileCond});

        curLoopEnter = contexts.Peek().enterLabel;
        curLoopEscape = contexts.Peek().escapeLabel;
        builder.MakeLabel(contexts.Peek().enterLabel);

        node.expr.AcceptVisitor(this);
        node.body.AcceptVisitor(this);
    }

    public override void Visit(BreakNode node)
    {
        builder.MakeJump(curLoopEscape);
    }

    public override void Visit(ContinueNode node)
    {
        builder.MakeJump(curLoopEnter);
    }

    

    

    public override void Visit(IfNode node)
    {
        contexts.Push(new(){escapeLabel = builder.NewLabelName(), type = ContextType.IfCond});

        node.expr.AcceptVisitor(this);
        node.body.AcceptVisitor(this);
    }

    public override void Visit(BlockNode node)
    {
        if(contexts.Peek().type == ContextType.WhileCond)
        {
            Context whileCtx = contexts.Pop();

            builder.MakeCmp();
            builder.MakeJmpFalse(whileCtx.escapeLabel);

            contexts.Push(new(){type = ContextType.While,
                                enterLabel = whileCtx.enterLabel,
                                escapeLabel = whileCtx.escapeLabel});
        }
        else if(contexts.Peek().type == ContextType.IfCond)
        {
            Context ifCtx = contexts.Pop();

            contexts.Push(new(){type = ContextType.If,
                                enterLabel = ifCtx.enterLabel,
                                escapeLabel = ifCtx.escapeLabel});

            builder.MakeCmp();
            builder.MakeJmpFalse(ifCtx.escapeLabel);

        }

        foreach(ASTNode n in node.statements)
        {
            n.AcceptVisitor(this);
        }
    }

    public override void Exit(BlockNode node)
    {
        
        if(contexts.Peek().type == ContextType.While)
        {
            builder.MakeJump(contexts.Peek().enterLabel);
            builder.MakeLabel(contexts.Pop().escapeLabel);

            if(contexts.Peek().type == ContextType.While)
            {
                curLoopEnter = contexts.Peek().enterLabel;
                curLoopEscape = contexts.Peek().escapeLabel;
            }
        }
        else if(contexts.Peek().type == ContextType.If)
        {
            builder.MakeLabel(contexts.Pop().escapeLabel);
        }
    }



}