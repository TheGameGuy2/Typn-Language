using System.ComponentModel;
using IR;
using Lexing;

namespace Parsing;

public class ASTNode
{
    public Token value;

    public virtual void Show(int depth)
    {
        string dist = "";
        for (int i = depth; i > 0; i--)
        {
            dist += "-";
        }
        Console.WriteLine($"{dist}{value}");
    }

    public virtual void MakeInstruction(IRBuilder builder)
    {
        
    }
}

public class BinOp : ASTNode
{
    public ASTNode left;
    public ASTNode right;

    public BinOp(ASTNode left, ASTNode op, ASTNode right)
    {
        this.left = left;
        this.right = right;
        this.value = op.value;
        
    }

    public override void Show(int depth)
    {
        base.Show(depth);
        left.Show(depth+1);
        right.Show(depth+1);
    }

    public override void MakeInstruction(IRBuilder builder)
    {
        left.MakeInstruction(builder);
        right.MakeInstruction(builder);
        builder.MakeArithmetic(value);

    }
    


}

public class Operator : ASTNode
{
    public Operator(Token value)
    {
        this.value = value;
    }



}

public class Number : ASTNode
{
    public Number(Token value)
    {
        this.value = value;
    }

    public override void MakeInstruction(IRBuilder builder)
    {


        Token dataType = new Token(TokenType.DataType, value.type switch { TokenType.INum => "int", TokenType.FNum => "float", _ => "INVALID"});
        //This is so stupid I have to manage data types smarter somehow.
        builder.MakePush(value, dataType);
    }
}

public class NegateNode : ASTNode
{
    public ASTNode expr;

    public NegateNode(ASTNode expr)
    {
        this.expr = expr;
        value = new Token(TokenType.Sub,"Negate");
    }

    public override void Show(int depth)
    {
        base.Show(depth);
        expr.Show(depth + 1);
    }

}

public class NotNode : ASTNode
{
    public ASTNode expr;
    public NotNode(ASTNode expr)
    {
        this.expr = expr;
        value = new Token(TokenType.Not,"Not");
    }

    public override void Show(int depth)
    {
        base.Show(depth);
        expr.Show(depth + 1);
    }
}

public class NullValue : ASTNode
{
    public NullValue()
    {
        value = new Token(TokenType.Null, "null");
    }
}

public class Name : ASTNode
{
    public Name(Token value)
    {
        this.value = value;
    }

    public override void MakeInstruction(IRBuilder builder)
    {

    }

}

public class VariableNode : ASTNode
{
    public Token name;
    public Token dataType;

    public ASTNode varValue;

    public VariableNode(Token name, Token dataType,ASTNode value)
    {
        this.name = name;
        this.dataType = dataType;
        this.varValue = value;
    }

    public override void Show(int depth)
    {
        value.value = $"VAR: {dataType} {name.value} =";
        base.Show(depth);
        varValue.Show(depth + 1);
    }

    public override void MakeInstruction(IRBuilder builder)
    {
        if(varValue.value.type == TokenType.Null)
        {
            builder.MakeDefine(name, dataType);
            return;
        }

        varValue.MakeInstruction(builder);


        builder.MakeDefine(name, dataType);
        builder.MakeSet(name);
    }

}

public class AssignNode : ASTNode
{
    public ASTNode assignExpr;
    public Token name;

    public AssignNode(Token name,ASTNode expr)
    {
        value = new Token(TokenType.Name, $"Assign {name}");
        assignExpr = expr;
        this.name = name;
    }

    public override void Show(int depth)
    {
        base.Show(depth);
        assignExpr.Show(depth + 1);
    }
}

public class CallNode : ASTNode
{
    public List<ASTNode> callExpr = new();
    public ASTNode caller;

    public CallNode(ASTNode caller)
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
}

public class BlockNode : ASTNode
{
    List<ASTNode> statements = new();

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
}

public class IfNode : ASTNode
{
    public ASTNode expr;
    public ASTNode body;

    public IfNode(ASTNode expression, ASTNode block)
    {
        this.value = new Token(TokenType.If, "if:");
        expr = expression;
        body = block;
    }

    public override void Show(int depth)
    {
        base.Show(depth);
        expr.Show(depth + 1);
        body.Show(depth + 1);

    }
}


public class WhileNode : ASTNode
{
    public ASTNode expr;
    public ASTNode body;

    public WhileNode(ASTNode expression, ASTNode block)
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
}