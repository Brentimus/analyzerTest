using System.Linq.Expressions;
using Lexer;

namespace Parser;

public partial class Parser
{
    public class StatementNode : Node
    {
    }

    public class CompoundStatementNode : StatementNode
    {
        public CompoundStatementNode(List<StatementNode> states)
        {
            States = states;
        }

        public List<StatementNode> States;
    }

    public class IfStatementNode : StatementNode
    {
        public IfStatementNode(ExpressionNode exp, StatementNode stateThen, StatementNode stateElse)
        {
            Exp = exp;
            StateThen = stateThen;
            StateElse = stateElse;
        }

        public ExpressionNode Exp { get; }
        public StatementNode StateThen { get; }
        public StatementNode StateElse { get; }

    }

    public class ForStatementNode : StatementNode
    {
        public ForStatementNode(IdNode id, ExpressionNode expFor, KeywordNode to, ExpressionNode expTo,
            StatementNode state)
        {
            Id = id;
            ExpFor = expFor;
            ExpTo = expTo;
            To = to;
            State = state;
        }

        public IdNode Id { get; }
        public ExpressionNode ExpFor { get; }
        public ExpressionNode ExpTo { get; }
        public KeywordNode To { get; }
        public StatementNode State { get; }

    }

    public class WhileStatementNode : StatementNode
    {
        public WhileStatementNode(ExpressionNode exp, StatementNode state)
        {
            Exp = exp;
            State = state;
        }

        public ExpressionNode Exp { get; }
        public StatementNode State { get; }
    }

    public class AssignmentStatementNode : StatementNode
    {
        public AssignmentStatementNode(ExpressionNode varRef, Lex op, ExpressionNode exp)
        {
            Exp = exp;
            VarRef = varRef;
            Op = op;
        }

        public ExpressionNode VarRef { get; }
        public Lex Op { get; }
        public ExpressionNode Exp { get; }
    }

    public class FunctionCallStatementNode : StatementNode
    {
        public FunctionCallStatementNode(IdNode id, List<IdNode> ids)
        {
            Ids = ids;
            Id = id;
        }

        public List<IdNode> Ids { get; }
        public IdNode Id { get; }
    }

    public StatementNode Statement()
    {
        var state = StructuredStatement() ?? SimpleStatement();
        return state;
    }

    public StatementNode SimpleStatement()
    {
        //TODO: function_call_statement
        var state = AssignmentStatement();
        return state;
    }

    public CompoundStatementNode CompoundStatement()
    {
        Require(LexKeywords.BEGIN);
        var states = StatementSequence();
        Require(LexKeywords.END);
        return new CompoundStatementNode(states);
    }

    public List<StatementNode> StatementSequence()
    {
        //TODO: Exception ?
        var states = new List<StatementNode>();
        while (true)
        {
            int checker = 0;
            while (_curLex.Is(LexSeparator.Semicolom))
            {
                Eat();
                checker++;
            }

            if (_curLex.Is(LexKeywords.END))
            {
                Eat();
                break;
            }

            if (states.Count > 0 && checker == 0)
                Require(LexSeparator.Semicolom);
            states.Add(Statement());
        }

        return states;
    }

    public StatementNode? StructuredStatement()
    {
        if (_curLex.Is(LexKeywords.BEGIN))
        {
            CompoundStatement();
        }

        if (_curLex.Is(LexKeywords.WHILE))
        {
            WhileStatement();
        }

        if (_curLex.Is(LexKeywords.FOR))
        {
            ForStatement();
        }

        if (_curLex.Is(LexKeywords.IF))
        {
            IfStatement();
        }

        return null;
    }

    public FunctionCallStatementNode FunctionCallStatement()
    {
        List<IdNode> ids = null;
        var id = Id();
        if (_curLex.Is(LexSeparator.Lparen))
        {
            Eat();
            ids = IdList();
            Require(LexSeparator.Rparen);
        }

        return new FunctionCallStatementNode(id, ids);
    }

    public AssignmentStatementNode AssignmentStatement()
    {
        var varRef = VarRef();
        Lex op;
        if (_curLex.Is(LexOperator.AssignSub, LexOperator.Assign, LexOperator.AssignAdd, LexOperator.AssignDiv,
                LexOperator.AssignMul))
        {
            op = _curLex; //TODO:fix?
        }
        else
        {
            throw new Exception("");
        }

        var exp = Expression();
        return new AssignmentStatementNode(varRef, op, exp);
    }
    public ForStatementNode ForStatement()
    {
        Eat();
        var id = Id();
        Require(LexOperator.Assign);
        var expFor = Expression();
        Require(_curLex.Is(LexKeywords.TO) ? LexKeywords.TO : LexKeywords.DOWNTO, false);
        var to = Keyword();
        var expTo = Expression();
        Require(LexKeywords.DO);
        var state = Statement();
        return new ForStatementNode(id, expFor, to, expTo, state);

    }
    public WhileStatementNode WhileStatement()
    {
        Eat();
        var exp = Expression();
        Require(LexKeywords.DO);
        var state = Statement();
        return new WhileStatementNode(exp, state);
    }
    public IfStatementNode IfStatement()
    {
        Eat();
        var exp = Expression();
        Require(LexKeywords.THEN);
        var stateThen = Statement();
        StatementNode stateElse = null;
        if (_curLex.Is(LexKeywords.ELSE))
        {
            Eat();
            stateElse = Statement();
        }

        return new IfStatementNode(exp, stateThen, stateElse);
    }
}