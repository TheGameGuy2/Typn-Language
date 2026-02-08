namespace Lexing;



public enum TokenType
{
    Null,
    FNum, //fnum, inum only tell if a number has a decimal point
    INum,
    Bool, //bool litteral, true/false
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

public static class TokenDataType
{
    public readonly static string Int = "int";
    public readonly static string Float = "float";
    public readonly static string Bool = "bool";

}

public struct Token
{
    public TokenType type;
    public string? value;
    
    public readonly int line;

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

