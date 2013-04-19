namespace SSBPD.Migrations
{
    using System.Data.Entity.Migrations;
    
    public partial class addEloTable : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "EloScores",
                c => new
                    {
                        EloScoreID = c.Int(nullable: false, identity: true),
                        PlayerID = c.Int(nullable: false),
                        Date = c.DateTime(nullable: false),
                        ELO = c.Double(nullable: false),
                    })
                .PrimaryKey(t => t.EloScoreID);
            
        }
        
        public override void Down()
        {
            DropTable("EloScores");
        }
    }
}
