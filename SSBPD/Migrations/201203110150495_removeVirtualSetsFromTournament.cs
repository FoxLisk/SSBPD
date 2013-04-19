namespace SSBPD.Migrations
{
    using System.Data.Entity.Migrations;
    
    public partial class removeVirtualSetsFromTournament : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("Games", "Tournament_TournamentID", "Tournaments");
            DropIndex("Games", new[] { "Tournament_TournamentID" });
            DropColumn("Games", "Tournament_TournamentID");
        }
        
        public override void Down()
        {
            AddColumn("Games", "Tournament_TournamentID", c => c.Int());
            CreateIndex("Games", "Tournament_TournamentID");
            AddForeignKey("Games", "Tournament_TournamentID", "Tournaments", "TournamentID");
        }
    }
}
