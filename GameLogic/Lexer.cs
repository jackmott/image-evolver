using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;



namespace GameLogic
{
    public enum TokenType : byte { OPEN_PAREN, CLOSE_PAREN, OP, VARIABLE, STRING, CONSTANT }
    public enum State : byte { DETERMINE, OP, NUMBER, STRING, EOF }

    public class ParseException : Exception
    {
        public Token token;

        public ParseException(string msg, Token token) : base(msg)
        {
            this.token = token;
        }
    }

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
        Queue<Token> tokens;
        Dictionary<string, NodeType> nodeDict;

        public Lexer(string s)
        {
            input = s.AsSpan();
            tokens = new Queue<Token>(8);
            nodeDict = new Dictionary<string, NodeType>();
            start = 0;
            pos = 0;

            foreach (var typeObj in Enum.GetValues(typeof(NodeType)))
            {
                var type = (NodeType)typeObj;
                if (type != NodeType.EMPTY && type != NodeType.CONSTANT)
                {
                    nodeDict.Add(AptNode.OpString(type).ToLower(), type);
                }
            }
        }

        public void BeginLexing()
        {
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
                    case State.STRING:
                        state = LexString();
                        break;
                    case State.OP:
                        state = LexOp();
                        break;
                    default:
                        break;
                }
            } while (state != State.EOF);

        }

        public AptNode stringToNode(string s, Token t)
        {
            NodeType type;
            if (nodeDict.TryGetValue(s, out type))
            {
                return AptNode.MakeNode(type);
            }
            else
            {
                throw new ParseException("invalid name:" + s, t);
            }

        }

        public Token ParseExpect()
        {
            try
            {
                return tokens.Dequeue();
            }
            catch (InvalidOperationException)
            {
                throw new Exception("Expected token but reached EOF");
            }
        }
        public Token ParseExpect(TokenType type)
        {
            try
            {
                var t = tokens.Dequeue();
                if (t.type != type)
                {
                    throw new ParseException("Expected tokentype " + type + " but got " + t.type, t);
                }
                return t;
            }
            catch (InvalidOperationException)
            {
                throw new ParseException("Expected tokentype " + type + " but reached EOF", new Token { });
            }


        }

        public Token ParseExpect(TokenType[] validTokens)
        {

            var t = ParseExpect();
            foreach (var valid in validTokens)
            {
                if (valid == t.type)
                {
                    return t;
                }
            }
            throw new ParseException("Invalid token", t);


        }

        public Pic ParsePic(GraphicsDevice g, GameWindow w)
        {
            ParseExpect(TokenType.OPEN_PAREN);
            var t = ParseExpect(TokenType.OP);
            var s = input.Slice(t.start, t.len).ToString().ToLower();
            Pic p;
            if (s == "gradient")
            {
                p = new Pic(PicType.GRADIENT, g, w);
                ParseExpect(TokenType.OPEN_PAREN);
                t = ParseExpect(TokenType.OP);
                s = input.Slice(t.start, t.len).ToString().ToLower();
                if (s != "hues")
                {
                    throw new ParseException("hues array expected after gradient",t);
                }

                var tempHues = new List<float>();
                while(true)
                {
                    t = ParseExpect(new[] { TokenType.CONSTANT, TokenType.CLOSE_PAREN });
                    if (t.type == TokenType.CLOSE_PAREN) break;
                    string numStr = input.Slice(t.start, t.len).ToString();
                    tempHues.Add(float.Parse(numStr));
                } 
                p.hues = tempHues.ToArray();
                ParseExpect(TokenType.OPEN_PAREN);
                t = ParseExpect(TokenType.OP);
                s = input.Slice(t.start, t.len).ToString().ToLower();
                if (s != "positions")
                {
                    throw new ParseException("position array expected after hues", t);
                }
                var tempPos = new List<float>();
                while (true)
                {
                    t = ParseExpect(new[] { TokenType.CONSTANT, TokenType.CLOSE_PAREN });
                    if (t.type == TokenType.CLOSE_PAREN) break;
                    string numStr = input.Slice(t.start, t.len).ToString();
                    tempPos.Add(float.Parse(numStr));
                }
                tempPos.Sort();
                p.pos = tempPos.ToArray();


                p.Trees[0] = ParseNodes();
                p.Machines[0] = new StackMachine(p.Trees[0]);

                p.Trees[1] = ParseNodes();
                p.Machines[1] = new StackMachine(p.Trees[1]);

                p.Trees[2] = ParseNodes();
                p.Machines[2] = new StackMachine(p.Trees[2]);
            }
            else if (s == "rgb")
            {
                p = new Pic(PicType.RGB, g, w);
                p.Trees[0] = ParseNodes();
                p.Machines[0] = new StackMachine(p.Trees[0]);

                p.Trees[1] = ParseNodes();
                p.Machines[1] = new StackMachine(p.Trees[1]);

                p.Trees[2] = ParseNodes();
                p.Machines[2] = new StackMachine(p.Trees[2]);
            }
            else if (s == "hsv")
            {
                p = new Pic(PicType.HSV, g, w);
                p.Trees[0] = ParseNodes();
                p.Machines[0] = new StackMachine(p.Trees[0]);

                p.Trees[1] = ParseNodes();
                p.Machines[1] = new StackMachine(p.Trees[1]);

                p.Trees[2] = ParseNodes();
                p.Machines[2] = new StackMachine(p.Trees[2]);
            }
            else
            {
                throw new ParseException("Top level type must be RGB, Gradient, or HSV", t);

            }

            ParseExpect(TokenType.CLOSE_PAREN);
            return p;

        }

        public AptNode ParseNodes()
        {
            var t = ParseExpect(new[] { TokenType.OPEN_PAREN, TokenType.CONSTANT, TokenType.VARIABLE });
            if (t.type == TokenType.VARIABLE)
            {
                var s = input.Slice(t.start, t.len).ToString().ToLower();
                return stringToNode(s, t);
            }
            else if (t.type == TokenType.CONSTANT)
            {
                var result = new AptNode { type = NodeType.CONSTANT };
                string numStr = input.Slice(t.start, t.len).ToString();
                result.value = float.Parse(numStr);
                return result;
            }
            else
            {
                t = ParseExpect(TokenType.OP);
                var s = input.Slice(t.start, t.len).ToString().ToLower();
                var result = stringToNode(s, t);
                if (result.type == NodeType.PICTURE)
                {
                    var filenameToken = ParseExpect(TokenType.STRING);
                    result.filename = input.Slice(filenameToken.start, filenameToken.len).ToString();

                }

                int warpCount = 0;
                for (int i = 0; i < result.children.Length; i++)
                {
                    result.children[i - warpCount] = ParseNodes();
                    result.children[i - warpCount].parent = result;                    
                    // warp returns two values, so it is a special case
                    if (result.children[i - warpCount].type == NodeType.WARP1)
                    {
                        warpCount++;
                        i++;
                    }
                }
                if (warpCount > 0)
                {
                    var newChildren = new AptNode[result.children.Length - warpCount];
                    for (int i = 0; i < newChildren.Length; i++)
                    {
                        newChildren[i] = result.children[i];
                    }
                    result.children = newChildren;
                }

                ParseExpect(TokenType.CLOSE_PAREN);
                return result;
            }

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

        public State DetermineToken()
        {

            while (true)
            {
                var c = next();

                if (IsWhiteSpace(c))
                {
                    ignore();
                }
                else if (c == '(')
                {
                    emit(TokenType.OPEN_PAREN);
                    ignore();
                }
                else if (c == ')')
                {
                    emit(TokenType.CLOSE_PAREN);
                    ignore();
                }
                else if (c == '"')
                {
                    return State.STRING;
                }
                else if (IsStartOfNumber(c))
                {
                    return State.NUMBER;
                }
                else if (c == char.MaxValue)
                {
                    return State.EOF;
                }
                else if (c == 'x' || c == 'y' || c == 't' || c == 'X' || c == 'Y' || c== 'T')
                {
                    emit(TokenType.VARIABLE);
                    ignore();
                }
                else
                {
                    return State.OP;
                }
            }
        }


        public State LexOp()
        {
            const string opchars = "+-/*abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            acceptRun(opchars);
            emit(TokenType.OP);
            return State.DETERMINE;

        }
        public State LexString()
        {
            ignore(); //skip the open "
            while (peek() != '"')
            {
                next();
            }
            emit(TokenType.STRING);
            next();
            ignore();

            return State.DETERMINE;
        }
        public State LexNumber()
        {
            const string numprefix = "-.";
            accept(numprefix);
            const string digits = "0123456789";
            acceptRun(digits);
            if (accept("."))
            {
                acceptRun(digits);
            }
            if (input[start] == '-' && (
                !IsStartOfNumber(input[start + 1])
                && input[start + 1] != '.'
                ))
            {
                emit(TokenType.OP);
            }
            else
            {
                emit(TokenType.CONSTANT);
            }
            return State.DETERMINE;
        }

        public bool accept(string valid)
        {

            if (valid.IndexOf(next()) >= 0)
            {
                return true;
            }
            backup();
            return false;
        }

        public void acceptRun(string valid)
        {
            while (valid.IndexOf(next()) >= 0) { }
            backup();
        }

        public void emit(TokenType type)
        {
            tokens.Enqueue(new Token { type = type, start = start, len = pos - start });
        }

        public char next()
        {
            if (pos < input.Length)
            {
                char c = input[pos];
                pos++;
                return c;
            }
            pos++;
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
