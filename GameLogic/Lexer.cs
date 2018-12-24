using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;



namespace GameLogic
{
    public enum TokenType : byte { OPEN_PAREN, CLOSE_PAREN, OP, STRING, CONSTANT}
    public enum State : byte { DETERMINE, OP, NUMBER, STRING,EOF}

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

        public AptNode stringToNode(string s)
        {            
            NodeType type;
            if (nodeDict.TryGetValue(s, out type)) {
                return AptNode.MakeNode(type);
            } else {
                throw new Exception("invalid string:" + s);
            }                       
            
        }

        public Pic ParsePic(GraphicsDevice g, GameWindow w)
        {
            while (true)
            {
                var t = tokens.Dequeue();
                switch (t.type)
                {
                    case TokenType.OP:
                        {
                            var s = input.Slice(t.start, t.len).ToString().ToLower();

                            if (s == "gradient")
                            {
                                Pic p = new Pic(PicType.GRADIENT, g, w);
                                p.Trees[0] = ParseNodes();
                                p.Machines[0] = new StackMachine(p.Trees[0]);
                                return p;
                            }
                            else if (s == "rgb")
                            {
                                Pic p = new Pic(PicType.RGB, g, w);
                                p.Trees[0] = ParseNodes();
                                p.Machines[0] = new StackMachine(p.Trees[0]);

                                p.Trees[1] = ParseNodes();
                                p.Machines[1] = new StackMachine(p.Trees[1]);

                                p.Trees[2] = ParseNodes();
                                p.Machines[2] = new StackMachine(p.Trees[2]);
                                return p;
                            }
                            else if (s == "hsv")
                            {
                                Pic p = new Pic(PicType.HSV, g, w);
                                p.Trees[0] = ParseNodes();
                                p.Machines[0] = new StackMachine(p.Trees[0]);

                                p.Trees[1] = ParseNodes();
                                p.Machines[1] = new StackMachine(p.Trees[1]);

                                p.Trees[2] = ParseNodes();
                                p.Machines[2] = new StackMachine(p.Trees[2]);
                                return p;
                            }
                            else
                            {
                                throw new Exception("inavalid tokentype:" + s);
                            }

                        }
                    case TokenType.CONSTANT:
                        {
                            throw new Exception("inavalid tokentype:" + t);
                        }
                    case TokenType.CLOSE_PAREN:
                    case TokenType.OPEN_PAREN:
                        continue;
                }
            }
        }

        public AptNode ParseNodes()
        {
            while(true)
            {
                var t = tokens.Dequeue();
                switch (t.type) {
                    case TokenType.OP:
                        {
                            var s = input.Slice(t.start, t.len).ToString().ToLower();                           
                            var node = stringToNode(s);
                            if (node.children != null)
                            {
                                if (node.type == NodeType.PICTURE)
                                {
                                    var filenameToken = tokens.Dequeue();
                                    if (filenameToken.type != TokenType.STRING)
                                    {
                                        throw new Exception("Picture did not have a file name");
                                    }
                                    node.filename = input.Slice(filenameToken.start, filenameToken.len).ToString();

                                }
                                for (int i = 0; i < node.children.Length; i++)
                                {
                                    node.children[i] = ParseNodes();
                                }
                            }
                            return node;                                                       
                        }
                    case TokenType.CONSTANT:
                        {
                            var node = new AptNode { type = NodeType.CONSTANT };
                            string numStr = input.Slice(t.start, t.len).ToString();
                            node.value = float.Parse(numStr);
                            return node;
                        }
                    
                    case TokenType.CLOSE_PAREN:
                    case TokenType.OPEN_PAREN:
                        continue;                    
                }
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
        public State LexNumber() {
            const string numprefix = "-.";
            accept(numprefix);
            const string digits = "0123456789";
            acceptRun(digits);
            if (accept(".")) {
                acceptRun(digits);
            }
            if (input[start] == '-' && (
                !IsStartOfNumber(input[start+1])
                && input[start+1] != '.'
                )) {
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
            tokens.Enqueue(new Token { type = type, start = start, len = pos-start});
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
