using System.Runtime.Serialization;
using Lexing;

namespace Parsing;




public partial class Parser
{

    private List<Token> tokens;

    private int current = -1;

    public Parser(List<Token> tokens)
    {
        this.tokens = tokens;
    }

    private Token Consume()
    {
        if(current+1 == tokens.Count)
        {
            return new Token(TokenType.EOF, "EOF");
        }
        current++;
        //Console.WriteLine($"current: {tokens[current]}");
        return tokens[current];
    }

    private Token Peek(int lookAhead = 1)
    {
        if(current+lookAhead >= tokens.Count)
        {
            return new Token(TokenType.EOF, "EOF");
        }
        return tokens[current+lookAhead];
    }

    private void Expect(TokenType type)
    {
        if(tokens[current].type != type)
        {
            throw new Exception($"Error: Expected {type} got {tokens[current]}");
        }
    }

    
    //Rule: All parse methods must stop at last related token. 

    // TODO -> so we can call inside of expressions
    private ASTNode MakePostfix()
    {

        if(Peek().type == TokenType.Name)
        {
            if(Peek(2).type == TokenType.OpenBrace)
            {
                return MakeCall();
            }
        }

        ASTNode left = MakePrimary();
        

        return left;
    }
    

    private ASTNode MakePrefix()
    {
        if(Peek().type == TokenType.Not)
        {
            Consume();
            return new NotNode(MakePostfix());
        }
        else if(Peek().type == TokenType.Sub)
        {
            Consume();
            return new NegateNode(MakePostfix());
        }
        else
        {
            return MakePostfix();
        }
    }

    private ASTNode MakeMultiplicative()
    {
        ASTNode left = MakePrefix();

        while (Peek().type == TokenType.Mul || Peek().type == TokenType.Div)
        {
            Token op = Consume();
            left = new BinOp(left,new Operator(op),MakePrefix());
        }

        return left;

    }

    private ASTNode MakeAdditive()
    {
        ASTNode left = MakeMultiplicative();

        while (Peek().type == TokenType.Plus || Peek().type == TokenType.Sub)
        {
            Token op = Consume();
            left = new BinOp(left,new Operator(op),MakeMultiplicative());
        }

        return left;

    }

    private ASTNode MakeLogical()
    {
        ASTNode left = MakeAdditive();

        

        TokenType[] compareOps = [TokenType.And,
                                    TokenType.Or,
                                    TokenType.Greater,
                                    TokenType.Lesser,
                                    TokenType.CompEqual,
                                    TokenType.NotEqual,
                                    TokenType.LessEqual,
                                    TokenType.GreaterEqual];

        TokenType next = Peek().type;

        while(compareOps.Contains(next))
        {
            Token op = Consume();
            left = new BinOp(left, new Operator(op), MakeAdditive());

            next = Peek().type;
        }

        return left;

    }

    public ASTNode ParseExpression()
    {
        return MakeLogical();
    }


    private ASTNode MakePrimary()
    {
        Token currentToken = Consume(); 

        if(currentToken.type == TokenType.INum || currentToken.type == TokenType.FNum || currentToken.type == TokenType.Bool)
        {
            ASTNode nNode = new ConstValue(currentToken);
            return nNode;
        }
        else if(currentToken.type == TokenType.Name)
        {
            ASTNode nNode = new Name(currentToken);
            return nNode;
        }
        else if(currentToken.type == TokenType.OpenBrace)
        {

            //Make Expr

            ASTNode expr = ParseExpression();
            Consume();
            Expect(TokenType.ClosedBrace); //Error Wrong closed brace
            
            return expr;
        }
        else
        {   
            throw new Exception($"Expected Value Expression or Name, got {currentToken}");
        }
        return new ASTNode();
    }


}
