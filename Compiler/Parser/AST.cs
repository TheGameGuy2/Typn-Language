using System.Globalization;
using Errors;
using IR;
using Lexing;
using ASTPasses;

namespace Parsing;

//TODO: Generate IR in separate AST pass.


//Base class
public class ASTNode
{
    public Token value;
    public IRDataType dataType;

    public virtual void Show(int depth)
    {
        string dist = "";
        for (int i = depth; i > 0; i--)
        {
            dist += "-";
        }
        Console.WriteLine($"{dist}{value}");
    }

    
    public virtual int GetLine()
    {
        return value.line;
    }

    public virtual void MakeInstruction(IRBuilder builder){}

    /// <summary>
    ///     Gets the data type of a value token, do not confuse with DataType tokens
    /// </summary>
    /// 
    public static IRDataType GetValueDataType(Token tok)
    {
        return tok.type switch
        {
            TokenType.FNum => IRDataType.Float,
            TokenType.INum => IRDataType.Int,
            TokenType.Bool => IRDataType.Bool,
            TokenType.String => IRDataType.String,
            TokenType.DataType => IRBuilder.GetDTFromToken(tok),
            _ => IRDataType.None
        };
    }
    public virtual void AcceptVisitor(ASTVisitor visitor)
    {

    }
    
}


//Binary operations, +, -, * , / ...
public class BinOp : ASTNode
{
    public ASTNode left;
    public ASTNode right;

    
    public BinOp(ASTNode left, ASTNode op, ASTNode right)
    {

        this.left = left;
        this.right = right;
        this.value = op.value;

        dataType = left.dataType;


    }

    public override void Show(int depth)
    {
        base.Show(depth);
        left.Show(depth+1);
        right.Show(depth+1);

    }

    public override void AcceptVisitor(ASTVisitor visitor)
    {

        right.AcceptVisitor(visitor);
        left.AcceptVisitor(visitor);
        visitor.Visit(this);
    }

    public override void MakeInstruction(IRBuilder builder)
    {
        right.MakeInstruction(builder);
        left.MakeInstruction(builder);

        builder.MakeOperator(IRBuilder.GetInstrFromOp(value));
        
    }

    

}


public class Operator : ASTNode
{

    

    public Operator(Token value)
    {
        this.value = value;
    }

    public override void AcceptVisitor(ASTVisitor visitor)
    {
        visitor.Visit(this);
    }

}

//Constant value
public class ConstValue : ASTNode
{
    public ConstValue(Token value)
    {
        this.value = value;

        dataType = GetValueDataType(value);
    }

    public override void MakeInstruction(IRBuilder builder)
    {
        
        
        //This is so stupid I have to manage data types smarter somehow.
        builder.MakeConstant(value.value, GetValueDataType(value));
    }

    public override void AcceptVisitor(ASTVisitor visitor)
    {
        visitor.Visit(this);
    }
}

// - prefix, -1 , -(23*32+2)
public class NegateNode : ASTNode
{
    public ASTNode Expr;

    public NegateNode(ASTNode expr)
    {
        this.Expr = expr;
        value = new Token(TokenType.Sub,"Negate");

        
        dataType = expr.dataType;
    }

    public override void Show(int depth)
    {
        base.Show(depth);
        Expr.Show(depth + 1);
    }

    public override void MakeInstruction(IRBuilder builder)
    {
        if(Expr is ConstValue)
        {
            switch(Expr.value.type)
            {
                case TokenType.FNum:
                    builder.MakeConstant((-1*float.Parse(Expr.value.value,CultureInfo.InvariantCulture)).ToString(), IRDataType.Float);
                    break;
                case TokenType.INum:
                    builder.MakeConstant((-1*int.Parse(Expr.value.value)).ToString(), IRDataType.Int);
                    break;
            }
        }
        else
        {
            Expr.MakeInstruction(builder);
            builder.MakeConstant("-1",dataType);
            builder.MakeOperator(IRBuilder.GetInstrFromOp(new Token(TokenType.Mul, "*")));
        }

        

    }

    public override void AcceptVisitor(ASTVisitor visitor)
    {
        visitor.Visit(this);
        Expr.AcceptVisitor(visitor);
    }
    

}

