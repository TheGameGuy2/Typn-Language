namespace Lexing;



public enum TokenType
{
    Null,
    FNum,
    INum,
    Plus,
    Sub,
    Mul,
    Div,
    Equal,
    Not,
    And,
    Or,
    Lesser,
    Greater,
    CompEqual, //==
    LessEqual, //<=
    GreaterEqual, //>=
    NotEqual,
    Name,
    DataType,
    Colon,
    SemiColon,
    OpenBrace,
    ClosedBrace,
    OpenCurBrace, //{
    ClosedCurBrace, //}
    NewLine, //acts as an expression end in this lang.
    Comma,
    If,
    
    While,
    EOF
}

public struct Token
{
    public TokenType type;
    public string? value;

    public Token(){}

    public Token(TokenType type, string value)
    {
        this.type = type;
        this.value = value;
    }

    public override string ToString()
    {
        return $"[{type}|{value}]";
    }
}

