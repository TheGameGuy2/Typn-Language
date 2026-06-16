using Errors;
using IR;
using Lexing;
using Parsing;

namespace ASTPasses;


internal record struct TypeTuple(IRDataType type1, IRDataType type2);


public class TypeResolver : ASTVisitor
{


    private readonly Dictionary<(IRDataType,IRDataType), HashSet<TokenType>> acceptedOps = new();

    
    //TODO: Automatic float convert of constant values if added float(4:int)+float(2.4)
    public TypeResolver()
    {

        HashSet<TokenType> intOps = 
        [TokenType.Plus, TokenType.Sub,
         TokenType.Mul, TokenType.Div, 
         TokenType.Lesser, TokenType.LessEqual,
         TokenType.Greater, TokenType.GreaterEqual,
         TokenType.CompEqual, TokenType.NotEqual
        ];
        acceptedOps.Add(new(IRDataType.Int, IRDataType.Int), intOps);

        HashSet<TokenType> fltOps = 
        [TokenType.Plus, TokenType.Sub,
         TokenType.Mul, TokenType.Div,
         TokenType.Lesser, TokenType.LessEqual,
         TokenType.Greater, TokenType.GreaterEqual,
         TokenType.CompEqual, TokenType.NotEqual
        ];
        acceptedOps.Add(new(IRDataType.Float, IRDataType.Float), fltOps);

        HashSet<TokenType> boolOps = 
        [ TokenType.And, TokenType.Not,
          TokenType.Or
        ];
        acceptedOps.Add(new(IRDataType.Bool, IRDataType.Bool), boolOps);


    }


    

    public override void Visit(BinOp node)
    {
        HashSet<TokenType> compareTypes = [TokenType.Lesser, TokenType.Greater, TokenType.LessEqual, TokenType.GreaterEqual];

        

        if(!acceptedOps.TryGetValue(new(node.left.dataType,node.right.dataType),out var supportedOps))
        {
            ErrorHandler.AddError(ErrorType.TypeError, node.GetLine(), $"Can not apply '{node.value.value}' on {node.left.dataType} and {node.right.dataType}");
        }
        else if(!supportedOps.Contains(node.value.type))
        {
            ErrorHandler.AddError(ErrorType.TypeError, node.GetLine(), $"Can not apply '{node.value.value}' on {node.left.dataType} and {node.right.dataType}");
        }
        
        
        node.dataType = compareTypes.Contains(node.value.type) ? IRDataType.Bool : node.left.dataType;
    }

    public override void Visit(Name node)
    {

        if(node.resolvedSymbol!=null)
        {
            node.dataType = node.resolvedSymbol.dataType;
        }
        else
        {
            ErrorHandler.AddError(ErrorType.TypeError, node.GetLine(), $"Found name '{node.value}' with unresolved symbol in TypeResolve.");
        }
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
        Symbol? varSymbol = node.name.resolvedSymbol; 
        if(varSymbol!=null)
        {
            node.dataType = node.name.resolvedSymbol.dataType;
        }
        //else: Symbol not found, error in Symbol resolver.

        if(node.varValueExpr.dataType != node.dataType && node.varValueExpr.dataType != IRDataType.None)
        {
            if(node.varValueExpr is ConstValue)
            {
                if(node.varValueExpr.dataType == IRDataType.Int && node.dataType == IRDataType.Float)
                {
                    node.varValueExpr.dataType = IRDataType.Float; //23 can be interpreted as 23.0
                    return;
                }
            }    
            
            ErrorHandler.AddError(ErrorType.TypeError, node.GetLine(), $"Can not assign {node.varValueExpr.dataType} to {node.dataType}");
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
