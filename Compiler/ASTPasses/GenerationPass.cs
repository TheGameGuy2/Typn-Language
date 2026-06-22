using IR;
using Parsing;
using Lexing;
using Errors;

namespace ASTPasses;

public class IRGeneratePass : ASTVisitor
{    
    private readonly IRBuilder builder;
    private Stack<Context> contexts = new(); 

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
        //Entering while condition context
        contexts.Push(new(){enterLabel = builder.NewLabelName(), 
                            escapeLabel = builder.NewLabelName(),
                            type = ContextType.WhileCond});

        //Storing escape and enter labels.
        string curLoopEnter = contexts.Peek().enterLabel;
        string curLoopEscape = contexts.Peek().escapeLabel;

        //Making label on top of loop condition
        builder.MakeLabel(curLoopEnter);

        //Loop condition
        node.expr.AcceptVisitor(this);
        
        builder.MakeCmp();
        builder.MakeJmpFalse(curLoopEscape);

        contexts.Pop(); //Leaving loop condition context

        //Entered while body context.
        contexts.Push(new(){enterLabel = curLoopEnter,
                            escapeLabel = curLoopEscape,
                            type = ContextType.While});



        node.body.AcceptVisitor(this);

        contexts.Pop(); //Leave while context

        //Loop end
        builder.MakeJump(curLoopEnter);
        builder.MakeLabel(curLoopEscape);

    }

    public override void Visit(BreakNode node)
    {
        builder.MakeJump(contexts.Peek().escapeLabel);
    }

    public override void Visit(ContinueNode node)
    {
        builder.MakeJump(contexts.Peek().enterLabel);
    }

    

    

    public override void Visit(IfNode node)
    {
        string elseLabel = builder.NewLabelName();
        string endLabel = builder.NewLabelName(); //End label is jumped to when condition is true -> skip else block

        contexts.Push(new(){escapeLabel = elseLabel, type = ContextType.IfCond});

        //Making if condition
        node.expr.AcceptVisitor(this);
        
        //Enter if body context
        contexts.Pop();
        contexts.Push(new(){escapeLabel = elseLabel, type = ContextType.If});

        //Check if condition
        builder.MakeCmp();
        builder.MakeJmpFalse(elseLabel);

        node.body.AcceptVisitor(this);

        contexts.Pop(); //leave if context

        if(node.elseBlock != null)
        {
            //If has else: adding jump to end of previous block, skipping else
            builder.MakeJump(endLabel);
            builder.MakeLabel(elseLabel);

            node.elseBlock.AcceptVisitor(this); //else block
            builder.MakeLabel(endLabel); //end label after else block
        }
        else
        {    
            builder.MakeLabel(elseLabel); //No else, using the else label as the end.
        }


    }

    public override void Visit(BlockNode node)
    {
        foreach(ASTNode n in node.statements)
        {
            n.AcceptVisitor(this);
        }
    }

}