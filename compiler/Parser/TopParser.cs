
using Lexing;

namespace Parsing;

public partial class Parser
{
    public List<ASTNode> ParseModule()
    {
        List<ASTNode> moduleAST = new();

        while(Peek().type != TokenType.EOF)
        {
            moduleAST.Add(ParseStatement());
            //while (Peek().type == TokenType.NewLine) { Consume(); }
        }

        return moduleAST;

    }

    private ASTNode ParseStatement()
    {
        TokenType[] operators = [TokenType.Plus, TokenType.Sub, TokenType.Mul, TokenType.Div];
        ASTNode statement;

        if(Peek().type == TokenType.DataType)
        {
            statement = MakeVarDef();
        }
        else if(Peek().type == TokenType.If)
        {
            statement = MakeIf();
        }
        else if(Peek().type == TokenType.While)
        {
            statement = MakeWhile();
        }
        else if(Peek().type == TokenType.Name)
        {
            if (Peek(2).type == TokenType.Equal)
            {
                statement = MakeAssign();
            }
            else if(operators.Contains(Peek(2).type) && Peek(3).type == TokenType.Equal)
            {
                statement = MakePrefixAssign();
            }
            else
            {
                statement = MakeCall();
            }
        }
        else
        {
            Console.WriteLine("Tried to parse empty (\\n) statement");
            Consume();
            return ParseStatement();
        }

        Consume();
        Expect(TokenType.NewLine);

        return statement;
    }

    private ASTNode MakePrefixAssign()
    {
        
        Token name = Consume();
        Expect(TokenType.Name);

        Token op = Consume();
        //Expect operator
        Consume();
        Expect(TokenType.Equal);


        BinOp operation = new(new Name(name), new Operator(op), ParseExpression());
        AssignNode node = new(new Name(name),operation);
        
        return node;
        
    }

    private ASTNode MakeWhile()
    {
        Consume();
        Expect(TokenType.While);
        
        Consume();
        Expect(TokenType.OpenBrace);

        ASTNode expr = ParseExpression();

        Consume();
        Expect(TokenType.ClosedBrace);

        if(Peek().type == TokenType.NewLine)
        {
            Consume();
        }

        ASTNode block = MakeBlock();

        return new WhileNode(expr, block);


    }

    private ASTNode MakeIf()
    {
        Consume();
        Expect(TokenType.If);
        
        Consume();
        Expect(TokenType.OpenBrace);

        ASTNode expr = ParseExpression();

        Consume();
        Expect(TokenType.ClosedBrace);

        if(Peek().type == TokenType.NewLine)
        {
            Consume();
        }

        ASTNode block = MakeBlock();

        return new IfNode(expr, block);


    }

    private ASTNode MakeBlock()
    {
        Consume();
        Expect(TokenType.OpenCurBrace);

        BlockNode node = new();

        if(Peek().type == TokenType.NewLine && Peek(2).type == TokenType.ClosedCurBrace)
        {
            Consume();
            goto _End;
        }

        while(Peek().type != TokenType.ClosedCurBrace)
        {
            node.AddStatement(ParseStatement());
        }

        _End: //I'm lazy, people.

        Consume(); //Consume to }
        
        if(tokens[current].type == TokenType.NewLine)
        {
            Consume();
            Expect(TokenType.ClosedCurBrace);
        }

        return node;
    }


    private ASTNode MakeAssign()
    {
        Token name = Consume();
        Expect(TokenType.Name);

        Consume();
        Expect(TokenType.Equal);

        ASTNode expr = ParseExpression();

        return new AssignNode(new Name(name), expr);
    }

    private ASTNode MakeVarDef()
    {
        Token dataType = Consume();
        Consume(); //Expect : here
        Expect(TokenType.Colon,"':'");

        Token name = Consume();
        Expect(TokenType.Name, "name");

        

        if(Consume().type == TokenType.SemiColon)
        {
            
            return new VariableNode(new Name(name), dataType,new NullValue());
        }
        else
        {
            Expect(TokenType.Equal, "'=' or ';'");
            return new VariableNode(new Name(name), dataType, ParseExpression());
        }

    }

    private ASTNode MakeCall()
    {
        Token name = Consume();
        Expect(TokenType.Name);

        Name nameNode = new Name(name);

        Consume();
        Expect(TokenType.OpenBrace);

        if(Peek().type == TokenType.ClosedBrace)
        {
            Consume();
            return new CallNode(nameNode);
        }

        CallNode node = new(nameNode);



        do
        {
            node.AddArgument(ParseExpression());

            if(Peek().type == TokenType.Comma)
            {
                Consume();
            }
        }
        while(tokens[current].type == TokenType.Comma);

        Consume(); //) from func call
        Expect(TokenType.ClosedBrace);

        return node;

    }
}