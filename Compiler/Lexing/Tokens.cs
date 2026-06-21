namespace Lexing;



public enum TokenType
{
    Null,
    FNum, //fnum, inum only tell if a number has a decimal point
    INum,
    Bool, //bool literal, true/false
    String, //string literal
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
    Else,
    Break,
    Continue,
    Return,
    EOF
}


public static class TokenDataType
{
    //This is used for the value of data type tokens so we can compare their data types without magic values.
    public readonly static string Int = "int";
    public readonly static string Float = "float";
    public readonly static string Bool = "bool";
    public readonly static string String = "str";

}

public struct Token
{
    public TokenType type;
    public string? value;
    
    public int line = -1;

    public Token(){}

    public Token(TokenType type, string value)
    {
        this.type = type;
        this.value = value;
    }

    public override string ToString()
    {
        return $"[{type}|{value} l{line}]";
    }
}

