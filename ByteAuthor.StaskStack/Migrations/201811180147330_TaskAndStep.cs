namespace ByteAuthor.StaskStack.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class TaskAndStep : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Steps",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Description = c.String(maxLength: 2147483647),
                        Order = c.Double(nullable: false),
                        IsDone = c.Boolean(nullable: false),
                        Task_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Tasks", t => t.Task_Id)
                .Index(t => t.Task_Id);
            
            CreateTable(
                "dbo.Tasks",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(maxLength: 2147483647),
                        Priority = c.Double(nullable: false),
                        IsActive = c.Boolean(nullable: false),
                        IsDone = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Steps", "Task_Id", "dbo.Tasks");
            DropIndex("dbo.Steps", new[] { "Task_Id" });
            DropTable("dbo.Tasks");
            DropTable("dbo.Steps");
        }
    }
}
