using System.ComponentModel;
using System.Globalization;
using System.Numerics;
using System.Runtime.CompilerServices;
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

    public virtual void MakeInstruction(IRBuilder builder){}

    public virtual bool IsComparison()
    {
        return false;
    }

    public void MakeConditionalJump(TokenType compOp, IRBuilder builder, string label)
    {
        switch(compOp)
        {
            case TokenType.Lesser:
                builder.MakeJmpGreater(label);
                builder.MakeJmpEQ(label);
                break;
            case TokenType.LessEqual:
                builder.MakeJmpGreater(label);
                break;
            case TokenType.Greater:
                builder.MakeJmpLess(label);
                builder.MakeJmpEQ(label);
                break;
            case TokenType.GreaterEqual:
                builder.MakeJmpLess(label);
                break;
            case TokenType.Equal:
                builder.MakeJmpNEQ(label);
                break;
            case TokenType.NotEqual:
                builder.MakeJmpEQ(label);
                break;
        }
    }
}

public class BinOp : ASTNode
{
    public ASTNode left;
    public ASTNode right;

    private bool isCompare = false;

    private static TokenType[] compTypes = [TokenType.CompEqual,
                                            TokenType.NotEqual,
                                            TokenType.Lesser,
                                            TokenType.LessEqual,
                                            TokenType.Greater,
                                            TokenType.GreaterEqual,
                                            ];
    private static TokenType[] logicalTypes = [TokenType.And, TokenType.Or];

    public BinOp(ASTNode left, ASTNode op, ASTNode right)
    {
        this.left = left;
        this.right = right;
        this.value = op.value;

        if(compTypes.Contains(op.value.type))
        {
            isCompare = true;
        }
        
    }

    public override void Show(int depth)
    {
        base.Show(depth);
        left.Show(depth+1);
        right.Show(depth+1);

    }

    public override void MakeInstruction(IRBuilder builder)
    {
        right.MakeInstruction(builder);
        left.MakeInstruction(builder);

        if(!isCompare)
        {
            builder.MakeOperator(IRBuilder.GetInstrFromOp(value));
        }

        

    }

    public override bool IsComparison()
    {
        return isCompare;
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
        IRDataType type = IRDataType.None;
        
        if(value.type == TokenType.FNum)
        {
            type = IRDataType.Float;
        }
        else if(value.type == TokenType.INum)
        {
            type = IRDataType.Int;
        }
        
        //This is so stupid I have to manage data types smarter somehow.
        builder.MakeConstant(value.value, type);
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

    public override void MakeInstruction(IRBuilder builder)
    {
        if(expr is Number)
        {
            switch(expr.value.type)
            {
                case TokenType.FNum:
                    builder.MakeConstant((-1*float.Parse(expr.value.value,CultureInfo.InvariantCulture)).ToString(), IRDataType.Float);
                    break;
                case TokenType.INum:
                    builder.MakeConstant((-1*int.Parse(expr.value.value)).ToString(), IRDataType.Int);
                    break;
            }
        }
        else
        {
            expr.MakeInstruction(builder);
            builder.MakeConstant("-1",builder.GetLastDT());
            builder.MakeOperator(IRBuilder.GetInstrFromOp(new Token(TokenType.Mul, "*")));
        }

        

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

    public override void MakeInstruction(IRBuilder builder)
    {
        expr.MakeInstruction(builder);
        builder.MakeNot();
    }

    public override bool IsComparison()
    {
        return true;
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
        builder.MakeLoad(value.value);
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
            builder.MakeDefine(name.value, IRBuilder.GetDTFromToken(dataType));
            return;
        }

        varValue.MakeInstruction(builder);

        builder.MakeDefine(name.value, IRBuilder.GetDTFromToken(dataType));
        builder.MakeSet(name.value);
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

    public override void MakeInstruction(IRBuilder builder)
    {
        assignExpr.MakeInstruction(builder);
        builder.MakeSet(name.value);
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

    public override void MakeInstruction(IRBuilder builder)
    {
        foreach(ASTNode node in statements)
        {
            node.MakeInstruction(builder);
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

    public override void MakeInstruction(IRBuilder builder)
    {

        //builder.MakeConstant("0",IRDataType.Int);
        


        string labelName = builder.NewLabelName();

        if(expr.IsComparison())
        {
            expr.MakeInstruction(builder); 

            builder.MakeCmp();
            TokenType compType = expr.value.type;
            MakeConditionalJump(compType, builder, labelName);

        }
        else
        {
            builder.MakeConstant("0", builder.GetLastDT());

            expr.MakeInstruction(builder); 

            builder.MakeCmp();
            
            builder.MakeJmpLess(labelName);
            builder.MakeJmpEQ(labelName);
            
        }



        //TODO: Determine based on operation above (based on expr, == jne, != je, < jl, >jg etc.)
        //Wait no, you only jump if the logic above is 0 or false, make a comp for that!
        

        body.MakeInstruction(builder);
        
        builder.MakeLabel(labelName);
        builder.ClearLabel(labelName);
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

    public override void MakeInstruction(IRBuilder builder)
    {
        string startLabel = builder.NewLabelName();
        string endLabel = builder.NewLabelName();
        
        builder.MakeLabel(startLabel);

        


        if(expr.IsComparison())
        {
            expr.MakeInstruction(builder);
            builder.MakeCmp();
            TokenType compType = expr.value.type;
            MakeConditionalJump(compType, builder, endLabel);
            
        }
        else
        {
            builder.MakeConstant("0", builder.GetLastDT());

            expr.MakeInstruction(builder);
            
            builder.MakeCmp();

            builder.MakeJmpLess(endLabel);
            builder.MakeJmpEQ(endLabel);
        }

        body.MakeInstruction(builder);

        builder.MakeJump(startLabel);
        builder.MakeLabel(endLabel);

        //builder.ClearLabel(startLabel);
        builder.ClearLabel(endLabel);


    }
}