namespace QuizApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Answer_ObjectFields : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ObjectFieldsInAnswer",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ObjectName = c.String(),
                        FieldName = c.String(),
                        Value = c.String(),
                        AnswerOptionsInQuizQuestionsId = c.Int(nullable: false),
                        CreatedOn = c.DateTime(nullable: false),
                        CreatedBy = c.Int(nullable: false),
                        LastUpdatedOn = c.DateTime(),
                        LastUpdatedBy = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AnswerOptionsInQuizQuestions", t => t.AnswerOptionsInQuizQuestionsId, cascadeDelete: true)
                .Index(t => t.AnswerOptionsInQuizQuestionsId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ObjectFieldsInAnswer", "AnswerOptionsInQuizQuestionsId", "dbo.AnswerOptionsInQuizQuestions");
            DropIndex("dbo.ObjectFieldsInAnswer", new[] { "AnswerOptionsInQuizQuestionsId" });
            DropTable("dbo.ObjectFieldsInAnswer");
        }
    }
}
