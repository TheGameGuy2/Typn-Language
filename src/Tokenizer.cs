
using System.Reflection.PortableExecutable;

namespace Lexing;


public class Tokenizer
{

    private string code;

    private int current = -1;
    
    private Dictionary<char,Token> tokenMap = new();
    private Dictionary<string, Token> keywordMap = new();

    private char[] doubleOperators = ['=','<','>','!','&','|'];
    private char[] specialSkipChars = ['\t',' '];




    public Tokenizer(string code)
    {
        this.code = code;

        tokenMap['+'] = new Token(TokenType.Plus, "+");
        tokenMap['-'] = new Token(TokenType.Sub, "-");
        tokenMap['*'] = new Token(TokenType.Mul, "*");
        tokenMap['/'] = new Token(TokenType.Div, "/");
        tokenMap[':'] = new Token(TokenType.Colon, ":");
        tokenMap[';'] = new Token(TokenType.SemiColon,";");
        tokenMap['\n'] = new Token(TokenType.NewLine, "\n");
        tokenMap['('] = new Token(TokenType.OpenBrace, "(");
        tokenMap[')'] = new Token(TokenType.ClosedBrace, ")");
        tokenMap['='] = new Token(TokenType.Equal, "=");
        tokenMap[','] = new Token(TokenType.Comma, ",");
        tokenMap['{'] = new Token(TokenType.OpenCurBrace, "{");
        tokenMap['}'] = new Token(TokenType.ClosedCurBrace, "}");
        tokenMap['!'] = new Token(TokenType.Not, "!");
        tokenMap['<'] = new Token(TokenType.Lesser, "<");
        tokenMap['>'] = new Token(TokenType.Greater, ">");

        keywordMap["int"] = new Token(TokenType.DataType, TokenDataType.Int);
        keywordMap["flt"] = new Token(TokenType.DataType, TokenDataType.Float);
        keywordMap["bol"] = new Token(TokenType.DataType, TokenDataType.Bool);

        keywordMap["=="] = new Token(TokenType.CompEqual,"==");
        keywordMap["!="] = new Token(TokenType.NotEqual, "!=");
        keywordMap["<="] = new Token(TokenType.LessEqual,"<=");
        keywordMap[">="] = new Token(TokenType.GreaterEqual, ">=");
        keywordMap["&&"] = new Token(TokenType.And,"&&");
        keywordMap["||"] = new Token(TokenType.Or,"||");

        keywordMap["true"] = new Token(TokenType.Bool, "1");
        keywordMap["false"] = new Token(TokenType.Bool, "0");

        keywordMap["if"] = new Token(TokenType.If, "if");
        keywordMap["while"] = new Token(TokenType.While, "while");
    }

    private bool IsNameLetter(char c)
    {
        return char.IsAsciiLetter(c) || c == '_';
    }

    private char Consume()
    {
        if (current+1 == code.Length)
        {
            return '\0';
        }
        current++;
        return code[current];
    }

    private char Peek()
    {
        int index = current + 1;

        if (index >= code.Length)
        {
            return '\0';
        }

        return code[index];
    }

    public List<Token> MakeTokens()
    {
        List<Token> tokens = new();

        while (Consume() != '\0')
        {

            char curChar = code[current];

            if (curChar == '#')
            {
                Consume();
                if (curChar == '#' && Peek() == '#')
                {
                    MakeMultiLineComment();
                    Consume(); //Consume last #
                }
            }

            if (curChar == '/' && Peek() == '/')
            {
                while (Consume() != '\n') { }
            }

            if (char.IsDigit(curChar))
            {
                tokens.Add(MakeNumber());
            }
            else if (char.IsAsciiLetter(curChar))
            {
                tokens.Add(MakeKeyword());
            }

            curChar = code[current];

            if (doubleOperators.Contains(curChar))
            {
                tokens.Add(MakeDoubleOp());
            }
            else if (tokenMap.ContainsKey(curChar))
            {
                if (curChar == '\n')
                {
                    if (tokens.Count - 1 >= 0 && tokens[^1].type != TokenType.NewLine)
                    {
                        tokens.Add(tokenMap[curChar]);
                    }
                }
                else
                {
                    tokens.Add(tokenMap[curChar]);
                }

            }
            else if (!specialSkipChars.Contains(curChar))
            {
                Console.WriteLine($"[Lexer] Warning, unexpected char \"{curChar}\"");
                //throw new Exception($"[Lexer] Unexpected character '{curChar}'");
            }







        }

        if(tokens.Count>0 && tokens[^1].type != TokenType.NewLine)
        {
            tokens.Add(tokenMap['\n']);
            //parser will be very angry if we don't do this
            //(it expects \n after each statement, last line could end in EOF)
        }

        tokens.Add(new Token(TokenType.EOF, "EOF"));
        return tokens;
    }

    private void MakeMultiLineComment()
    {

        char cur = Consume();
        while(cur != '\0')
        {
            
            if(cur == '#' && Peek() == '#')
            {
                Consume();
                if(Peek() == '#')
                {
                    Consume();
                    return;
                }
            }
            cur = Consume();
        }
    }

    private Token MakeDoubleOp()
    {
        string op ="" + code[current];

        char next = Peek();

        if(doubleOperators.Contains(next))
        {
            op += next;
            Consume();
        }
        else
        {
            if(!tokenMap.ContainsKey(op[0]))
            {
                throw new Exception($"[Lexer] Invalid operator {op[0]}");
            }
            return tokenMap[op[0]];
        }

        //check keywordMap for op
        if(keywordMap.ContainsKey(op))
        {
            return keywordMap[op];
        }
        else
        {
            //Error
            throw new Exception($"[Lexer] Invalid operator {op}");
            return new Token();
        }
    }

    private Token MakeNumber()
    {
        string num = "";
        bool isFloat = false;

        do
        {
            if (code[current] == '.')
            {
                if (isFloat)
                {
                    throw new Exception("[Lexer] Too many . in float");
                }
                isFloat = true;
            }

            num += code[current];
            
        }
        while (IsDigitOrDot(Consume()));

        return isFloat ? new Token(TokenType.FNum, num) : new Token(TokenType.INum, num);

    }

    private bool IsDigitOrDot(char c)
    {
        return (char.IsDigit(c) || c == '.');
    }

    private Token MakeKeyword()
    {
        string name = "";

        do
        {
            name += code[current];
        }
        while (IsNameLetter(Consume()));


        return keywordMap.ContainsKey(name) ? keywordMap[name] : new Token(TokenType.Name, name);


    }

}