//bool not, !true == false
public class NotNode : ASTNode
{
    public ASTNode Expr;
    public NotNode(ASTNode expr)
    {
        this.Expr = expr;
        value = new Token(TokenType.Not,"Not");
        value.line = Expr.value.line;

        dataType = IRDataType.Bool;
    }

    public override void Show(int depth)
    {
        base.Show(depth);
        Expr.Show(depth + 1);
    }

    public override void MakeInstruction(IRBuilder builder)
    {
        Expr.MakeInstruction(builder);
        builder.MakeNot();
    }

    public override void AcceptVisitor(ASTVisitor visitor)
    {
        Expr.AcceptVisitor(visitor);
        visitor.Visit(this);
    }
}

//This should be a constant
public class NullValue : ASTNode
{
    public NullValue()
    {
        value = new Token(TokenType.Null, "null");
    }
}

//Names
public class Name : ASTNode
{
    public Symbol? resolvedSymbol; 

    public Name(Token value)
    {
        this.value = value;
        
    }

    public override void MakeInstruction(IRBuilder builder)
    {
        builder.MakeLoad(value.value);
    }

    public override void AcceptVisitor(ASTVisitor visitor)
    {
        visitor.Visit(this);
    }

}

/// <summary>
/// Var declaration.
/// </summary>
public class VariableNode : ASTNode 
{
    public Name name;
    public Token dataTypeToken;
    public ASTNode varValueExpr;

    public VariableNode(Name name, Token dataTypeToken,ASTNode value)
    {
        this.name = name;
        this.dataTypeToken = dataTypeToken;
        this.varValueExpr = value;
        
        this.value.line = name.value.line;

        dataType = GetValueDataType(dataTypeToken);
    }

    public override void Show(int depth)
    {
        
        value.value = $"VAR: {dataTypeToken} {name.value} =";
        base.Show(depth);
        varValueExpr.Show(depth + 1);
    }

    public override void MakeInstruction(IRBuilder builder)
    {
        if(varValueExpr.value.type == TokenType.Null)
        {
            builder.MakeDefine(name.resolvedSymbol.id, IRBuilder.GetDTFromToken(dataTypeToken));
            return;
        }

        varValueExpr.MakeInstruction(builder);

        builder.MakeDefine(name.resolvedSymbol.id, IRBuilder.GetDTFromToken(dataTypeToken));
        builder.MakeSet(name.resolvedSymbol.id);
    }

    public override void AcceptVisitor(ASTVisitor visitor)
    {

        varValueExpr.AcceptVisitor(visitor);
        visitor.Visit(this);
    }

}

/// <summary>
/// Name assignemt
/// </summary>
public class AssignNode : ASTNode
{
    public ASTNode assignExpr;
    public Name name;

    public AssignNode(Name name,ASTNode expr)
    {
        value = new Token(TokenType.Name, $"Assign {name.value.value}");
        assignExpr = expr;

        this.name = name;
    }

    public override void Show(int depth)
    {
        base.Show(depth);
        assignExpr.Show(depth + 1);
    }

    public override void MakeInstruction(IRBuilder builder)
    {
        assignExpr.MakeInstruction(builder);
        builder.MakeSet(name.resolvedSymbol.id);
    }

    public override void AcceptVisitor(ASTVisitor visitor)
    {
        //because name gets written to, we only visit name in SymbolResolver
        assignExpr.AcceptVisitor(visitor);
        visitor.Visit(this);
    }
}

public class CallNode : ASTNode
{
    public List<ASTNode> callExpr = new();
    public Name caller;

    public CallNode(Name caller)
    {
        this.caller = caller;
        this.value.value = "Call";

    }

    public void AddArgument(ASTNode arg)
    {
        callExpr.Add(arg);
    }

    public override void Show(int depth)
    {
        value = new Token(TokenType.Name,$"call {caller.value}");
        base.Show(depth);
        
        foreach(ASTNode node in callExpr)
        {
            node.Show(depth + 1);
        }
    }

    public override void MakeInstruction(IRBuilder builder)
    {
        foreach(ASTNode node in callExpr)
        {
            node.MakeInstruction(builder);
        }
        
        if(caller.value.type != TokenType.Name)
        {
            throw new Exception($"[AST] Can not call {caller.value}");
        }

        builder.MakeCall(caller.value.value);
    }

