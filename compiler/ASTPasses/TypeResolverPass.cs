using System.Net.Http.Headers;
using Errors;
using IR;
using Lexing;
using Parsing;

namespace ASTPasses;



public class TypeResolver : ASTVisitor
{

    //TODO: for cleaner code, map what operations are supported between what data types

    public override void Visit(BinOp node)
    {
        //Todo: resolve names based on data types, if type isn't found, search scopes for var with correct type
        if(node.left.dataType != node.right.dataType)
        {
            ErrorHandler.AddError(ErrorType.TypeError, node.GetLine(), $"Can not apply '{node.value.value}' on {node.left.dataType} and {node.right.dataType}");
        }

        node.dataType = node.left.dataType;
    }

    public override void Visit(NotNode node)
    {
        node.dataType = IRDataType.Bool;
        if(node.Expr.dataType != IRDataType.Bool)
        {
            ErrorHandler.AddError(ErrorType.TypeError, node.GetLine(), $"'!' can only be used with bool expressions.");
        }
    }

    public override void Visit(ConstValue node)
    {
        node.dataType = ASTNode.GetValueDataType(node.value);
    }

    public override void Visit(VariableNode node)
    {
        node.dataType = IRBuilder.GetDTFromToken(node.dataTypeToken);

        if(node.varValue.dataType != node.dataType)
        {
            if(node.varValue is ConstValue)
            {
                if(node.varValue.dataType == IRDataType.Int && node.dataType == IRDataType.Float)
                {
                    node.varValue.dataType = IRDataType.Float; //23 can be interpreted as 23.0
                    return;
                }
            }    
            
            ErrorHandler.AddError(ErrorType.TypeError, node.GetLine(), $"Can not assign {node.varValue.dataType} to {node.dataType}");
        }

    }

    public override void Visit(AssignNode node)
    {
        if(node.assignExpr.dataType != node.name.dataType)
        {
            ErrorHandler.AddError(ErrorType.TypeError, node.GetLine(), $"Can not assign {node.name.dataType} to {node.assignExpr.dataType}");
        }

    }


    

}
