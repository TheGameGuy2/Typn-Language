using IR;
using Parsing;
using Lexing;

namespace ASTPasses;

//WIP - Generation is done in AST for now.
// TODO: Do this, remember to track context and put var defs before loops
public class IRGeneratePass : ASTVisitor
{
    
    private readonly IRBuilder builder;
    private Context currentCtx;



    public IRGeneratePass(IRBuilder builder)
    {
        this.currentCtx.type = ContextType.TopLevel;
        this.currentCtx.escapeLabel = "_P_END";
        this.builder = builder;
    }

    public override void Visit(BinOp node)
    {
        switch(node.value.type)
        {
            case TokenType.Plus:
                builder.MakeOperator(InstrType.Add);
                break;
            case TokenType.Sub:
                builder.MakeOperator(InstrType.Sub);
                break;
            case TokenType.Div:
                builder.MakeOperator(InstrType.Div);
                break;
            case TokenType.Mul:
                builder.MakeOperator(InstrType.Mul);
                break;
        }

    }


    public override void Visit(AssignNode node)
    {
        if(currentCtx.type == ContextType.While)
        {
            
        }
        builder.MakeDefine(node.name.resolvedSymbol.id, node.name.resolvedSymbol.dataType);
    }

    public override void Visit(WhileNode node)
    {
        
    }


}