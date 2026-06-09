using IR;
using Parsing;
using Lexing;

namespace ASTPasses;

//WIP - Generation is done in AST for now.

public class IRGeneratePass : ASTVisitor
{
    
    private readonly IRBuilder builder;


    public IRGeneratePass(IRBuilder builder)
    {
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
        switch(node.name.resolvedSymbol.DataType)
        {
            case IRDataType.Int:
            
            break;
        }
    }


}