    public override void AcceptVisitor(ASTVisitor visitor)
    {
        foreach(ASTNode node in callExpr)
        {
            node.AcceptVisitor(visitor);
        }
        visitor.Visit(this);
        //caller.AcceptVisitor(visitor);
    }
}

public class BlockNode : ASTNode
{
    public List<ASTNode> statements {get; private set;} = new();

    public BlockNode()
    {
        value.value = "{Block}";
    }

    public void AddStatement(ASTNode stmnt)
    {
        statements.Add(stmnt);
    }

    public override void Show(int depth)
    {
        base.Show(depth);
        foreach(ASTNode node in statements)
        {
            node.Show(depth + 1);
        }
    }

    public override void MakeInstruction(IRBuilder builder)
    {
        foreach(ASTNode node in statements)
        {
            node.MakeInstruction(builder);
        }
    }

    public override void AcceptVisitor(ASTVisitor visitor)
    {
        visitor.Visit(this);

        foreach(ASTNode node in statements)
        {
            node.AcceptVisitor(visitor);
        }

        visitor.Exit(this);

    }
}



public class IfNode : ASTNode
{
    public ASTNode expr;
    public ASTNode? elseBlock;
    public ASTNode body;

    public IfNode(ASTNode expression, ASTNode block, ASTNode? elseBlock = null)
    {
        this.value = new Token(TokenType.If, "if:");
        expr = expression;
        body = block;
        this.elseBlock = elseBlock ?? null;
        
    }

    public override void Show(int depth)
    {
        base.Show(depth);
        expr.Show(depth + 1);
        body.Show(depth + 1);

    }

    public override void MakeInstruction(IRBuilder builder)
    {

        //builder.MakeConstant("0",IRDataType.Int);
        
        string endLabel = builder.NewLabelName();

        expr.MakeInstruction(builder); 

        builder.MakeCmp();
        builder.MakeJmpFalse(endLabel);


        body.MakeInstruction(builder);
        
        builder.MakeLabel(endLabel);
        builder.ClearLabel(endLabel);
    }


    public override void AcceptVisitor(ASTVisitor visitor)
    {

        visitor.Visit(this);
        expr.AcceptVisitor(visitor);
        body.AcceptVisitor(visitor);
        elseBlock?.AcceptVisitor(visitor);
    }
}


public class WhileNode : ASTNode
{
    public ASTNode expr;
    public BlockNode body;

    public WhileNode(ASTNode expression, BlockNode block)
    {
        this.value = new Token(TokenType.While, "while:");
        expr = expression;
        body = block;
    }

    public override void Show(int depth)
    {
        base.Show(depth);
        expr.Show(depth + 1);
        body.Show(depth + 1);

    }

    public override void MakeInstruction(IRBuilder builder)
    {
        string startLabel = builder.NewLabelName();
        string endLabel = builder.NewLabelName();
        
        builder.MakeLabel(startLabel);
        
        expr.MakeInstruction(builder);
        builder.MakeCmp();
        builder.MakeJmpFalse(endLabel);



        body.MakeInstruction(builder);

        builder.MakeJump(startLabel);
        builder.MakeLabel(endLabel);

        //builder.ClearLabel(startLabel);
        builder.ClearLabel(endLabel);

    }

    public override void AcceptVisitor(ASTVisitor visitor)
    {
        visitor.Visit(this);
        expr.AcceptVisitor(visitor);
        body.AcceptVisitor(visitor);
    }
}



public class BreakNode : ASTNode
{
    
    public BreakNode(Token tok)
    {
        value = tok;
    }

    public override void AcceptVisitor(ASTVisitor visitor)
    {
        visitor.Visit(this);
    }

}

public class ContinueNode : ASTNode
{
    
    public ContinueNode(Token tok)
    {
        value = tok;
    }

    public override void AcceptVisitor(ASTVisitor visitor)
    {
        visitor.Visit(this);
    }

}

public class ReturnNode : ASTNode
{
    public ASTNode? retExpr;
    public ReturnNode(Token tok)
    {
        value = tok;
    }

    public ReturnNode(Token tok, ASTNode expression)
    {
        value = tok;
        retExpr = expression;
    }

    public override void AcceptVisitor(ASTVisitor visitor)
    {
        visitor.Visit(this);
    }

    public override void Show(int depth)
    {
        base.Show(depth);
        retExpr?.Show(depth+1);
    }

}