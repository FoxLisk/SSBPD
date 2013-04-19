namespace SSBPD.Migrations
{
    using System.Data.Entity.Migrations;
    
    public partial class InitialCreate : DbMigration
    {
        public override void Up()
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
                        Tournament_TournamentID = c.Int(),
                        Player_PlayerId = c.Int(),
                    })
                .PrimaryKey(t => t.GameID)
                .ForeignKey("Tournaments", t => t.Tournament_TournamentID)
                .ForeignKey("Sets", t => t.SetID, cascadeDelete: true)
                .ForeignKey("Players", t => t.Player_PlayerId)
                .Index(t => t.Tournament_TournamentID)
                .Index(t => t.SetID)
                .Index(t => t.Player_PlayerId);
            
            CreateTable(
                "Sets",
                c => new
                    {
                        SetID = c.Int(nullable: false, identity: true),
                        WinnerID = c.Int(nullable: false),
                        LoserID = c.Int(nullable: false),
                        TournamentID = c.Int(nullable: false),
                        BracketName = c.String(nullable: false, maxLength: 4000),
                        Round = c.Int(),
                        IsWinners = c.Boolean(),
                        isPool = c.Boolean(nullable: false),
                        BestOf = c.Int(),
                        Losses = c.Int(),
                        PoolNum = c.Int(),
                        DatePlayed = c.DateTime(nullable: false),
                        Player_PlayerId = c.Int(),
                    })
                .PrimaryKey(t => t.SetID)
                .ForeignKey("Tournaments", t => t.TournamentID, cascadeDelete: true)
                .ForeignKey("Players", t => t.Player_PlayerId)
                .Index(t => t.TournamentID)
                .Index(t => t.Player_PlayerId);
            
            CreateTable(
                "Tournaments",
                c => new
                    {
                        TournamentID = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 4000),
                        Date = c.DateTime(nullable: false),
                        locked = c.Boolean(nullable: false),
                        eloProcessed = c.Boolean(nullable: false),
                        TournamentGuid = c.String(nullable: false, maxLength: 4000),
                    })
                .PrimaryKey(t => t.TournamentID);
            
            CreateTable(
                "Players",
                c => new
                    {
                        PlayerId = c.Int(nullable: false, identity: true),
                        firstName = c.String(maxLength: 4000),
                        lastName = c.String(maxLength: 4000),
                        Tag = c.String(nullable: false, maxLength: 4000),
                        ELO = c.Double(nullable: false),
                    })
                .PrimaryKey(t => t.PlayerId);
            
            CreateTable(
                "Users",
                c => new
                    {
                        UserID = c.Int(nullable: false, identity: true),
                        username = c.String(nullable: false, maxLength: 4000),
                        password = c.String(nullable: false, maxLength: 4000),
                        salt = c.String(nullable: false, maxLength: 4000),
                        authorizedUploader = c.Boolean(nullable: false),
                        isAdmin = c.Boolean(nullable: false),
                        email = c.String(nullable: false, maxLength: 4000),
                    })
                .PrimaryKey(t => t.UserID);
            
            CreateTable(
                "TournamentFiles",
                c => new
                    {
                        TournamentFileID = c.Int(nullable: false, identity: true),
                        XML = c.String(nullable: false),
                        OriginalFileName = c.String(nullable: false, maxLength: 4000),
                        Processed = c.Boolean(nullable: false),
                        ProcessedAt = c.DateTime(),
                        Inserted = c.DateTime(nullable: false),
                        AuthorizedForProcessing = c.Boolean(nullable: false),
                        TournamentGuid = c.String(nullable: false, maxLength: 4000),
                    })
                .PrimaryKey(t => t.TournamentFileID);
            
        }
        
        public override void Down()
        {
            DropIndex("Sets", new[] { "Player_PlayerId" });
            DropIndex("Sets", new[] { "TournamentID" });
            DropIndex("Games", new[] { "Player_PlayerId" });
            DropIndex("Games", new[] { "SetID" });
            DropIndex("Games", new[] { "Tournament_TournamentID" });
            DropForeignKey("Sets", "Player_PlayerId", "Players");
            DropForeignKey("Sets", "TournamentID", "Tournaments");
            DropForeignKey("Games", "Player_PlayerId", "Players");
            DropForeignKey("Games", "SetID", "Sets");
            DropForeignKey("Games", "Tournament_TournamentID", "Tournaments");
            DropTable("TournamentFiles");
            DropTable("Users");
            DropTable("Players");
            DropTable("Tournaments");
            DropTable("Sets");
            DropTable("Games");
        }
    }
}
