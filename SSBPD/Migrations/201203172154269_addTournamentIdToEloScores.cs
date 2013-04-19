namespace SSBPD.Migrations
{
    using System.Data.Entity.Migrations;
    
    public partial class addTournamentIdToEloScores : DbMigration
    {
        public override void Up()
        {
            AddColumn("EloScores", "TournamentID", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("EloScores", "TournamentID");
        }
    }
}
