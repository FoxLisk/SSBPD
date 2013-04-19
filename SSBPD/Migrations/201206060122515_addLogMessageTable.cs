namespace SSBPD.Migrations
{
    using System.Data.Entity.Migrations;
    
    public partial class addLogMessageTable : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "LogMessages",
                c => new
                    {
                        LogMessageID = c.Int(nullable: false, identity: true),
                        Date = c.DateTime(nullable: false),
                        Message = c.String(nullable: false, maxLength: 4000),
                    })
                .PrimaryKey(t => t.LogMessageID);
            
        }
        
        public override void Down()
        {
            DropTable("LogMessages");
        }
    }
}
