namespace QuizApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Quiz_VariableLinking : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.VariableInQuiz",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        VariableId = c.Int(nullable: false),
                        QuizId = c.Int(nullable: false),
                        NumberOfUses = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Variables", t => t.VariableId, cascadeDelete: true)
                .Index(t => t.VariableId);
            
            CreateTable(
                "dbo.Variables",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.VariablesDetails",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        LeadId = c.Int(nullable: false),
                        VariableInQuizId = c.Int(nullable: false),
                        VariableValue = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.VariableInQuiz", t => t.VariableInQuizId, cascadeDelete: true)
                .Index(t => t.VariableInQuizId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.VariablesDetails", "VariableInQuizId", "dbo.VariableInQuiz");
            DropForeignKey("dbo.VariableInQuiz", "VariableId", "dbo.Variables");
            DropIndex("dbo.VariablesDetails", new[] { "VariableInQuizId" });
            DropIndex("dbo.VariableInQuiz", new[] { "VariableId" });
            DropTable("dbo.VariablesDetails");
            DropTable("dbo.Variables");
            DropTable("dbo.VariableInQuiz");
        }
    }
}
