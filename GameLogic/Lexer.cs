using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace GameLogic
{
    public enum TokenType : byte { OPEN_PAREN, CLOSE_PAREN, OP, CONSTANT}
    public enum State : byte { DETERMINE, OP, NUMBER, EOF }

    public struct Token
    {
        public TokenType type;
        public int start;
        public int len;
    }

    public ref struct Lexer
    {        
        public ReadOnlySpan<char> input;
        int start;
        int pos;     
        List<Token> tokens;

        public void BeginLexing(string s)
        {
            this = new Lexer { input = s.AsSpan(), tokens = new List<Token>(8) };
            var state = DetermineToken();
            do
            {
                switch (state)
                {
                    case State.DETERMINE:
                        state = DetermineToken();
                        break;
                    case State.NUMBER:
                        state = LexNumber();
                        break;
                    case State.OP:
                        state = LexOp();
                        break;
                    default:
                        break;
                }
            } while (state != State.EOF);
            
        }

        public override string ToString()
        {
            string result = string.Empty;
            foreach (Token t in tokens)
            {
                result += input.Slice(t.start, t.len).ToString() + "\n";
            }
            return result;
        }

        public State DetermineToken() {
            while (true) {
                char c = next();
                if (IsWhiteSpace(c))
                {
                    ignore();
                }
                else if (c == '(')
                {
                    emit(TokenType.OPEN_PAREN);
                }
                else if (c == ')')
                {
                    emit(TokenType.CLOSE_PAREN);
                }
                else if (IsStartOfNumber(c))
                {
                    return State.NUMBER;
                }
                else if (pos >= input.Length)
                {
                    return State.EOF;
                }
                else
                {
                    return State.OP;
                }
            }
        }


        public State LexOp() {
            const string opchars = "+-/*abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            acceptRun(opchars);
            emit(TokenType.OP);
            return State.DETERMINE;
                
        }

        public State LexNumber() {
            const string numprefix = "-.";
            accept(numprefix);
            const string digits = "0123456789";
            acceptRun(digits);
            if (accept(".")) {
                acceptRun(digits);
            }
            if (input[start] == '-') {
                emit(TokenType.OP);         
            } else {
                emit(TokenType.CONSTANT);
            }

            return State.DETERMINE;
        }

        public bool accept(string valid) {
            if (valid.IndexOf(next()) >= 0) {
                return true;
            }
            backup();
            return false;
        }

        public void acceptRun(string valid) {
            while (valid.IndexOf(next()) >= 0) { }
            backup();
        }

        public void emit(TokenType type)
        {
            tokens.Add(new Token { type = type, start = start, len = pos-start});
        }

        public char next()
        {
            if (pos < input.Length)
            {
                char c = input[pos];
                pos++;
                return c;
            }

            return char.MaxValue;
        }

        public void backup()
        {
            pos--;
        }

        public void ignore()
        {
            start = pos;
        }

        public char peek()
        {
            return input[pos];
        }

        public static bool IsWhiteSpace(char c)
        {
            return c == ' ' || c == '\n' || c == '\t' || c == '\r';
        }

        public static bool IsStartOfNumber(char c)
        {
            return (c >= '0' && c <= '9') || c == '-' || c == '.';
        }


        
    }

    
}
