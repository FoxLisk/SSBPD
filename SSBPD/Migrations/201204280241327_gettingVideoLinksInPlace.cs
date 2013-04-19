namespace SSBPD.Migrations
{
    using System.Data.Entity.Migrations;
    
    public partial class gettingVideoLinksInPlace : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("Games", "SetID", "Sets");
            DropForeignKey("Games", "Player_PlayerId", "Players");
            DropForeignKey("Sets", "Player_PlayerId", "Players");
            DropIndex("Games", new[] { "SetID" });
            DropIndex("Games", new[] { "Player_PlayerId" });
            DropIndex("Sets", new[] { "Player_PlayerId" });
            CreateTable(
                "SetLinks",
                c => new
                    {
                        SetLinkID = c.Int(nullable: false, identity: true),
                        SetID = c.Int(nullable: false),
                        Title = c.String(nullable: false, maxLength: 4000),
                        URL = c.String(nullable: false, maxLength: 4000),
                        UserID = c.Int(nullable: false),
                        Deleted = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.SetLinkID);
            
            DropColumn("Sets", "Player_PlayerId");
            DropTable("Games");
        }
        
        public override void Down()
        {
            CreateTable(
                "Games",
                c => new
                    {
                        GameID = c.Int(nullable: false, identity: true),
                        SetID = c.Int(nullable: false),
                        WinnerID = c.Int(nullable: false),
                        LoserID = c.Int(nullable: false),
                        date = c.DateTime(nullable: false),
                        Player_PlayerId = c.Int(),
                    })
                .PrimaryKey(t => t.GameID);
            
            AddColumn("Sets", "Player_PlayerId", c => c.Int());
            DropTable("SetLinks");
            CreateIndex("Sets", "Player_PlayerId");
            CreateIndex("Games", "Player_PlayerId");
            CreateIndex("Games", "SetID");
            AddForeignKey("Sets", "Player_PlayerId", "Players", "PlayerId");
            AddForeignKey("Games", "Player_PlayerId", "Players", "PlayerId");
            AddForeignKey("Games", "SetID", "Sets", "SetID", cascadeDelete: true);
        }
    }
}
