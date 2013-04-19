namespace SSBPD.Migrations
{
    using System.Data.Entity.Migrations;
    
    public partial class addFlags : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "PlayerFlags",
                c => new
                    {
                        PlayerFlagID = c.Int(nullable: false, identity: true),
                        PlayerID = c.Int(nullable: false),
                        toPlayerID = c.Int(),
                        userID = c.Int(nullable: false),
                        newTag = c.String(nullable: false, maxLength: 4000),
                    })
                .PrimaryKey(t => t.PlayerFlagID);
            
        }
        
        public override void Down()
        {
            DropTable("PlayerFlags");
        }
    }
}
