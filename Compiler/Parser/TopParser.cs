
using Errors;
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

    /// <summary>
    /// Parses a statement
    /// </summary>
    /// <param name="allowClosingBracket">Allows '}' instead of '\n' at statement end</param>
    /// <returns></returns>
    private ASTNode ParseStatement()
    {
        TokenType[] operators = [TokenType.Plus, TokenType.Sub, TokenType.Mul, TokenType.Div];
        ASTNode statement;

        switch(Peek().type)
        {
            case TokenType.DataType:
                statement = MakeVarDef();
            break;

            case TokenType.If:
                statement = MakeIf();
            break;

            case TokenType.While:
                statement = MakeWhile();
            break;

            case TokenType.Break:
                statement = new BreakNode(Consume());
            break;

            case TokenType.Continue:
                statement = new ContinueNode(Consume());
            break;

            case TokenType.Return:
                statement = (Peek(2).type == TokenType.NewLine) ? new ReturnNode(Consume()) : new ReturnNode(Consume(), ParseExpression());
            break;

            case TokenType.Name:
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
            break;

            case TokenType.NewLine:
                Consume();
                return ParseStatement();

            default:
                ErrorHandler.AddError(ErrorType.SyntaxError,Peek().line,"Invalid statement.",true);
                return new ASTNode();//This will never happen
            break;

        }

        
        
        Consume();
        Expect(TokenType.NewLine, "'\\n'"); 

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

        BlockNode block = (BlockNode)MakeBlock();

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
        Expect(TokenType.OpenBrace,"function call");

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