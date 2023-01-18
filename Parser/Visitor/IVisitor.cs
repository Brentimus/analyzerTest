using Parser.Sym;

namespace Parser;

public interface IVisitor
{
    
    //programm
    void Visit(Parser.ProgramNode node);
    
    //expression
    void Visit(Parser.BinOpExpressionNode node);
    //void Visit(CastNode node);
    void Visit(Parser.UnOpExpressionNode node);
    void Visit(Parser.RecordAccess node);
    void Visit(Parser.ArrayAccess node);
    void Visit(Parser.CallNode node);
    void Visit(Parser.WriteCallNode node);
    void Visit(Parser.ReadCallNode node);

    void Visit(Parser.NumberExpressionNode node);
    void Visit(Parser.StringNode node);
    void Visit(Parser.BooleanNode node);

    void Visit(Parser.IdNode node);
    void Visit(SymInteger node);
    void Visit(SymDouble node);
    void Visit(SymChar node);
    void Visit(SymString node);
    void Visit(SymBoolean node);

    // declaration nodes visit
    void Visit(Parser.BlockNode node);
    void Visit(Parser.ConstDeclsNode node);
    void Visit(Parser.VarDeclsNode node);
    void Visit(Parser.TypeDeclsNode node);
    void Visit(SymFunction node);
    void Visit(SymProcedure node);
    void Visit(Parser.ParameterNode node);
    void Visit(SymAlias node);
    void Visit(Parser.KeywordNode node);

    // statement nodes visit
    void Visit(Parser.CompoundStatementNode node);
    //void Visit(EmptyStmtNode node);
    void Visit(Parser.FunctionCallStatementNode node);
    void Visit(Parser.AssignmentStatementNode node);
    void Visit(Parser.IfStatementNode node);
    void Visit(Parser.WhileStatementNode node);
    //void Visit(RepeatStmtNode node);
    void Visit(Parser.ForStatementNode node);
    //void Visit(ForRangeNode node);

    // type nodes visit
    void Visit(SymType node);
    void Visit(SymArray node);
    //void Visit(SubrangeTypeNode node);
    void Visit(SymRecord node);
    void Visit(Parser.TypeRangeNode node);
    void Visit(Parser.FieldSelectionNode node);
    void Visit(Parser.ConstDeclNode node);
    void Visit(Parser.VarDeclNode node);
    void Visit(Parser.ProcDelcNode node);
    void Visit(Parser.FuncDelcNode node);
    void Visit(Parser.StatementNode node);
}